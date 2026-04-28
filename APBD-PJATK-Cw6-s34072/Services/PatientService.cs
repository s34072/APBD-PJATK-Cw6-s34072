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
    public async Task<PatientDTO?> AddPatientAsync(CreatePatientDTO newPatient)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);

        var checkEmailQuery = "SELECT COUNT(1) FROM dbo.Patients WHERE Email = @Email";
        await using var checkCommand = new SqlCommand(checkEmailQuery, connection);
        checkCommand.Parameters.AddWithValue("@Email", newPatient.Email);

        await connection.OpenAsync();
        
        var emailExists = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (emailExists)
        {
            return null; 
        }

        var insertQuery = @"
            INSERT INTO dbo.Patients (FirstName, LastName, Email, PhoneNumber, DateOfBirth, IsActive)
            OUTPUT INSERTED.IdPatient
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @DateOfBirth, 1)";

        await using var insertCommand = new SqlCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@FirstName", newPatient.FirstName);
        insertCommand.Parameters.AddWithValue("@LastName", newPatient.LastName);
        insertCommand.Parameters.AddWithValue("@Email", newPatient.Email);
        insertCommand.Parameters.AddWithValue("@PhoneNumber", newPatient.PhoneNumber);
        insertCommand.Parameters.AddWithValue("@DateOfBirth", newPatient.DateOfBirth);

        var newId = (int)await insertCommand.ExecuteScalarAsync();

        return new PatientDTO
        {
            IdPatient = newId,
            FirstName = newPatient.FirstName,
            LastName = newPatient.LastName,
            Email = newPatient.Email,
            PhoneNumber = newPatient.PhoneNumber,
            DateOfBirth = newPatient.DateOfBirth,
            IsActive = true
        };
    }

    public async Task<bool> DeactivatePatientAsync(int id)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);

        var query = "UPDATE dbo.Patients SET IsActive = 0 WHERE IdPatient = @Id";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();
        
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
    
    public async Task<PatientDTO?> UpdatePatientAsync(int id, UpdatePatientDTO updatedPatient)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var checkEmailQuery = "SELECT COUNT(1) FROM dbo.Patients WHERE Email = @Email AND IdPatient != @Id";
        await using var checkCommand = new SqlCommand(checkEmailQuery, connection);
        checkCommand.Parameters.AddWithValue("@Email", updatedPatient.Email);
        checkCommand.Parameters.AddWithValue("@Id", id);
        
        var emailTaken = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (emailTaken)
        {
            throw new Exception("EmailExists");
        }

        var query = @"
            UPDATE dbo.Patients 
            SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                PhoneNumber = @PhoneNumber, DateOfBirth = @DateOfBirth
            WHERE IdPatient = @Id AND IsActive = 1";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@FirstName", updatedPatient.FirstName);
        command.Parameters.AddWithValue("@LastName", updatedPatient.LastName);
        command.Parameters.AddWithValue("@Email", updatedPatient.Email);
        command.Parameters.AddWithValue("@PhoneNumber", updatedPatient.PhoneNumber);
        command.Parameters.AddWithValue("@DateOfBirth", updatedPatient.DateOfBirth);

        var rowsAffected = await command.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            return null;
        }

        return new PatientDTO
        {
            IdPatient = id,
            FirstName = updatedPatient.FirstName,
            LastName = updatedPatient.LastName,
            Email = updatedPatient.Email,
            PhoneNumber = updatedPatient.PhoneNumber,
            DateOfBirth = updatedPatient.DateOfBirth,
            IsActive = true
        };
    }
}