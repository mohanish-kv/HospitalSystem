namespace HospitalSystem.API.Configuration;

public class EmailSettings
{
    public bool IsEnabled { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "Hospital System";
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseStartTls { get; set; } = true;
}
