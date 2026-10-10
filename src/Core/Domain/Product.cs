using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    private int _quantity;

    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public int Quantity => _quantity;

    // Приватний конструктор: захист від створення об'єкта в обхід перевірок
    private Product(string id, string sku, string name, string unit, int quantity)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        _quantity = quantity;
    }

    // Фабричний метод: перевіряє всі вхідні дані до створення сутності
    public static Product Create(string id, string sku, string name, string unit, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU товару не може бути порожнім", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(name));

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Початковий залишок товару не може бути від'ємним");

        return new Product(id.Trim(), sku.Trim().ToUpperInvariant(), name.Trim(), unit?.Trim() ?? "шт", quantity);
    }

    // Бізнес-метод приходу товару
    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість надходження має бути більшою за нуль");

        _quantity += amount;
    }

    // Бізнес-метод видачі (списання) товару
    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість видачі має бути більшою за нуль");

        if (amount > _quantity)
            throw new InvalidOperationException($"Не можна видати {amount} одиниць: залишок товару [{Sku}] становить лише {_quantity}");

        _quantity -= amount;
    }

    // Мапінг у DTO тижня 3 і назад для майбутнього сховища
    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);

    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);

    public override string ToString() => $"{Id} [{Sku}] {Name} - {Quantity} {Unit}";
}