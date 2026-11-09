using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

// DTOs (Data Transfer Objects): the shapes the API sends and receives.
// Keeping them separate from Models lets the API contract change independently.

public record ProductDto(int Id, string Name, decimal Price, DateTime CreatedAt);

public record CreateProductDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Range(0.01, 1_000_000)] decimal Price);

public record UpdateProductDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Range(0.01, 1_000_000)] decimal Price);
