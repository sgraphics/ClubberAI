using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClubberAI.ServiceDefaults.Model
{

	public class Party
	{
		[BsonId] // Specifies that this is the primary key field.
		[BsonRepresentation(BsonType.ObjectId)] // Maps the ObjectId from MongoDB to a string in .NET.
		public string Id { get; set; }

		[BsonElement("blammo")] // Maps the "Name" property to the "Name" field in the MongoDB document.
		public string Blammo { get; set; }
	}
}
