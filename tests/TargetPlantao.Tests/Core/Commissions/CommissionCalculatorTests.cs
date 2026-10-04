using TargetPlantao.Core.Commissions;

namespace TargetPlantao.Tests.Core.Commissions;

public class CommissionCalculatorTests
{
    private readonly CommissionCalculator _calculator = new(CommissionPolicy.Default);

    [Theory]
    [InlineData("90.75", "0")]
    [InlineData("100.00", "1.00")]
    [InlineData("250.30", "2.50")]
    [InlineData("499.99", "5.00")]
    [InlineData("500.00", "25.00")]
    [InlineData("950.75", "47.54")]
    public void Commission_is_rounded_to_cents_per_sale(string amount, string expected)
    {
        var result = _calculator.Calculate(new Sale("Vendedor", Dec(amount)));

        Assert.Equal(Dec(expected), result.Commission);
    }

    [Fact]
    public void Sales_are_grouped_by_seller_and_ranked_by_commission()
    {
        var summaries = _calculator.CalculateBySeller(
        [
            new Sale("Ana", 200m),
            new Sale("Bruno", 1000m),
            new Sale("Ana", 50m),
        ]);

        Assert.Collection(
            summaries,
            bruno =>
            {
                Assert.Equal("Bruno", bruno.Seller);
                Assert.Equal(50m, bruno.TotalCommission);
            },
            ana =>
            {
                Assert.Equal("Ana", ana.Seller);
                Assert.Equal(2, ana.Sales.Count);
                Assert.Equal(250m, ana.TotalSold);
                Assert.Equal(2m, ana.TotalCommission);
            });
    }

    [Fact]
    public void No_sales_produce_no_summaries()
    {
        Assert.Empty(_calculator.CalculateBySeller([]));
    }

    [Theory]
    [InlineData("João Silva", 10, "10754.70", "495.69")]
    [InlineData("Maria Souza", 9, "9874.30", "465.96")]
    [InlineData("Ana Lima", 9, "8763.95", "404.99")]
    [InlineData("Carlos Oliveira", 8, "7928.35", "379.38")]
    public void Challenge_dataset_produces_the_expected_commission_per_seller(
        string seller, int salesCount, string totalSold, string totalCommission)
    {
        using var json = Open("vendas.json");
        var summary = _calculator.CalculateBySeller(SalesJsonReader.Read(json)).Single(s => s.Seller == seller);

        Assert.Equal(salesCount, summary.Sales.Count);
        Assert.Equal(Dec(totalSold), summary.TotalSold);
        Assert.Equal(Dec(totalCommission), summary.TotalCommission);
    }
}
