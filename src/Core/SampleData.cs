namespace Core;

using Core.Domain;

public static class SampleData
{
    public static List<Product> Products() => new()
    {
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "мішок", 100),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 15),
        Product.Create("P-003", "SKU-003", "Цегла червона рядова", "шт", 1500),
        Product.Create("P-004", "SKU-004", "Ґрунтовка глибока 10л", "каністра", 40),
        Product.Create("P-005", "SKU-005", "Фарба фасадна біла 14кг", "відро", 25),
        Product.Create("P-006", "SKU-006", "Шпаклівка фінішна 25кг", "мішок", 60),
        Product.Create("P-007", "SKU-007", "Гіпсокартон стіновий 2.5м", "лист", 80),
        Product.Create("P-008", "SKU-008", "Профіль CD-60 3м", "шт", 120),
        Product.Create("P-009", "SKU-009", "Саморізи по металу 25мм", "пачка", 50),
        Product.Create("P-010", "SKU-010", "Мінвата 100мм 5м2", "рулон", 30),
        Product.Create("P-011", "SKU-011", "Піна монтажна 750мл", "балон", 45),
        Product.Create("P-012", "SKU-012", "Силікон прозорий 310мл", "тюбик", 35),
        Product.Create("P-013", "SKU-013", "Клей для плитки 25кг", "мішок", 70),
        Product.Create("P-014", "SKU-014", "Пластифікатор для бетону 5л", "каністра", 20),
        Product.Create("P-015", "SKU-015", "Рукавиці робочі", "пара", 200)
    };
}