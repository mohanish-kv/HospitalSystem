namespace HospitalSystem.API.Domain.Exceptions;

public class AppointmentConflictException : DomainException
{
    public AppointmentConflictException(string message) : base(message)
    {
    }
}
