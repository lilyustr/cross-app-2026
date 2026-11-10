using System;
using System.IO;
using System.Reflection;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

namespace Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // 1. Визначаємо середовище
        var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

        // 2. Збираємо ланцюжок джерел конфігурації
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")
            .AddCommandLine(args)
            .Build();

        // 3. Біндимо конфігурацію у AppOptions
        var options = config.Get<AppOptions>() ?? new AppOptions();

        // 4. Підтримка прямої змінної CROSSAPP_DATA_PATH
        var legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
        var storageOptions = new StorageOptions
        {
            DataPath = !string.IsNullOrWhiteSpace(legacyDataPath) ? legacyDataPath : options.Storage.DataPath,
            FileName = options.Storage.FileName
        };

        // Розв meщуємо повний шлях до файлу відносно AppContext.BaseDirectory
        var fullPath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);
        Console.WriteLine($"Файл даних: {fullPath}");

        // 5. Завдання 6: Перевірка та друк інформації про ApiKey (лише довжина!)
        var apiKey = config["Integration:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("ApiKey: не заданий");
        }
        else
        {
            Console.WriteLine($"ApiKey: присутній, довжина {apiKey.Length}");
        }

        // 6. Створюємо сховище та перевіряємо аргументи командного рядка
        var store = new FileCatalogStore(fullPath);

        if (args.Length > 0 && args[0].Equals("--export", StringComparison.OrdinalIgnoreCase))
        {
            // Якщо каталог порожній — наповнюємо тестовими даними для першого експорту
            if (store.GetAll().Count == 0)
            {
                store.Add(Product.Create("p1", "SKU-101", "Кабель UTP cat6", "м", 75));
                store.Add(Product.Create("p2", "SKU-102", "Патчкорд 1м", "шт", 120));
                store.Add(Product.Create("p3", "SKU-103", "Комутатор 8-port", "шт", 15));
            }

            var items = store.GetAll();
            Console.WriteLine($"Експортовано {items.Count} записи.");
            return;
        }

        // За замовчуванням виводимо інформацію про наявні дані
        var count = store.GetAll().Count;
        Console.WriteLine($"У каталозі зберігається записів: {count}");
    }
}