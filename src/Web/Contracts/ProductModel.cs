using System.ComponentModel.DataAnnotations;

namespace Web.Contracts;

public sealed class ProductModel
{
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "Артикул обов'язковий.")]
    [StringLength(32, ErrorMessage = "Максимум 32 символи.")]
    public string Sku { get; set; } = "";

    [Required(ErrorMessage = "Назва обов'язкова.")]
    [StringLength(200, ErrorMessage = "Максимум 200 символів.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Одиниця виміру обов'язкова.")]
    [StringLength(20, ErrorMessage = "Максимум 20 символів.")]
    public string Unit { get; set; } = "";

    [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути від'ємною.")]
    public int Quantity { get; set; }
}

public sealed record PagedResponse<T>(List<T> Items, int Total);

public sealed record ApiResult(
    bool Ok,
    string? Message = null,
    Dictionary<string, string[]>? Errors = null);

public sealed record ValidationProblemResponse(
    string? Title,
    Dictionary<string, string[]>? Errors);