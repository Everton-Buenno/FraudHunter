using System.Text;
using System.Text.Json;
using FraudHunter.Core.Models;
using Microsoft.ML;
using Prometheus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FraudHunter.Detector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private PredictionEngine<TransactionData, FraudPrediction> _predictionEngine;


    private static readonly Counter TransactionsProcessed = Metrics
        .CreateCounter("fraud_transactions_total", "Total de transações processadas.");

    private static readonly Counter FraudsDetected = Metrics
        .CreateCounter("fraud_detected_total", "Total de transações classificadas como fraude.");

    private static readonly Histogram AnomalyScore = Metrics
        .CreateHistogram("fraud_anomaly_score", "Score de anomalia dado pelo modelo ML.NET.");

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        LoadModel();
    }

    private void LoadModel()
    {
        var mlContext = new MLContext();
        var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fraud_model.zip");

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"O modelo não foi encontrado em {modelPath}.");
        }

        ITransformer model = mlContext.Model.Load(modelPath, out var inputSchema);

        _predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionData, FraudPrediction>(model);
        _logger.LogInformation("Modelo carregado com sucesso!");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        const string queueName = "transaction_stream";

        await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var transaction = JsonSerializer.Deserialize<TransactionData>(message);

                if (transaction != null)
                {
                    var prediction = _predictionEngine.Predict(transaction);

                    TransactionsProcessed.Inc();
                    AnomalyScore.Observe(prediction.Score);
                    bool isFraud = prediction.PredictedLabel || transaction.Amount > 5000;

                    if (isFraud)
                    {
                        FraudsDetected.Inc();

                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        var reason = transaction.Amount > 5000 ? "REGRA DE VALOR" : "IA ML.NET";
                        _logger.LogWarning($" FRAUDE ({reason})! Valor: ${transaction.Amount:F2} | Score: {prediction.Score:F4}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        _logger.LogInformation($" Transação OK. Valor: ${transaction.Amount:F2} | Score: {prediction.Score:F4}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar mensagem: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}