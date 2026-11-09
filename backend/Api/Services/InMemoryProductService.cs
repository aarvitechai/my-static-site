using Api.Dtos;
using Api.Models;

namespace Api.Services;

// Stores data in a list in memory. Data resets every time the app restarts.
public class InMemoryProductService : IProductService
{
    private readonly List<Product> _products =
    [
        new() { Id = 1, Name = "Keyboard", Price = 49.99m },
        new() { Id = 2, Name = "Mouse", Price = 19.99m },
        new() { Id = 3, Name = "Monitor", Price = 199.00m },
    ];
    private int _nextId = 4;
    private readonly Lock _lock = new();

    public IEnumerable<ProductDto> GetAll()
    {
        lock (_lock) return _products.Select(ToDto).ToList();
    }

    public ProductDto? GetById(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return product is null ? null : ToDto(product);
        }
    }

    public ProductDto Create(CreateProductDto dto)
    {
        lock (_lock)
        {
            var product = new Product { Id = _nextId++, Name = dto.Name, Price = dto.Price };
            _products.Add(product);
            return ToDto(product);
        }
    }

    public bool Update(int id, UpdateProductDto dto)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is null) return false;
            product.Name = dto.Name;
            product.Price = dto.Price;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _products.RemoveAll(p => p.Id == id) > 0;
    }

    private static ProductDto ToDto(Product p) => new(p.Id, p.Name, p.Price, p.CreatedAt);
}
