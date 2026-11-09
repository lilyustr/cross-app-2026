using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default);

    // Потоковий контракт імпорту
    IAsyncEnumerable<Product> StreamAsync(string path, CancellationToken ct = default);
}