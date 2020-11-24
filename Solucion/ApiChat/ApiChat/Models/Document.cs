using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Models
{
    public class Document
    {

        [BsonElement]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string DocNombre { get; set; }


        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string EmisorId { get; set; }


        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string ReceptorId { get; set; }


        [BsonElement]
        [BsonRepresentation(BsonType.String)]

        public string Contenido { get; set; }


        [BsonElement]
        [BsonRepresentation(BsonType.DateTime)]

        public DateTime FechaSubida { get; set; }


        public Document()
        {
            Contenido = "";
        }
      
    }
}
