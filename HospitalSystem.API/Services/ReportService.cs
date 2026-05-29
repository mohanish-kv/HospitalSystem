using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class ReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo) => _repo = repo;

    public Task<IEnumerable<ConsolidatedAppointmentReportResponse>> GetConsolidatedAsync()
        => _repo.GetConsolidatedAsync();

    public Task<IEnumerable<DoctorAppointmentCountResponse>> GetDoctorCountsAsync()
        => _repo.GetDoctorCountsAsync();

    public Task<IEnumerable<RevenueBySpecializationResponse>> GetRevenueAsync()
        => _repo.GetRevenueAsync();

    public Task<IEnumerable<DuplicateAppointmentResponse>> GetDuplicatesAsync()
        => _repo.GetDuplicatesAsync();
}
