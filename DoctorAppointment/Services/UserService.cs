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

namespace DoctorAppointment.Services
{
    public class UserService:IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        public UserService(IUserRepository userRepository,IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request)
        {
            if (!IsValidEmail(request.Email))
            {
                return new RegisterUserResponse { Success = false, Message = "Invalid email format." };
            }

            if (!IsStrongPassword(request.Password))
            {
                return new RegisterUserResponse { Success = false, Message = "Password must be at least 8 characters long and contain uppercase, lowercase, and a digit." };
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new RegisterUserResponse { Success = false, Message = "Email is already in use." };
            }

            var hashedPassword = HashPassword(request.Password);
            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = hashedPassword
            };

            var result = await _userRepository.AddUserAsync(newUser);
            if (!result)
            {
                return new RegisterUserResponse { Success = false, Message = "User registration failed." };
            }

            var token = GenerateJwtToken(newUser);

            return new RegisterUserResponse { Success = true, Message = "User registered successfully.", Token = token };
        }
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, "User")
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<LoginUserResponse> LoginUserAsync(LoginUserRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginUserResponse { Success = false, Message = "Invalid email or password." };
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return new LoginUserResponse { Success = false, Message = "Invalid email or password." };
            }

            var token = GenerateJwtToken(user);
            return new LoginUserResponse { Success = true, Token = token, Message = "Login successful." };
        }


    }
}
