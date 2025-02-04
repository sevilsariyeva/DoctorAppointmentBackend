using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models
{
    public class Admin
    {
        public ObjectId Id { get; set; }
        [Required]
        [BsonElement("email")]
        public string Email { get; set; }
        [Required]
        [BsonElement("password")]
        public string Password { get; set; }
    }
}
