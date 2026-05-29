create database HospitalDB;
GO

use HospitalDB;
GO

--patient table
CREATE TABLE Patients(
PatientID INT IDENTITY(1,1) PRIMARY KEY,
PatientCode VARCHAR(150) NOT NULL UNIQUE,
FullName NVARCHAR(150) NOT NULL,
DateOfBirth DATE NOT NULL,
Gender CHAR(1) NOT NULL CHECK(Gender IN ('M','F','O')),
PhoneNumber VARCHAR(20) NOT NULL UNIQUE,
Email VARCHAR(150) NULL UNIQUE,
IsActive BIT NOT NULL DEFAULT 1,
CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

--doctor table

CREATE TABLE Doctors (
DoctorId INT IDENTITY(1,1) PRIMARY KEY,
DoctorCode        VARCHAR(20)    NOT NULL UNIQUE,
FullName          NVARCHAR(150)  NOT NULL,
Specialization    NVARCHAR(100)  NOT NULL,
PhoneNumber       VARCHAR(20)    NOT NULL UNIQUE,
ConsultationFee   DECIMAL(10,2)  NOT NULL CHECK (ConsultationFee >= 0),
IsAvailable       BIT            NOT NULL DEFAULT 1,
CreatedAt         DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

--appointment table

CREATE TABLE Appointments (
    AppointmentId     INT            IDENTITY(1,1) PRIMARY KEY,
    PatientId         INT            NOT NULL REFERENCES Patients(PatientId),
    DoctorId          INT            NOT NULL REFERENCES Doctors(DoctorId),
    AppointmentDate   DATETIME2      NOT NULL,
    Status            VARCHAR(20)    NOT NULL DEFAULT 'Scheduled'
                        CHECK (Status IN ('Scheduled','Completed','Cancelled')),
    CancelledAt       DATETIME2      NULL,
    CreatedAt         DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

--indexes

CREATE INDEX IX_Appointments_DoctorId
    ON Appointments(DoctorId);

CREATE INDEX IX_Appointments_AppointmentDate
    ON Appointments(AppointmentDate);

CREATE INDEX IX_Appointments_Doctor_Date
    ON Appointments(DoctorId, AppointmentDate);
GO

--stored procedure for patient

-- Register a new patient
CREATE PROCEDURE sp_RegisterPatient
    @PatientCode  VARCHAR(20),
    @FullName     NVARCHAR(150),
    @DateOfBirth  DATE,
    @Gender       CHAR(1),
    @PhoneNumber  VARCHAR(20),
    @Email        VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Patients WHERE PhoneNumber = @PhoneNumber)
        THROW 50001, 'Phone number already registered.', 1;
    IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM Patients WHERE Email = @Email)
        THROW 50002, 'Email already registered.', 1;

    INSERT INTO Patients (PatientCode,FullName,DateOfBirth,Gender,PhoneNumber,Email)
    VALUES (@PatientCode,@FullName,@DateOfBirth,@Gender,@PhoneNumber,@Email);
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

-- Get all active patients
CREATE PROCEDURE sp_GetActivePatients
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PatientId, PatientCode, FullName, DateOfBirth, Gender,
           PhoneNumber, Email, IsActive, CreatedAt
    FROM Patients
    WHERE IsActive = 1;
END
GO

-- Update patient
CREATE PROCEDURE sp_UpdatePatient
    @PatientId   INT,
    @FullName    NVARCHAR(150),
    @PhoneNumber VARCHAR(20),
    @Email       VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Patients WHERE PatientId = @PatientId AND IsActive = 1)
        THROW 50003, 'Patient not found or inactive.', 1;
    UPDATE Patients
    SET FullName = @FullName, PhoneNumber = @PhoneNumber, Email = @Email
    WHERE PatientId = @PatientId;
END
GO

-- deactivate a patient
CREATE PROCEDURE sp_DeactivatePatient
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Patients SET IsActive = 0 WHERE PatientId = @PatientId;
END
GO

--stored procedure for doctors

-- Get doctors by specialization and/or availability
CREATE PROCEDURE sp_GetDoctors
    @Specialization NVARCHAR(100) = NULL,
    @IsAvailable    BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DoctorId, DoctorCode, FullName, Specialization,
           PhoneNumber, ConsultationFee, IsAvailable
    FROM Doctors
    WHERE (@Specialization IS NULL OR Specialization = @Specialization)
      AND (@IsAvailable    IS NULL OR IsAvailable    = @IsAvailable);
END
GO

-- Book an appointment (runs inside a transaction)
CREATE PROCEDURE sp_BookAppointment
    @PatientId       INT,
    @DoctorId        INT,
    @AppointmentDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Check doctor is available
        IF NOT EXISTS (SELECT 1 FROM Doctors WHERE DoctorId=@DoctorId AND IsAvailable=1)
        BEGIN
            ROLLBACK;
            THROW 50010, 'Doctor is not available for booking.', 1;
        END

        INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate)
        VALUES (@PatientId, @DoctorId, @AppointmentDate);

        COMMIT;
        SELECT SCOPE_IDENTITY() AS NewId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

-- Cancel a scheduled appointment
CREATE PROCEDURE sp_CancelAppointment
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Appointments WHERE AppointmentId=@AppointmentId AND Status='Scheduled')
        THROW 50011, 'Appointment not found or not in Scheduled status.', 1;

    UPDATE Appointments
    SET Status='Cancelled', CancelledAt=GETDATE()
    WHERE AppointmentId=@AppointmentId;
END
GO

-- Get upcoming appointments (next 7 days)
CREATE PROCEDURE sp_GetUpcomingAppointments
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.AppointmentId,
           p.FullName AS PatientName,
           d.FullName AS DoctorName,
           d.Specialization,
           CONVERT(VARCHAR,a.AppointmentDate,113) AS AppointmentDateFormatted,
           a.Status,
           d.ConsultationFee
    FROM Appointments a
    JOIN Patients p ON a.PatientId = p.PatientId
    JOIN Doctors  d ON a.DoctorId  = d.DoctorId
    WHERE a.AppointmentDate BETWEEN GETDATE() AND DATEADD(DAY,7,GETDATE())
      AND a.Status = 'Scheduled'
    ORDER BY a.AppointmentDate;
END
GO

-- Get appointments by doctor
CREATE PROCEDURE sp_GetAppointmentsByDoctor
    @DoctorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.AppointmentId,
           p.FullName AS PatientName,
           CONVERT(VARCHAR,a.AppointmentDate,113) AS AppointmentDateFormatted,
           a.Status, d.ConsultationFee
    FROM Appointments a
    JOIN Patients p ON a.PatientId = p.PatientId
    JOIN Doctors  d ON a.DoctorId  = d.DoctorId
    WHERE a.DoctorId = @DoctorId
    ORDER BY a.AppointmentDate DESC;
END
GO

--Stored procedure for reports

-- Consolidated appointment report
CREATE PROCEDURE sp_ConsolidatedReport
AS
BEGIN
    SELECT p.FullName AS PatientName, d.FullName AS DoctorName,
           d.Specialization,
           CONVERT(VARCHAR,a.AppointmentDate,113) AS AppointmentDate,
           a.Status, d.ConsultationFee
    FROM Appointments a
    JOIN Patients p ON a.PatientId = p.PatientId
    JOIN Doctors  d ON a.DoctorId  = d.DoctorId
    ORDER BY a.AppointmentDate DESC;
END
GO

-- Doctors with more than 2 appointments
CREATE PROCEDURE sp_DoctorAppointmentCount
AS
BEGIN
    SELECT d.FullName, COUNT(*) AS TotalAppointments
    FROM Appointments a
    JOIN Doctors d ON a.DoctorId = d.DoctorId
    GROUP BY d.DoctorId, d.FullName
    HAVING COUNT(*) > 2
    ORDER BY TotalAppointments DESC;
END
GO

-- Revenue by specialization
CREATE PROCEDURE sp_RevenueBySpecialization
AS
BEGIN
    SELECT d.Specialization,
           SUM(d.ConsultationFee) AS TotalRevenue,
           COUNT(*) AS AppointmentCount
    FROM Appointments a
    JOIN Doctors d ON a.DoctorId = d.DoctorId
    WHERE a.Status = 'Completed'
    GROUP BY d.Specialization
    ORDER BY TotalRevenue DESC;
END
GO

-- Patients booked with same doctor on same date
CREATE PROCEDURE sp_DuplicateDayBookings
AS
BEGIN
    SELECT d.FullName AS DoctorName,
           CAST(a.AppointmentDate AS DATE) AS AppointmentDate,
           COUNT(*) AS PatientCount,
           STRING_AGG(p.FullName, ', ') AS Patients
    FROM Appointments a
    JOIN Patients p ON a.PatientId = p.PatientId
    JOIN Doctors  d ON a.DoctorId  = d.DoctorId
    WHERE a.Status <> 'Cancelled'
    GROUP BY d.DoctorId, d.FullName, CAST(a.AppointmentDate AS DATE)
    HAVING COUNT(*) > 1;
END
GO

INSERT INTO Patients (PatientCode,FullName,DateOfBirth,Gender,PhoneNumber,Email)
VALUES
('P001','Arjun Sharma','1990-05-12','M','9876543210','arjun@email.com'),
('P002','Priya Reddy','1985-11-22','F','9123456789','priya@email.com'),
('P003','Mohammed Ali','1995-03-08','M','9988776655',NULL);

INSERT INTO Doctors (DoctorCode,FullName,Specialization,PhoneNumber,ConsultationFee,IsAvailable)
VALUES
('D001','Dr. Sneha Rao','Cardiology','9111222333',800,1),
('D002','Dr. Kiran Kumar','Neurology','9444555666',950,1),
('D003','Dr. Fatima Nair','Orthopedics','9777888999',600,0);

INSERT INTO Appointments (PatientId,DoctorId,AppointmentDate,Status)
VALUES
(1,1,DATEADD(DAY,2,GETDATE()),'Scheduled'),
(2,1,DATEADD(DAY,3,GETDATE()),'Scheduled'),
(3,2,DATEADD(DAY,1,GETDATE()),'Scheduled'),
(1,2,DATEADD(DAY,-5,GETDATE()),'Completed');