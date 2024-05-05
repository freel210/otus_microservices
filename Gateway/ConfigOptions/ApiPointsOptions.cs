using System.ComponentModel.DataAnnotations;

namespace Gateway.ConfigOptions;

public class ApiPointsOptions
{
    [Required]
    public string? AuthUrl { get; set; }

    [Required]
    public string? DemoUrl { get; set; }

    [Required]
    public string? BillingUrl { get; set; }

    [Required]
    public string? OrdersUrl { get; set; }
}
