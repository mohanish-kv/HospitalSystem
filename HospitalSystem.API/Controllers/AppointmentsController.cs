using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;

    public AppointmentsController(AppointmentService service) => _service = service;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
    {
        var newId = await _service.BookAsync(request);
        return CreatedAtAction(nameof(Book), new { id = newId }, new { id = newId });
    }

    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id);
        return NoContent();
    }

    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUpcoming()
    {
        var appointments = await _service.GetUpcomingAsync();
        return Ok(appointments);
    }

    [HttpGet("doctor/{doctorId:int}")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetByDoctor(int doctorId)
    {
        var appointments = await _service.GetByDoctorAsync(doctorId);
        return Ok(appointments);
    }
}
