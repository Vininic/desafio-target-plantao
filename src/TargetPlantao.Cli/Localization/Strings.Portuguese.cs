using TargetPlantao.Core.Common;

namespace TargetPlantao.Cli.Localization;

internal sealed partial record Strings
{
    public static Strings Portuguese { get; } = new()
    {
        Back = "← voltar",
        Quit = "sair",
        Unit = "un",
        MenuHint = screens => $"↑↓ navegar  ·  enter abrir  ·  1-{screens} atalho  ·  l english  ·  q sair",
        LoadFailed = reason => $"não foi possível carregar os dados: {reason}",
        Error = error => error switch
        {
            DomainError.ProductNotFound e => $"Produto {e.ProductCode} não encontrado.",
            DomainError.NonPositiveQuantity => "Quantidade deve ser maior que zero.",
            DomainError.MissingDescription => "Descrição obrigatória.",
            DomainError.InsufficientStock e => $"Estoque insuficiente: {e.Product} tem {e.Available} un.",
            DomainError.NonPositiveAmount => "Valor deve ser maior que zero.",
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null),
        },
        Commission = new()
        {
            Title = "Comissões de vendas",
            Summary = "comissão por vendedor",
            Section = "01 · comissões",
            Seller = "Vendedor",
            Sales = "Vendas",
            ByTier = "Por faixa",
            TotalSold = "Total vendido",
            Commission = "Comissão",
            Total = "Total",
            Sale = "Venda",
            Tier = "Faixa",
        },
        Inventory = new()
        {
            Title = "Movimentação de estoque",
            Summary = "entradas, saídas e saldo",
            Section = "02 · estoque",
            NewMovement = "lançar movimentação",
            History = "histórico",
            Code = "Código",
            Product = "Produto",
            Stock = "Estoque",
            Time = "Hora",
            Type = "Tipo",
            Quantity = "Qtde",
            Description = "Descrição",
            FinalStock = "Saldo final",
            Inbound = "entrada",
            Outbound = "saída",
            QuantityPrompt = "quantidade:",
            DescriptionPrompt = "descrição:",
            InvalidQuantity = "quantidade inválida",
            MissingDescription = "descrição obrigatória",
            NoMovements = "sem movimentações",
            Of = "de",
            Registered = id => $"movimentação #{id} registrada",
        },
        Interest = new()
        {
            Title = "Juros por atraso",
            Summary = rate => $"{rate} ao dia",
            Section = "03 · juros",
            Today = (date, rate) => $"hoje {date}  ·  {rate} ao dia",
            AmountPrompt = "valor:",
            DueDatePrompt = "vencimento (dd/mm/aaaa):",
            InvalidAmount = "valor inválido",
            InvalidDate = "data inválida",
            OriginalAmount = "valor original",
            DueDate = "vencimento",
            DaysOverdue = "dias em atraso",
            Formula = (rate, days) => $"juros ({rate} × {days} dias)",
            Interest = "juros",
            NoInterest = "sem juros",
            UpdatedTotal = "total atualizado",
            NewCalculation = "novo cálculo",
        },
    };
}
