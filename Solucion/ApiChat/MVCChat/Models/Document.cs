using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCChat.Models
{
    public class Document
    {
        public string Id { get; set; }

        public string DocNombre { get; set; }

        public string EmisorId { get; set; }

        public string ReceptorId { get; set; }

        public string Contenido { get; set; }

        public DateTime FechaSubida { get; set; }

        public Document()
        {
            Id = "";
            DocNombre = "";
            EmisorId = "";
            ReceptorId = "";
            Contenido = "";
            FechaSubida = DateTime.Now;
        }
    }
}