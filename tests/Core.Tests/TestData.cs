using System.Collections.Generic;
using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Product> Items() => new()
    {
        Product.Create("t1", "SKU-BIG", "Товар 1", "шт", 100),
        Product.Create("t2", "SKU-MID", "Товар 2", "шт", 50),
        Product.Create("t3", "SKU-LOW", "Товар 3", "шт", 5)
    };
}