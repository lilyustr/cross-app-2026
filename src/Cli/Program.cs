using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

var sysInfo = new 
{
    App = "CrossApp - практикум з крос-платформного програмування",
    Student = "Устрицька Лілія, група ФЕІ-32.2",
    OsDescription = RuntimeInformation.OSDescription,
    OsEnvironment = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    ClrVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад (Product, StockBatch, Warehouse, Movement)"
};

if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions 
    { 
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
    };
    string jsonOutput = JsonSerializer.Serialize(sysInfo, new JsonSerializerOptions { WriteIndented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    Console.WriteLine(jsonOutput);
}
else
{
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

}




