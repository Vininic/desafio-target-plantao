namespace TargetPlantao.Core.Common;

public sealed class DomainException(DomainError error) : Exception(error.ToString())
{
    public DomainError Error { get; } = error;
}
