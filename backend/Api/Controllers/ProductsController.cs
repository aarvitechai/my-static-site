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
    public ActionResult<IEnumerable<ProductDto>> GetAll() => Ok(service.GetAll());

    // GET /api/products/5
    [HttpGet("{id:int}")]
    public ActionResult<ProductDto> GetById(int id)
    {
        var product = service.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }

    // POST /api/products
    [HttpPost]
    public ActionResult<ProductDto> Create(CreateProductDto dto)
    {
        var created = service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/products/5
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, UpdateProductDto dto) =>
        service.Update(id, dto) ? NoContent() : NotFound();

    // DELETE /api/products/5
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) =>
        service.Delete(id) ? NoContent() : NotFound();
}
