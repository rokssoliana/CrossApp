using System;

namespace Core;

/// <summary>
/// Виняток для представлення помилок бізнес-логіки та невалідних доменних даних.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}