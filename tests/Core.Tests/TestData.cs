using System.Collections.Generic;
using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static IReadOnlyList<Product> Items() => new List<Product>
    {
        Product.Create("id-1", "SKU-MID", "Товар 1", "шт", 5),  // < 10
        Product.Create("id-2", "SKU-BIG", "Товар 2", "шт", 50), // >= 10
        Product.Create("id-3", "SKU-LOW", "Товар 3", "шт", 2)   // < 10 (і < 1)
    };
}