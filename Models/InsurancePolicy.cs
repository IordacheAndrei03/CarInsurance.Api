namespace CarInsurance.Api.Models;

public class InsurancePolicy
{
    public bool ExpirationLogged { get; set; }
    public long Id { get; set; }

    public long CarId { get; set; }
    public Car Car { get; set; } = default!;

    public string? Provider { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; } //Am schimbat DateOnly? in DateOnly
}
