using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly DoctorService _service;

    public DoctorsController(DoctorService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DoctorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorResponse>>> Get(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable)
    {
        var doctors = await _service.GetAsync(specialization, isAvailable);
        return Ok(doctors);
    }
}
