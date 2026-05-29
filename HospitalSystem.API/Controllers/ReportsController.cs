using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _service;

    public ReportsController(ReportService service) => _service = service;

    [HttpGet("consolidated")]
    [ProducesResponseType(typeof(IEnumerable<ConsolidatedAppointmentReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConsolidatedAppointmentReportResponse>>> GetConsolidated()
    {
        var report = await _service.GetConsolidatedAsync();
        return Ok(report);
    }

    [HttpGet("doctor-counts")]
    [ProducesResponseType(typeof(IEnumerable<DoctorAppointmentCountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorAppointmentCountResponse>>> GetDoctorCounts()
    {
        var report = await _service.GetDoctorCountsAsync();
        return Ok(report);
    }

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(IEnumerable<RevenueBySpecializationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RevenueBySpecializationResponse>>> GetRevenue()
    {
        var report = await _service.GetRevenueAsync();
        return Ok(report);
    }

    [HttpGet("duplicates")]
    [ProducesResponseType(typeof(IEnumerable<DuplicateAppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DuplicateAppointmentResponse>>> GetDuplicates()
    {
        var report = await _service.GetDuplicatesAsync();
        return Ok(report);
    }
}
