using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    // Метод Products(), який повертає список (з дужками, щоб StoreFactory міг його викликати)
    public static List<Product> Products() => GetProducts();
    public static List<Product> GetProducts()
    {
        return new List<Product>
        {
            // Одиниця: шт (є дефіцитні позиції < 10)
            Product.Create("p1", "SKU-001", "Ноутбук Lenovo", "шт", 5),
            Product.Create("p2", "SKU-002", "Мишка бездротова", "шт", 45),
            Product.Create("p3", "SKU-003", "Клавіатура механічна", "шт", 12),
            Product.Create("p4", "SKU-004", "Монітор 27\"", "шт", 8),
            Product.Create("p5", "SKU-005", "Кабель HDMI 2m", "шт", 120),
            Product.Create("p6", "SKU-006", "Веб-камера Full HD", "шт", 3),

            // Одиниця: кг
            Product.Create("p7", "SKU-007", "Кава в зернах Arabica", "кг", 25),
            Product.Create("p8", "SKU-008", "Чай чорний байховий", "кг", 15),
            Product.Create("p9", "SKU-009", "Цукор білий", "кг", 80),
            Product.Create("p10", "SKU-010", "Борошно пшеничне", "кг", 50),

            // Одиниця: л
            Product.Create("p11", "SKU-011", "Вода мінеральна", "л", 200),
            Product.Create("p12", "SKU-012", "Сік яблучний", "л", 35),
            Product.Create("p13", "SKU-013", "Олія соняшникова", "л", 60),

            // Одиниця: уп
            Product.Create("p14", "SKU-014", "Папір А4 (500 арк)", "уп", 40),
            Product.Create("p15", "SKU-015", "Маркери для дошки", "уп", 18),
            Product.Create("p16", "SKU-016", "Скріпки канцелярські", "уп", 90),
            Product.Create("p17", "SKU-017", "Файли прозорі А4", "уп", 110)
        };
    }
}