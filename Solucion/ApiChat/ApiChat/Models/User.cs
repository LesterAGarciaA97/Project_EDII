using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Nombre { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Apellido { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Username { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Contrasena { get; set; }


        [BsonElement]
        [BsonRepresentation(BsonType.Int64)]
        public int Telefono { get; set; }

        [BsonElement]
        public List<Msg> Mensajes { get; set; }
        
        [BsonElement]
        public List<string> Conversaciones { get; set; }


    }
}
