namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class MenuResponse
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}