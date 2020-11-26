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
    public class ConversacionesController : Controller
    {
        // GET: Conversaciones
        public ActionResult Conversaciones()
        {
            string id = new Jwt().ObtenerId();

            if (id != "")
            {
                var Direccion = "Conversaciones/" + id;
                var Respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(Direccion);
                Respuesta.Wait();
                var Resultado = Respuesta.Result;

                if (Resultado.StatusCode == HttpStatusCode.OK)
                {
                    var ReadTask = Resultado.Content.ReadAsStringAsync();
                    ReadTask.Wait();

                    var ConversacionesUser = JsonConvert.DeserializeObject<List<string>>(ReadTask.Result);
                    var ListaConversaciones = new List<string>();
                    foreach (var Conversacion in ConversacionesUser)
                    {
                        ListaConversaciones.Add(Conversacion.Split('.')[0]);
                    }
                    return View(ListaConversaciones);
                }
                else
                {
                    return RedirectToAction("HomePerfil", "Perfil");
                }

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult CrearConversacion() {
            string id = new Jwt().ObtenerId();
            if (id != "")
            {
                var Direccion = "AllUsers/" + id;
                var Respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(Direccion);
                Respuesta.Wait();
                var Resultado = Respuesta.Result;

                if (Resultado.StatusCode == HttpStatusCode.OK)
                {
                    var ReadTask = Resultado.Content.ReadAsStringAsync();
                    ReadTask.Wait();
                    var ListaUsuariosRegistrados = JsonConvert.DeserializeObject<List<User>>(ReadTask.Result);
                    return View(ListaUsuariosRegistrados);
                }
                else
                {
                    return RedirectToAction("HomePerfil", "Perfil");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Mensajes(string id)
        {
            //El id recibido será el del receptor
            var idEmisor = new Jwt().ObtenerId();

            if (idEmisor != "")
            {
                var direccion = "Perfil/" + idEmisor;
                var respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(direccion);
                respuesta.Wait();

                var result = respuesta.Result;

                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();

                var userEmisor = JsonConvert.DeserializeObject<User>(readTask.Result);

                var UserReceptor = "";

                foreach (var itemConversacion in userEmisor.Conversaciones)
                {
                    if (itemConversacion.Split('.')[1] == id)
                    {
                        UserReceptor = itemConversacion.Split('.')[0];
                    }
                }

                if (UserReceptor != "")
                {
                    TempData["userReceptor"] = UserReceptor;
                }
                else
                {
                    var direccion2 = "Perfil/" + id;
                    var respuesta2 = Data.Instancia.RocketChat.Cliente.GetAsync(direccion2);
                    respuesta2.Wait();

                    var result2 = respuesta2.Result;

                    var readTask2 = result2.Content.ReadAsStringAsync();
                    readTask2.Wait();

                    var userReceptor = JsonConvert.DeserializeObject<User>(readTask2.Result);

                    var Receptor = userReceptor.Username;

                    TempData["userReceptor"] = Receptor;
                }

                ViewBag.userReceptor = TempData["userReceptor"];

                var direccionMensajes = "Chat/" + idEmisor + "/" + id;
                var respuestaMensajes = Data.Instancia.RocketChat.Cliente.GetAsync(direccionMensajes);
                respuestaMensajes.Wait();

                var resultMensaje = respuestaMensajes.Result;

                ViewBag.Emisor = idEmisor;
                ViewBag.Receptor = id;

                if (resultMensaje.StatusCode == HttpStatusCode.OK)
                {
                    var readTaskMessages = resultMensaje.Content.ReadAsStringAsync();
                    readTaskMessages.Wait();

                    var listaMensajes = JsonConvert.DeserializeObject<List<Msg>>(readTaskMessages.Result);

                    return View(listaMensajes);
                }
                return RedirectToAction("HomePerfil", "Perfil");
            }
            else
            {
                return RedirectToAction("CerrarSesion", "Perfil");

            }
        }

        public ActionResult UserNameToId(string usernameReceptor) {
            var idEmisor = new Jwt().ObtenerId();
            if (idEmisor != "")
            {
                var Direccion = "Perfil/" + idEmisor;
                var Respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(Direccion);
                Respuesta.Wait();
                var Resultado = Respuesta.Result;
                var ReadTask = Resultado.Content.ReadAsStringAsync();
                ReadTask.Wait();

                var user = JsonConvert.DeserializeObject<User>(ReadTask.Result);
                var idReceptor = "";

                foreach (var ItemConversacion in user.Conversaciones)
                {
                    if (ItemConversacion.Split('.')[0] == usernameReceptor)
                    {
                        idReceptor = ItemConversacion.Split('.')[1];
                    }
                }
                return RedirectToAction("Mensajes", new { id = idReceptor });
            }
            return RedirectToAction("Conversaciones", "Conversaciones");
        }

        [HttpPost]
        public ActionResult MandarMensajes(FormCollection collection) {
            var idReceptor = TempData["receptor"];
            var cuerpoMensaje = collection["Contenido"];
            var idEmisor = new Jwt().ObtenerId();
            var mensaje = new Msg();
            mensaje.Contenido = cuerpoMensaje;

            TempData.Remove("receptor");
            if (idEmisor != "")
            {
                var Direccion = "Chat/" + idEmisor + "/" + idReceptor;
                var Respuesta = Data.Instancia.RocketChat.Cliente.PutAsJsonAsync(Direccion, mensaje);
                Respuesta.Wait();
                var Resultado = Respuesta.Result;
                if (Resultado.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = Resultado.Content.ReadAsStringAsync();
                    readTask.Wait();
                    return RedirectToAction("Mensajes", new { id = idReceptor });
                }
                else
                {
                    return RedirectToAction("HomePerfil", "Perfil");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}