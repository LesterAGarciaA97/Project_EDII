using MVCChat.Models;
using MVCChat.Singleton;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace MVCChat.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            var user = new User();
            user.Username = collection["Username"];
            user.Contrasena = collection["Contrasena"];

            var value = Data.Instancia.RocketChat.Cliente.PostAsJsonAsync("authenticate", user);
            value.Wait();

            var Result = value.Result;

            if (Result.StatusCode == HttpStatusCode.Accepted)
            {
                var ReadTask = Result.Content.ReadAsStringAsync();
                ReadTask.Wait();
                var Jwt = JsonConvert.DeserializeObject<Jwt>(ReadTask.Result);
                Data.Instancia.RocketChat.Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Jwt.Token);
                return RedirectToAction("HomePerfil", "Perfil");
            }
            return RedirectToAction("Index");
        }
    }
}