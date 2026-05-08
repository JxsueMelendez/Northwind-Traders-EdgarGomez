
namespace Northwind.Tests;

public class CompanyConfigTest
{
    [Fact]
    public void GetCompanyName_ShouldReturnNorthwindTraders()
    {
        var result = Northwind.Domain.CompanyConfig.GetCompanyName();

        Assert.Equal("Northwind Traders", result);
    }
}