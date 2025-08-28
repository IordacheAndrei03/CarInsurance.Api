using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpPost("cars")]
    public async Task<ActionResult<CarDto>> CreateCar([FromBody] CreateCarRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Vin))
            return BadRequest("VIN is required.");

        try
        {
            var created = await _service.AddCarAsync(request);
            return Created($"/api/cars/{created.Id}", created);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException se && se.SqliteErrorCode == 19)
        {
            return Conflict("A car with the same VIN already exists.");
        }
    }

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return BadRequest("Missing 'date' query parameter. Use YYYY-MM-DD (e.g., 2025-08-01).");

        if (!DateOnly.TryParseExact(
           date,
           "yyyy-MM-dd",
           CultureInfo.InvariantCulture,
           DateTimeStyles.None,
           out var parsed))
        {
            return BadRequest("Invalid date. Use strict format YYYY-MM-DD (e.g., 2025-08-01).");
        }

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("cars/{carId:long}/history")]
    public async Task<ActionResult<List<CarHistoryItem>>> GetCarHistory(long carId)
    {
        try
        {
            var history = await _service.GetCarHistoryAsync(carId);
            return Ok(history); 
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }


}
