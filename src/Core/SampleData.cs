using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 120),
        Product.Create("P-002", "SKU-002", "Пісок річковий 50кг", "міш", 65),
        Product.Create("P-003", "SKU-003", "Цегла червона рядова", "шт", 1400),
        Product.Create("P-004", "SKU-004", "Шпаклівка фінішна 20кг", "відро", 30),
        Product.Create("P-005", "SKU-005", "Гіпсокартон 12.5мм", "лист", 85),
        Product.Create("P-006", "SKU-006", "Профіль CD-60 3м", "шт", 150),
        Product.Create("P-007", "SKU-007", "Саморізи по металу 25мм", "уп", 40),
        Product.Create("P-008", "SKU-008", "Грунтовка глибокого проникнення", "каністра", 22),
        Product.Create("P-009", "SKU-009", "Фарба фасадна біла 10л", "відро", 18),
        Product.Create("P-010", "SKU-010", "Клей для плитки 25кг", "міш", 90),
        Product.Create("P-011", "SKU-011", "Утеплювач мінвата 100мм", "рул", 35),
        Product.Create("P-012", "SKU-012", "Перфоратор SDS-Plus", "шт", 7),
        Product.Create("P-013", "SKU-013", "Болгарка 125мм 900Вт", "шт", 12),
        Product.Create("P-014", "SKU-014", "Рулетка вимірювальна 5м", "шт", 55),
        Product.Create("P-015", "SKU-015", "Кабель ВВГ-Пнг 3х2.5", "м", 320)
    ];
}