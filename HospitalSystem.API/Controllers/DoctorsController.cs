using HospitalSystem.API.DTOs.Requests;
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDoctor([FromBody] AddDoctorRequest request)
    {
        var id = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetDoctorById), new { id }, new { id });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DoctorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorResponse>>> Get(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable)
    {
        var doctors = await _service.GetAsync(specialization, isAvailable);
        return Ok(doctors);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorResponse>> GetDoctorById(int id)
    {
        var doctor = await _service.GetByIdAsync(id);
        return doctor is null ? NotFound() : Ok(doctor);
    }
}
