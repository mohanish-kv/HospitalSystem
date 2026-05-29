using HospitalSystem.API.DTOs.Responses;

namespace HospitalSystem.API.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<ConsolidatedAppointmentReportResponse>> GetConsolidatedAsync();
    Task<IEnumerable<DoctorAppointmentCountResponse>> GetDoctorCountsAsync();
    Task<IEnumerable<RevenueBySpecializationResponse>> GetRevenueAsync();
    Task<IEnumerable<DuplicateAppointmentResponse>> GetDuplicatesAsync();
}
