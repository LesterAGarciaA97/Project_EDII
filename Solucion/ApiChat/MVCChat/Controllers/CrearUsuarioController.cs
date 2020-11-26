using MVCChat.Models;
using MVCChat.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MVCChat.Controllers
{
    public class CrearUsuarioController : Controller
    {
        // GET: CrearUsuario
        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crear(FormCollection collection) {
            var _User = new User();

            _User.Nombre = collection["Nombre"];
            _User.Apellido = collection["Apellido"];
            _User.Username = collection["Username"];
            _User.Contrasena = collection["Contrasena"];
            _User.Telefono = int.Parse(collection["Telefono"]);


            var Respuesta = Data.Instancia.RocketChat.Cliente.PostAsJsonAsync("Create", _User);
            Respuesta.Wait();
            var Result = Respuesta.Result;
            if (Result.StatusCode == HttpStatusCode.Created)
            {

                var ReadTask = Result.Content.ReadAsStringAsync();
                ReadTask.Wait();
                return RedirectToAction("Index", "Login");

            }
            return RedirectToAction("Crear");


        }
    }
}