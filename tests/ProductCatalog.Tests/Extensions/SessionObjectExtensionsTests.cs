using Microsoft.AspNetCore.Http;
using ProductCatalog.Services;
using ProductCatalog.Web.Extensions;
using Xunit;

namespace ProductCatalog.Tests.Extensions;

public class SessionObjectExtensionsTests
{
    [Fact]
    public void SetObject_ThenGetObject_RoundTripsCart()
    {
        var session = new FakeSession();
        var cart = new Cart();
        cart.Add(1, 2);
        cart.Add(3, 1);

        session.SetObject("Cart", cart);
        var loaded = session.GetObject<Cart>("Cart");

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.Items.Count);
        Assert.Equal(3, loaded.TotalQuantity);
    }

    [Fact]
    public void GetObject_MissingKey_ReturnsNull()
    {
        var session = new FakeSession();

        Assert.Null(session.GetObject<Cart>("Cart"));
    }

    [Fact]
    public void SetObject_StoresObjectAsJsonString()
    {
        var session = new FakeSession();

        session.SetObject("Cart", new Cart());

        Assert.Equal("{\"items\":[]}", session.GetString("Cart"));
    }

    // Test için basit bellek içi session
    private sealed class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_store.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = Array.Empty<byte>();
            return false;
        }
    }
}
