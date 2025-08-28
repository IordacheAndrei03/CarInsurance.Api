using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate >= date) // Am sters p.EndDate == null 
        );
    }

    public async Task<List<CarHistoryItem>> GetCarHistoryAsync(long carId)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var policies = await _db.Policies
            .AsNoTracking()
            .Where(p => p.CarId == carId)
            .Select(p => new { p.Id, p.StartDate, p.EndDate })
            .ToListAsync();

        var claims = await _db.Claims
            .AsNoTracking()
            .Where(c => c.CarId == carId)
            .Select(c => new { c.Id, c.ClaimDate, c.Description, c.Amount })
            .ToListAsync();

        var combined = new List<(DateOnly Key, int Priority, CarHistoryItem Item)>();

        foreach (var p in policies)
        {
            combined.Add((
                p.StartDate,
                0, 
                new CarHistoryItem(
                    Type: "policy",
                    Date: p.StartDate.ToString("yyyy-MM-dd"),
                    CarId: carId,
                    PolicyId: p.Id,
                    PolicyStartDate: p.StartDate.ToString("yyyy-MM-dd"),
                    PolicyEndDate: p.EndDate.ToString("yyyy-MM-dd")
                )
            ));
        }

        foreach (var c in claims)
        {
            combined.Add((
                c.ClaimDate,
                1,
                new CarHistoryItem(
                    Type: "claim",
                    Date: c.ClaimDate.ToString("yyyy-MM-dd"),
                    CarId: carId,
                    ClaimId: c.Id,
                    Description: c.Description,
                    Amount: c.Amount
                )
            ));
        }

        return combined
            .OrderBy(x => x.Key)
            .ThenBy(x => x.Priority)
            .Select(x => x.Item)
            .ToList();
    }

    public async Task<CarDto> AddCarAsync(CreateCarRequest req)
    {
   
        var car = new Car
        {
            Vin = req.Vin,
            Make = req.Make,
            Model = req.Model,
            YearOfManufacture = req.YearOfManufacture,
            OwnerId = req.OwnerId
        };

        _db.Cars.Add(car);
        await _db.SaveChangesAsync(); 

        var owner = await _db.Owners.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == car.OwnerId);

        return new CarDto(
            car.Id,
            car.Vin,
            car.Make,
            car.Model,
            car.YearOfManufacture,
            car.OwnerId,
            owner?.Name ?? string.Empty,
            owner?.Email
        );
    }
}
