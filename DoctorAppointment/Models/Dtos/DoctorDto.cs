using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models.Dtos
{
    public class DoctorDto
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Experience { get; set; }

        [Required]
        public decimal Fees { get; set; }

        [Required]
        public string About { get; set; }
        [Required]
        public bool Available { get; set; }

        [Required]
        public string Speciality { get; set; }

        [Required]
        public string Degree { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }

        public string? Image { get; set; }
    }

}
