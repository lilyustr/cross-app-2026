using System.Runtime.InteropServices;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("CrossApp - практика з крос-платформного програмування");
Console.WriteLine("Студент: Устрицька Лілія, група ФЕІ-32.2");
Console.WriteLine(new string ('-', 52));
Console.WriteLine($"OC (OSDescription): {RuntimeInformation.OSDescription}");
Console.WriteLine($"OC (Environment) : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу: {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR): {Environment.Version}");
Console.WriteLine($"Runtime            : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку: {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог  : {Environment.CurrentDirectory}");
Console.WriteLine(new string ('-', 52));
Console.WriteLine("Предметна область: Склад (Product, StockBatch, Warehouse, Movement)");



