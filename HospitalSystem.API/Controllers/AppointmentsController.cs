using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _appointmentService;

    public AppointmentsController(AppointmentService appointmentService)
        => _appointmentService = appointmentService;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BookAppointment(BookAppointmentRequest request)
    {
        var id = await _appointmentService.BookAsync(request);
        return CreatedAtAction(nameof(BookAppointment), new { id }, new { appointmentId = id });
    }

    [HttpDelete("{appointmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        await _appointmentService.CancelAsync(appointmentId);
        return NoContent();
    }
}
