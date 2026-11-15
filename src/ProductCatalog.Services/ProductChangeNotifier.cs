using Microsoft.Extensions.Primitives;

namespace ProductCatalog.Services;

// Ürünler değişince önbelleği geçersiz kılmak için bağımlılık belirteci (dependency token).
// Önbellek girdisi bu token'a bağlanır; NotifyChanged çağrılınca girdi otomatik silinir.
public class ProductChangeNotifier
{
    private readonly object _lock = new();
    private CancellationTokenSource _cts = new();

    public IChangeToken GetChangeToken()
    {
        lock (_lock)
        {
            return new CancellationChangeToken(_cts.Token);
        }
    }

    public void NotifyChanged()
    {
        CancellationTokenSource previous;

        lock (_lock)
        {
            previous = _cts;
            _cts = new CancellationTokenSource();
        }

        previous.Cancel();
    }
}
