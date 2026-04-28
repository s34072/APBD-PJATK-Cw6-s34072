using APBD_PJATK_Cw6_s34072.DTOs;
using APBD_PJATK_Cw6_s34072.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_PJATK_Cw6_s34072.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? lastName)
    {
        var patients = await _patientService.GetPatientsAsync(lastName);
        return Ok(patients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        
        if (patient == null)
        {
            return NotFound($"Pacjent o ID {id} nie istnieje w bazie.");
        }

        return Ok(patient);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddPatient(CreatePatientDTO newPatient)
    {
        var createdPatient = await _patientService.AddPatientAsync(newPatient);

        if (createdPatient == null)
        {
            return Conflict($"Błąd: Pacjent z adresem email {newPatient.Email} już istnieje.");
        }

        return Created($"/api/Patients/{createdPatient.IdPatient}", createdPatient);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivatePatient(int id)
    {
        var success = await _patientService.DeactivatePatientAsync(id);

        if (!success)
        {
            return NotFound($"Pacjent o ID {id} nie istnieje w bazie.");
        }

        return NoContent();
    }
}