using DoctorAppointment.Exceptions;
using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;

        public UserController(IUserService userService,IAppointmentService appointmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _userService.RegisterUserAsync(request);
            return Ok(new { success = true, message = "User registered successfully.", token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, message = "Email and password are required." });
            }

            var response = await _userService.LoginUserAsync(request);
            if (string.IsNullOrEmpty(response.Token))
            {
                return Unauthorized(new { success = false, message = "Invalid credentials" });
            }

            return Ok(new { success = true, response.Token,response.ExpiryTime });
        }

        [Authorize(Roles="User")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            var profile = await _userService.GetProfileAsync(currentUserId);
            return Ok(profile);
        }

        [Authorize(Roles = "User")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            var updatedUser = await _userService.UpdateUserAsync(currentUserId, request);
            return Ok(new { success = true, message = "Profile updated successfully.", data = updatedUser });
        }

        [Authorize(Roles = "User")]
        [HttpPost("book-appointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
        {
            var appointment = await _appointmentService.BookAppointmentAsync(request.UserId, request.DocId, request.SlotDate, request.SlotTime);

            if (appointment != null)
            {
                return Ok(new { success = true, message = "Appointment booked successfully.", appointment });
            }

            return BadRequest(new { success = false, message = "Doctor is not available or slots have been booked." });
        }

        [Authorize(Roles = "User")]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetUserAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
            return Ok(new { success = true, appointments });
        }

        [Authorize(Roles = "User")]
        [HttpDelete("cancel-appointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(CancelAppointmentRequest request)
        {
            request.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(request.UserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            var isCanceled = await _appointmentService.CancelAppointmentAsync(request, false);
            return isCanceled
                ? Ok(new { success = true, message = "Appointment canceled successfully." })
                : BadRequest(new { success = false, message = "Failed to cancel the appointment. Please try again later." });
        }

        [Authorize(Roles = "User")]
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest paymentRequest)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(paymentRequest.AppointmentId);
            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Appointment not found." });
            }

            var doctor = await _appointmentService.GetDoctorByAppointmentIdAsync(paymentRequest.AppointmentId);
            if (doctor?.Fees == null)
            {
                return BadRequest(new { success = false, message = "Doctor's fees are not available." });
            }

            appointment.Amount = doctor.Fees;
            appointment.Payment = paymentRequest.Payment;

            var paymentSuccess = true; 

            if (!paymentSuccess)
            {
                return BadRequest(new { success = false, message = "Payment failed. Please try again." });
            }

            var updateRequest = new UpdateAppointmentRequest
            {
                AppointmentId = appointment.Id,
                Payment = appointment.Payment,
                Amount=appointment.Amount,
            };

            await _appointmentService.UpdateAppointmentAsync(updateRequest);

            return Ok(new { success = true, message = "Payment successful!" });
        }

    }
}
