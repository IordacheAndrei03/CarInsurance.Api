namespace CarInsurance.Api.Dtos;
using System.Text.Json.Serialization;


public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record CreateClaimRequest(DateOnly ClaimDate, string Description, decimal Amount);
public record ClaimResponse(long Id, long CarId, string ClaimDate, string Description, decimal Amount);

public record CarHistoryItem(
    string Type,            
    string Date,
    long CarId,

    // Policy
    long? PolicyId = null,
    string? PolicyStartDate = null,
    string? PolicyEndDate = null,

    // Claim
    long? ClaimId = null,
    string? Description = null,
    decimal? Amount = null
);

public record CreateCarRequest(
    string Vin,
    string? Make,
    string? Model,
    int YearOfManufacture,
    long OwnerId
);