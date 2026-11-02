using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> Products() => new()
    {
        Product.Create("p001", "SKU-001", "Кабель UTP cat6", "м", 120),
        Product.Create("p002", "SKU-002", "Автоматичний вимикач 16A", "шт", 45),
        Product.Create("p003", "SKU-003", "Розетка подвійна із заземленням", "шт", 80),
        Product.Create("p004", "SKU-004", "Світлодіодна панель 36W", "шт", 15),
        Product.Create("p005", "SKU-005", "Дюбель-цвях 6х40", "упак", 200),
        Product.Create("p006", "SKU-006", "Саморізи по металу 3.5х25", "упак", 150),
        Product.Create("p007", "SKU-007", "Клей для плитки Ceresit CM11", "мішок", 40),
        Product.Create("p008", "SKU-008", "Ґрунтовка глибокого проникнення", "каністра", 8),
        Product.Create("p009", "SKU-009", "Фарба фасадна біла 10л", "відро", 5),
        Product.Create("p010", "SKU-010", "Шпаклівка фінішна 25кг", "мішок", 30),
        Product.Create("p011", "SKU-011", "Гіпсокартон стельовий 12.5мм", "шт", 60),
        Product.Create("p012", "SKU-012", "Перфоратор SDS-Plus", "шт", 4),
        Product.Create("p013", "SKU-013", "Болгарка 125мм 900Вт", "шт", 12),
        Product.Create("p014", "SKU-014", "Плівка поліетиленова 150мкм", "рулон", 25),
        Product.Create("p015", "SKU-015", "Цвяхи будівельні 100мм", "кг", 90),
        Product.Create("p016", "SKU-016", "Анкерний болт 10х80", "шт", 3),
        Product.Create("p017", "SKU-017", "Герметик силіконовий прозорий", "шт", 22)
    };
}