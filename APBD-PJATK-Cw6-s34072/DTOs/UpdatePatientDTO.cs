using System.ComponentModel.DataAnnotations;

namespace APBD_PJATK_Cw6_s34072.DTOs;

public class UpdatePatientDTO
{
    [Required(ErrorMessage = "Imię jest wymagane")]
    [MaxLength(80, ErrorMessage = "Imię może mieć maksymalnie 80 znaków")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    [MaxLength(80)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Niepoprawny format adresu email")]
    [MaxLength(120)]
    public string Email { get; set; }

    [Required]
    [MaxLength(30)]
    public string PhoneNumber { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }
}