using Api.Dtos;

namespace Api.Services;

// The controller depends on this interface, not on a concrete class.
// Later you can swap the in-memory version for an EF Core / database version.
public interface IProductService
{
    IEnumerable<ProductDto> GetAll();
    ProductDto? GetById(int id);
    ProductDto Create(CreateProductDto dto);
    bool Update(int id, UpdateProductDto dto);
    bool Delete(int id);
}
