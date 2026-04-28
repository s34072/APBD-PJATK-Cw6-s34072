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
}