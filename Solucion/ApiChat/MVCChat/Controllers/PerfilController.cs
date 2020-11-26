using MVCChat.Models;
using MVCChat.Singleton;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MVCChat.Controllers
{
    public class PerfilController : Controller
    {
        // GET: Perfil
        public ActionResult HomePerfil()
        {
            var id = new Jwt().ObtenerId();
            if (id != "")
            {
                var Direccion = "Perfil/" + id;
                var Respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(Direccion);
                Respuesta.Wait();
                var Result = Respuesta.Result;
                if (Result.StatusCode == HttpStatusCode.OK)
                {
                    var ReadTask = Result.Content.ReadAsStringAsync();
                    ReadTask.Wait();
                    var User = JsonConvert.DeserializeObject<User>(ReadTask.Result);
                    return View(User);
                }
                else if (Result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        public ActionResult Editar() {
            var id = TempData["id"];
            var Direccion = "Perfil/" + id;
            var Respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(Direccion);
            Respuesta.Wait();

            var Result = Respuesta.Result;
            var ReadTask = Result.Content.ReadAsStringAsync();
            ReadTask.Wait();

            var User = JsonConvert.DeserializeObject<User>(ReadTask.Result);
            return View(User);
        }

        [HttpPost]
        public ActionResult Editar(FormCollection collection)
        {
            var User = new User();
            User.Id = new Jwt().ObtenerId();
            User.Nombre = collection["Nombre"];
            User.Apellido = collection["Apellido"];
            User.Contrasena = collection["Contrasena"];
            User.Username = collection["Username"];
            User.Telefono = int.Parse(collection["Telefono"]);


            var Direccion = "Perfil/" + new Jwt().ObtenerId();
            var Respuesta = Data.Instancia.RocketChat.Cliente.PutAsJsonAsync(Direccion, User);
            Respuesta.Wait();
            var Result = Respuesta.Result;

            if (Result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("CerrarSesion");
            }
            TempData["id"] = new Jwt().ObtenerId();
            return RedirectToAction("Editar", "Perfil");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult MenuResult(FormCollection collection) {
            try
            {
                var id = new Jwt().ObtenerId();
                TempData["id"] = id;

                if (collection["CrearConversacion"] != null)
                {
                    return RedirectToAction("CrearConversacion", "Conversaciones"); //Pendiente Crear
                }
                else if (collection["Conversaciones"] != null)
                {
                    return RedirectToAction("Conversaciones", "Conversaciones"); //Pendiente Crear
                }
                else if (collection["Archivos"] != null)
                {
                    return RedirectToAction("MisDocumentos", "Archivo"); //Pendiente Crear
                }
                else if (collection["EditarPerfil"] != null)
                {
                    return RedirectToAction("Editar");
                }
                else if (collection["EliminarPerfil"] != null)
                {
                    return RedirectToAction("Eliminar");
                }
                else if (collection["CerrarSesion"] != null)
                {
                    return RedirectToAction("CerrarSesion");
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                return RedirectToAction("HomePerfil");
            }

            return null;
        }

        public ActionResult CerrarSesion()
        {
            Data.Instancia.RocketChat.Cliente.DefaultRequestHeaders.Authorization = null;

            return RedirectToAction("Index", "Login");
        }

        public ActionResult Eliminar()
        {
            var id = TempData["id"];

            var direccion = "Perfil/" + id;
            var respuesta = Data.Instancia.RocketChat.Cliente.DeleteAsync(direccion);
            respuesta.Wait();

            var result = respuesta.Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("CerrarSesion");
            }

            TempData["id"] = new Jwt().ObtenerId();
            return RedirectToAction("HomePerfil", "Perfil");

        }
    }
}