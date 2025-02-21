using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Required]
        public String FullName { get; set; }

        [Required]
        public String Email { get; set; }

        [Required]
        public String Password { get; set; }
        public String ImageUrl { get; set; }
        public Address Address { get; set; }
        public String Gender { get; set; }
        [DataType(DataType.Date)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Dob { get; set; }
        public String Phone { get; set; }
    }
}
