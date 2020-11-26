using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCChat.Models
{
    public class User
    {
        public string Id { get; set; }

        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Debe de ingresar su nombre correctamente")]
        public string Nombre { get; set; }

        [Display(Name = "Appellido")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Debe de ingresar su apellido correctamente")]
        public string Apellido { get; set; }

        [Display(Name = "Usuario")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Debe de ingresar un usuario correctamente")]
        public string Username { get; set; }

        [Display(Name = "Contrasena")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Debe de ingresar una contraseña correctamente")]
        public string Contrasena { get; set; }

        [Display(Name = "Teléfono")]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Debe de ingresar un número de teléfono correctamente")]
        public int Telefono { get; set; }

        //Lista de mensajes
        public List<Msg> Mensajes { get; set; }

        //Lista de conversaciones
        public List<string> Conversaciones { get; set; }

        //Constructor
        public User()
        {
            Id = "";
            Nombre = "";
            Apellido = "";
            Username = "";
            Contrasena = "";
            Telefono = 0;
            Mensajes = new List<Msg>();
            Conversaciones = new List<string>();
        }
    }
}