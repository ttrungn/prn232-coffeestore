using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class UpdateMenuRequest : BaseServiceRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public DateTime? FromDate { get; set; }

    [Required]
    public DateTime? ToDate { get; set; }

    [Required]
    public List<MenuProductRequest> Products { get; set; } = new List<MenuProductRequest>();
}
