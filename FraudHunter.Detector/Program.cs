using FraudHunter.Detector;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);


var metricServer = new KestrelMetricServer(port: 5000);
metricServer.Start();

Console.WriteLine("Servidor de Métricas rodando em localhost:5000/metrics");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();