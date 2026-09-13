using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Length > 0 && args[0] == "--json")
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    var jsonOutput = new
    {
        App = "CrossApp",
        Student = "Устрицька Лілія",
        Group = "ФЕІ-32.2",
        Environment = report
    };

    Console.WriteLine(JsonSerializer.Serialize(jsonOutput, options));
    return;
}

Console.WriteLine("CrossApp - практика з крос-платформного програмування");
Console.WriteLine("Студент: Устрицька Лілія, група ФЕІ-32.2");
Console.WriteLine(new string ('-', 52));
Console.WriteLine($"OC               : {report.OsDescription}");
Console.WriteLine($"Runtime          : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура      : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено)  : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET)   : {report.ReportedRid}");
Console.WriteLine($"Примітка збірки  : {report.BuildNote}");
Console.WriteLine($"Каталог          : {report.BaseDirectory}");
Console.WriteLine(new string ('-', 52));
Console.WriteLine("Предметна область: Склад (Product, StockBatch, Warehouse, Movement)");





