using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api/cars/{carId:long}/claims")]
public class ClaimsController(ClaimService service) : ControllerBase
{
    private readonly ClaimService _service = service;

    /// POST /api/cars/{carId}/claims
    [HttpPost]
    public async Task<IActionResult> Create(long carId, [FromBody] CreateClaimRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Amount must be greater than 0");
        if (string.IsNullOrWhiteSpace(request.Description)) return BadRequest("Description is required");

        try
        {
            var created = await _service.AddClaimAsync(carId, request);
            return Created($"/api/cars/{carId}/claims/{created.Id}", value: null);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// GET /api/cars/{carId}/claims/
    [HttpGet]
    public async Task<ActionResult<List<ClaimResponse>>> GetAll(long carId)
    {
        try
        {
            var list = await _service.ListByCarAsync(carId);
            return Ok(list); 
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

    }
}
