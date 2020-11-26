using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Models
{
    public class Msg
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Emisor { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Receptor { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)] 
        public string Fecha { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Contenido { get; set; }

    }
}
