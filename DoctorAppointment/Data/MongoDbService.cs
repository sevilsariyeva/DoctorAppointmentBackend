using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using DoctorAppointment.Models;
using MongoDB.Bson;

namespace DoctorAppointment.Services
{
    public class MongoDbService
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var uri = configuration.GetValue<string>("MongoDb:Uri");
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("MongoDb URI not provided in the configuration.");
            }

            _client = new MongoClient(uri);
            _database = _client.GetDatabase("DOCTORAPPOINTMENT");

        }
        public IMongoDatabase GetDatabase() => _database;

       
        public IMongoCollection<Admin> GetAdminsCollection()
        {
            return _database.GetCollection<Admin>("admins"); 
        }
        public IMongoCollection<User> GetUsersCollection() =>
            _database.GetCollection<User>("users");
        public IMongoCollection<Appointment> GetAppointmentsCollection() =>
            _database.GetCollection<Appointment>("appointments"); 
        public IMongoCollection<Doctor> GetDoctorsCollection() =>
            _database.GetCollection<Doctor>("doctors");
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
