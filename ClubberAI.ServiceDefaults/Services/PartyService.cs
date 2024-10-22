using ClubberAI.ServiceDefaults.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace ClubberAI.ServiceDefaults.Services
{
	public class PartyService
	{
		private readonly IMongoCollection<Party> _myCollection;

		public PartyService(IConfiguration config)
		{
			// Access the connection string from the ConnectionStrings section
			var connectionString = config.GetConnectionString("mongodb");
			var client = new MongoClient(connectionString);

			// Extract database name if included in the connection string
			var database = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);

			_myCollection = database.GetCollection<Party>("test");
		}

		public Party GetFirst()
		{
			return _myCollection.FindSync(Builders<Party>.Filter.Empty).FirstOrDefault();
		}
	}
}
