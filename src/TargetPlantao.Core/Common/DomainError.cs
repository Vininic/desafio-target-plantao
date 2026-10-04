namespace TargetPlantao.Core.Common;

public abstract record DomainError
{
    private DomainError()
    {
    }

    public sealed record ProductNotFound(int ProductCode) : DomainError;

    public sealed record NonPositiveQuantity : DomainError;

    public sealed record MissingDescription : DomainError;

    public sealed record InsufficientStock(string Product, int Available) : DomainError;

    public sealed record NonPositiveAmount : DomainError;
}
