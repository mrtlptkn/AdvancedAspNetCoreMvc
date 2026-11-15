using ProductCatalog.Services;
using Xunit;

namespace ProductCatalog.Tests.Services;

public class CartTests
{
    [Fact]
    public void Add_NewProduct_AddsLine()
    {
        var cart = new Cart();

        cart.Add(1, 2);

        Assert.Single(cart.Items);
        Assert.Equal(2, cart.TotalQuantity);
    }

    [Fact]
    public void Add_SameProductTwice_SumsQuantity()
    {
        var cart = new Cart();

        cart.Add(1, 2);
        cart.Add(1, 3);

        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items[0].Quantity);
    }

    [Fact]
    public void Add_QuantityAboveLimit_CapsAtMax()
    {
        var cart = new Cart();

        cart.Add(1, 90);
        cart.Add(1, 50);

        Assert.Equal(Cart.MaxQuantityPerProduct, cart.Items[0].Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Add_InvalidQuantity_Throws(int quantity)
    {
        var cart = new Cart();

        Assert.Throws<ArgumentOutOfRangeException>(() => cart.Add(1, quantity));
    }

    [Fact]
    public void Remove_RemovesOnlyThatProduct()
    {
        var cart = new Cart();
        cart.Add(1, 1);
        cart.Add(2, 1);

        cart.Remove(1);

        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].ProductId);
    }
}
