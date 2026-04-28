using Microsoft.Data.SqlClient;
using APBD_PJATK_Cw6_s34072.DTOs;

namespace APBD_PJATK_Cw6_s34072.Services;

public class PatientService : IPatientService
{
    private readonly IConfiguration _configuration;

    public PatientService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<PatientDTO>> GetPatientsAsync(string? lastName)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        var result = new List<PatientDTO>();

        await using var connection = new SqlConnection(connectionString);
        
        var query = "SELECT IdPatient, FirstName, LastName, Email, PhoneNumber, DateOfBirth, IsActive FROM dbo.Patients";
        if (!string.IsNullOrEmpty(lastName))
        {
            query += " WHERE LastName = @LastName";
        }

        await using var command = new SqlCommand(query, connection);
        
        if (!string.IsNullOrEmpty(lastName))
        {
            command.Parameters.AddWithValue("@LastName", lastName);
        }

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new PatientDTO
            {
                IdPatient = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                PhoneNumber = reader.GetString(4),
                DateOfBirth = reader.GetDateTime(5),
                IsActive = reader.GetBoolean(6)
            });
        }

        return result;
    }

    public async Task<PatientDetailsDTO?> GetPatientByIdAsync(int id)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        
        await using var connection = new SqlConnection(connectionString);
        
        var query = @"
            SELECT p.IdPatient, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.DateOfBirth, p.IsActive,
                   a.IdAppointment, a.AppointmentDate, a.Status, a.Reason,
                   d.FirstName AS DoctorFirstName, d.LastName AS DoctorLastName,
                   s.Name AS SpecializationName
            FROM dbo.Patients p
            LEFT JOIN dbo.Appointments a ON p.IdPatient = a.IdPatient
            LEFT JOIN dbo.Doctors d ON a.IdDoctor = d.IdDoctor
            LEFT JOIN dbo.Specializations s ON d.IdSpecialization = s.IdSpecialization
            WHERE p.IdPatient = @Id";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        PatientDetailsDTO? patient = null;

        while (await reader.ReadAsync())
        {
            if (patient == null)
            {
                patient = new PatientDetailsDTO
                {
                    IdPatient = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PhoneNumber = reader.GetString(4),
                    DateOfBirth = reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                };
            }

            if (!reader.IsDBNull(7))
            {
                patient.Appointments.Add(new AppointmentDTO
                {
                    IdAppointment = reader.GetInt32(7),
                    AppointmentDate = reader.GetDateTime(8),
                    Status = reader.GetString(9),
                    Reason = reader.GetString(10),
                    DoctorName = $"{reader.GetString(11)} {reader.GetString(12)}",
                    DoctorSpecialization = reader.GetString(13)
                });
            }
        }

        return patient;
    }
}