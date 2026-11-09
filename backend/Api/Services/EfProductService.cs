using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

// EF Core implementation. Works the same against SQL Server or SQLite.
public class EfProductService(AppDbContext db) : IProductService
{
    public Task<List<ProductDto>> GetAllAsync() =>
        db.Products
            .AsNoTracking()                       // read-only query: skip change tracking
            .OrderBy(p => p.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.CreatedAt))
            .ToListAsync();

    public Task<ProductDto?> GetByIdAsync(int id) =>
        db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.CreatedAt))
            .FirstOrDefaultAsync();

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product { Name = dto.Name, Price = dto.Price };
        db.Products.Add(product);
        await db.SaveChangesAsync();             // INSERT; the database assigns Id
        return new ProductDto(product.Id, product.Name, product.Price, product.CreatedAt);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return false;
        product.Name = dto.Name;                 // EF tracks these changes...
        product.Price = dto.Price;
        await db.SaveChangesAsync();             // ...and sends an UPDATE here
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await db.Products.Where(p => p.Id == id).ExecuteDeleteAsync() > 0;   // single DELETE statement
}
