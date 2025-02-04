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
            PrintAllCollectionsAndFields();

        }
        public IMongoDatabase GetDatabase() => _database;

        // Method to print all collections and documents in the database
        public void PrintAllCollectionsAndFields()
        {
            // Get all collection names
            var collections = _database.ListCollectionNames().ToList();


            foreach (var collectionName in collections)
            {
                Console.WriteLine($"Collection: {collectionName}");

                var collection = _database.GetCollection<BsonDocument>(collectionName); // BsonDocument istifadə edin
                var firstDocument = collection.Find(FilterDefinition<BsonDocument>.Empty).FirstOrDefault();

                if (firstDocument != null)
                {
                    Console.WriteLine("Fields in the first document:");
                    foreach (var element in firstDocument) // BsonDocument üzərində iterasiya
                    {
                        Console.WriteLine($"- {element.Name}: {element.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("No documents found in this collection.");
                }

                Console.WriteLine(); // New line between collections
            }

        }
        public IMongoCollection<Admin> GetAdminsCollection()
        {
            return _database.GetCollection<Admin>("admins"); 
        }
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
