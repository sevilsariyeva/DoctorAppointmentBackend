using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/doctor")]
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
        [HttpGet("all-doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(doctors);
        }
        [HttpPut("change-availability/{doctorId}")]
        public async Task<IActionResult> ChangeAvailability(string doctorId)
        {
            var success = await _doctorService.ChangeAvailabilityAsync(doctorId);
            if (!success)
            {
                return NotFound(new { success = false, message = "Doctor not found" });
            }

            return Ok(new { success = true, message = "Availability updated successfully" });
        }



    }

}
