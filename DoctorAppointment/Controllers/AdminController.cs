using DoctorAppointment.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController
    {
        private readonly AdminService _adminService;
        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public IActionResult LoginAdmin([FromBody] LoginRequest request)
        {
            try
            {
                var token = _adminService.LoginAdmin(request.Email, request.Password);
                return new OkObjectResult(new { success = true, token });
            }
            catch (UnauthorizedAccessException)
            {
                return new UnauthorizedObjectResult(new { success = false, message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { success = false, message = ex.Message })
                {
                    StatusCode = 500 
                };
            }
        }
    }
}
