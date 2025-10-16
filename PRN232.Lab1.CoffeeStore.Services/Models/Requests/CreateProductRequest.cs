using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class CreateProductRequest : BaseServiceRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be >= 0.")]
    public decimal Price { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    [Required]
    public Guid CategoryId { get; set; }
}
