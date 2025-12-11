using System.Text;
using System.Text.Json;
using FraudHunter.Core.Models;
using RabbitMQ.Client;

Console.WriteLine("===  INICIANDO SIMULADOR DE CARTÃO DE CRÉDITO ===");

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

const string queueName = "transaction_stream";

await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine($"Conectado ao RabbitMQ! Enviando transações para a fila '{queueName}'...");

var random = new Random();

while (true)
{
    var transaction = GenerateRandomTransaction(random);

    var messageBody = JsonSerializer.Serialize(transaction);
    var body = Encoding.UTF8.GetBytes(messageBody);

    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);

    var type = transaction.Amount > 5000 ? "⚠️  SUSPEITA" : "✅ Normal";
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Enviado: ${transaction.Amount:F2} | V1: {transaction.V1:F2} | {type}");

    await Task.Delay(500);
}

static TransactionData GenerateRandomTransaction(Random rng)
{
    bool isAnomaly = rng.NextDouble() > 0.90;

    if (isAnomaly)
    {
        return new TransactionData
        {
            Amount = rng.Next(10000, 50000), 
            V1 = 100.0f,
            V2 = -100.0f,
            V3 = 100.0f,
            V14 = -100.0f
        };
    }
    else
    {
        return new TransactionData
        {
            Amount = rng.Next(10, 500),
            V1 = (float)(rng.NextDouble() * 2 - 1),
            V2 = (float)(rng.NextDouble() * 2 - 1),
            V3 = (float)(rng.NextDouble() * 2 - 1),
            V4 = (float)(rng.NextDouble() * 2 - 1),
            V10 = (float)(rng.NextDouble() * 2 - 1),
            V14 = (float)(rng.NextDouble() * 2 - 1)
        };
    }
}