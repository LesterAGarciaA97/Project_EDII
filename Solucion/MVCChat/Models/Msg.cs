﻿using System.ComponentModel.DataAnnotations;

namespace MVCChat.Models
{
    public class Msg
    {
        public string Emisor { get; set; }

        public string Receptor { get; set; }

        public string Fecha { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Debe de ingresar su nombre correctamente")]
        public string Contenido { get; set; }

        public Msg()
        {
            Emisor = "";
            Receptor = "";
            Fecha = "";
        }

        public override string ToString()
        {
            return Contenido + "\nFecha: " + Fecha;
        }
    }
}