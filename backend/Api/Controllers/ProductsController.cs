using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]                 // automatic model validation + 400 responses
[Route("api/[controller]")]     // => /api/products
public class ProductsController(IProductService service) : ControllerBase
{
    // GET /api/products
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll() => Ok(await service.GetAllAsync());

    // GET /api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    // POST /api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto) =>
        await service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    // DELETE /api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}
