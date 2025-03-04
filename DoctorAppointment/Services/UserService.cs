using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using DoctorAppointment.Repositories;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using System.Security.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using DoctorAppointment.Utilities;
using DoctorAppointment.Exceptions;
using DoctorAppointment.Helpers;

namespace DoctorAppointment.Services
{
    public class UserService:IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        public UserService(IUserRepository userRepository,IDoctorRepository doctorRepository,IPasswordHasher<User> passwordHasher,IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _doctorRepository = doctorRepository;
        }

        public async Task<JwtTokenResponse> RegisterUserAsync(RegisterUserRequest request)
        {
            if (!RegisterHelper.IsValidEmail(request.Email))
            {
                throw new ArgumentException("Invalid email format.", nameof(request.Email));
            }

            if (!RegisterHelper.IsStrongPassword(request.Password))
            {
                throw new WeakPasswordException("Password must be at least 8 characters long and contain uppercase, lowercase, and a digit.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException("Email is already in use.");
            }
            if (!await EmailHelper.EmailExists(request.Email))
            {
                throw new EmailValidationException("Email does not exist.");
            }

            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                ImageUrl = "/uploads/upload_area.png"
            };

            newUser.Password = _passwordHasher.HashPassword(newUser, request.Password);

            var result = await _userRepository.AddUserAsync(newUser);
            if (!result)
            {
                throw new Exception("User registration failed.");
            }
            SendEmailHelper.SendEmail(request.Email, "Welcome to DocApp!", "Dear Patient,\r\nWelcome to the DocApp website! We are glad to have you with us. You can now easily book appointments, consult with doctors, and manage your health records conveniently.\r\n\r\nIf you have any questions, feel free to contact our support team.\r\n\r\nBest regards,\r\nDocApp Team");
            return JwtTokenGenerator.GenerateToken(newUser.Id.ToString(), newUser.Email, "User", _configuration);
        }


        public async Task<JwtTokenResponse> LoginUserAsync(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return JwtTokenGenerator.GenerateToken(user.Id, user.Email, "User", _configuration);

        }

        public async Task<User> GetProfileAsync(string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ArgumentException("User ID is required.", nameof(currentUserId));
            }

            var user = await _userRepository.GetUserByIdAsync(currentUserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return user;
        }

        public async Task<User> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            bool hasChanges = false;

            if (!string.IsNullOrEmpty(request.FullName) && request.FullName != user.FullName)
            {
                user.FullName = request.FullName;
                hasChanges = true;
            }

            if (request.Image != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                user.ImageUrl = "/uploads/" + Path.GetFileName(filePath);
                hasChanges = true;
            }

            if (request.Address != null)
            {
                if (user.Address == null)
                {
                    user.Address = new Address();
                }

                if (!string.IsNullOrEmpty(request.Address.Line1) && request.Address.Line1 != user.Address.Line1)
                {
                    user.Address.Line1 = request.Address.Line1;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(request.Address.Line2) && request.Address.Line2 != user.Address.Line2)
                {
                    user.Address.Line2 = request.Address.Line2;
                    hasChanges = true;
                }
            }

            if (request.Gender != null && request.Gender != user.Gender)
            {
                user.Gender = request.Gender;
                hasChanges = true;
            }

            if (request.Dob.HasValue && request.Dob.Value != user.Dob)
            {
                user.Dob = request.Dob.Value;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(request.Phone) && request.Phone != user.Phone)
            {
                user.Phone = request.Phone;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return user;
            }

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result)
            {
                throw new Exception("User update failed.");
            }

            return user;
        }



    }
}
