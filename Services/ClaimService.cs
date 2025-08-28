using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;


public class ClaimService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<ClaimResponse> AddClaimAsync(long carId, CreateClaimRequest req)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var claim = new InsuranceClaim
        {
            CarId = carId,
            ClaimDate = req.ClaimDate,
            Description = req.Description,
            Amount = req.Amount
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        return new ClaimResponse(
            claim.Id,
            claim.CarId,
            claim.ClaimDate.ToString("yyyy-MM-dd"),
            claim.Description,
            claim.Amount
        );
    }

    public async Task<List<ClaimResponse>> ListByCarAsync(long carId)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Claims
            .AsNoTracking()
            .Where(c => c.CarId == carId)
            .OrderByDescending(c => c.ClaimDate) 
            .Select(c => new ClaimResponse(
                c.Id,
                c.CarId,
                c.ClaimDate.ToString("yyyy-MM-dd"),
                c.Description,
                c.Amount
            ))
            .ToListAsync();
    }
}
