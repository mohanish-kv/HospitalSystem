namespace HospitalSystem.API.DTOs.Responses;

public class RevenueBySpecializationResponse
{
    public string Specialization { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}
