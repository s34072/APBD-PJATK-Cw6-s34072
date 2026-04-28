namespace APBD_PJATK_Cw6_s34072.DTOs;

public class AppointmentDTO
{
    public int IdAppointment { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
    public string DoctorName { get; set; }
    public string DoctorSpecialization { get; set; }
}