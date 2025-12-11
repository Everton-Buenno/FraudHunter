using FraudHunter.Core.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

Console.WriteLine("=== Iniciando Treinamento do Fraud Hunter ===");
var mlContext = new MLContext(seed: 2025);

var dataPath = Path.Combine(Environment.CurrentDirectory, "creditcard.csv");

Console.WriteLine($"Lendo dados de: {dataPath}");

IDataView dataView = mlContext.Data.LoadFromTextFile<TransactionData>(
    path: dataPath,
    hasHeader: true,
    separatorChar: ',',     
    allowQuoting: true,     
    trimWhitespace: true);  


var trainData = mlContext.Data.FilterRowsByColumn(dataView, "Class", lowerBound: 0, upperBound: 0.9);
var pipeline = mlContext.Transforms.Concatenate("Features", "V1", "V2", "V3", "V4", "V10", "V14", "Amount")


    .Append(mlContext.Transforms.NormalizeMeanVariance("Features"))
    .Append(mlContext.AnomalyDetection.Trainers.RandomizedPca(
        featureColumnName: "Features",
        exampleWeightColumnName: null,
        rank: 5,
        ensureZeroMean: false
    ));
Console.WriteLine("Treinando o modelo (Isso pode levar alguns segundos)...");
var model = pipeline.Fit(trainData);
var outputPath = Path.Combine(Environment.CurrentDirectory, "fraud_model.zip");
mlContext.Model.Save(model, trainData.Schema, outputPath);
Console.WriteLine($"✅ Modelo salvo com sucesso em: {outputPath}");
