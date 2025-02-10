using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpPost("add-doctor")]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto doctorDto, [FromForm] IFormFile image)
        {
            await _doctorService.AddDoctorAsync(doctorDto, image);
            return Ok(new { success = true, message = "Doctor added successfully" });
        }

    }

}
