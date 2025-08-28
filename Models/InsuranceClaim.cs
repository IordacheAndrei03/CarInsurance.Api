using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Models;

public class InsuranceClaim
{
    public long Id { get; set; }

    [Required]
    public long CarId { get; set; }
    public Car Car { get; set; } = default!;

    [Required]
    public DateOnly ClaimDate { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = default!;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}
