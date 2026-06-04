# HospitalSystem

A .NET 8 ASP.NET Core Web API for basic hospital operations, including patient registration and maintenance, doctor lookups, appointment booking/cancellation, and reporting. Data access is implemented with ADO.NET against SQL Server stored procedures defined in `HospitalDB.sql`.

## Project structure

```text
HospitalSystem.sln                  # Visual Studio/.NET solution
HospitalDB.sql                      # SQL Server database schema, indexes, seed data, and stored procedures
HospitalSystem.API/                 # ASP.NET Core Web API project
  Controllers/                      # HTTP API endpoints grouped by feature
  Domain/Entities/                  # Core domain models: Patient, Doctor, Appointment, Person
  Domain/Exceptions/                # Domain-specific exception types
  DTOs/Requests/                    # Request payload contracts
  DTOs/Responses/                   # Response payload contracts
  Interfaces/                       # Repository abstractions
  Middleware/                       # Global exception handling and request logging middleware
  Repositories/                     # ADO.NET repository implementations
  Services/                         # Business/use-case services used by controllers
  Program.cs                        # Dependency injection and HTTP middleware pipeline
  appsettings.json                  # Default configuration, including HospitalDb and SMTP email settings
```

## Setup steps

1. Install the .NET 8 SDK and ensure a SQL Server instance is available.
2. Create and initialize the database by running `HospitalDB.sql` against SQL Server. The script creates the `HospitalDB` database and required stored procedures.
3. Update `HospitalSystem.API/appsettings.json` if your SQL Server connection differs from the default local trusted connection:

   ```json
   "ConnectionStrings": {
     "HospitalDb": "Server=localhost;Database=HospitalDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

4. Configure SMTP email delivery if patient notifications should be sent for registrations, booked appointments, and cancelled appointments. Keep `IsEnabled` set to `false` for local development without an SMTP account.

   ```json
   "EmailSettings": {
     "IsEnabled": true,
     "SmtpServer": "smtp.your-provider.com",
     "Port": 587,
     "SenderName": "Hospital System",
     "SenderEmail": "no-reply@your-hospital.com",
     "Username": "smtp-user",
     "Password": "smtp-password",
     "UseStartTls": true
   }
   ```

5. Restore and build the solution:

   ```bash
   dotnet restore HospitalSystem.sln
   dotnet build HospitalSystem.sln
   ```

6. Run the API:

   ```bash
   dotnet run --project HospitalSystem.API/HospitalSystem.API.csproj
   ```

7. In a development environment, open Swagger UI at the HTTPS or HTTP URL printed by `dotnet run` with `/swagger` appended.

## Doctor API examples

Add a doctor with `POST /api/doctors`:

```json
{
  "doctorCode": "D004",
  "fullName": "Dr. Asha Menon",
  "specialization": "Dermatology",
  "phoneNumber": "9000012345",
  "consultationFee": 700.00,
  "isAvailable": true
}
```

The endpoint returns `201 Created` with the newly created doctor id and a `Location` header for `GET /api/doctors/{id}`.

## Assumptions

- SQL Server is the backing database, and application queries rely on the stored procedures in `HospitalDB.sql` being present.
- The default connection string assumes local SQL Server with Windows/Trusted authentication; non-Windows or remote environments may need SQL authentication or another server name.
- Swagger is enabled only when `ASPNETCORE_ENVIRONMENT` is `Development`.
- No automated test project is currently included in the solution, so `dotnet build` is the primary verification command available in this repository.
