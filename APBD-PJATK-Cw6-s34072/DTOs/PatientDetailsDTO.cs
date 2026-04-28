namespace APBD_PJATK_Cw6_s34072.DTOs;

public class PatientDetailsDTO : PatientDTO
{
    public List<AppointmentDTO> Appointments { get; set; } = new List<AppointmentDTO>();
}