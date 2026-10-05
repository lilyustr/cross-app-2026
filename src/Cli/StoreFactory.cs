namespace Cli;

using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cached");

        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        // 1. Створюємо базове сховище відповідно до аргументів
        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        // 2. Якщо користувач передав прапорець --cached, обгортаємо декоратором
        if (useCache)
        {
            store = new CachingCatalogStore(store);
        }

        return store;
    }
}