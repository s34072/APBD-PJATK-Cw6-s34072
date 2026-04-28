using APBD_PJATK_Cw6_s34072.DTOs;

namespace APBD_PJATK_Cw6_s34072.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientDTO>> GetPatientsAsync(string? lastName);
    Task<PatientDetailsDTO?> GetPatientByIdAsync(int id);
    Task<PatientDTO?> AddPatientAsync(CreatePatientDTO newPatient);
    Task<bool> DeactivatePatientAsync(int id);
    Task<PatientDTO?> UpdatePatientAsync(int id, UpdatePatientDTO updatedPatient);
}