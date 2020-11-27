using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCChat.Models;
using System.Text;
using MVCChat.Singleton;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

namespace MVCChat.Controllers
{
    public class ArchivoController : Controller
    {
        // GET: Archivo
        public ActionResult Index()
        {
            var idReceptor = TempData["receptor"];
            ViewBag.Receptor = idReceptor;

            var userNameReceptor = TempData["receptorUser"];
            ViewBag.userReceptor = userNameReceptor;
            return View();
        }

        [HttpPost]
        public ActionResult Cargar(HttpPostedFileBase postedFile)
        {
            try
            {
                var idReceptor = TempData["receptor"];
                var filePath = string.Empty;
                var path = Server.MapPath("~/MisArchivos/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var RutaAbsolutaServer = path;
                var RutaAbsolutaArchivo = "";
                var nombre = postedFile.FileName.Split('.')[0];
                filePath = path + Path.GetFileName(postedFile.FileName);
                RutaAbsolutaArchivo = filePath;
                postedFile.SaveAs(filePath);


                LZW _lzw = new LZW();
                _lzw.Compresion(postedFile, postedFile.FileName,RutaAbsolutaServer);
                var SDES = new SDES.SDES(nombre + ".lzw", RutaAbsolutaServer + nombre + ".lzw", path, 250);
                SDES.Operar(1);
                var doc = new Document();
                doc.DocNombre = nombre;
                doc.EmisorId = new Jwt().ObtenerId();
                doc.ReceptorId = idReceptor.ToString();

                using (var file = new FileStream(SDES.RutaAbsolutaArchivoOperado, FileMode.Open))
                {
                    using (var reader = new BinaryReader(file, Encoding.UTF8))
                    {
                        var contenido = new byte[reader.BaseStream.Length];
                        contenido = reader.ReadBytes(contenido.Length);

                        var chars = Encoding.UTF8.GetChars(contenido);

                        foreach (var caracter in chars)
                        {
                            doc.Contenido += caracter;
                        }
                    }
                }
                var respuesta = Data.Instancia.RocketChat.Cliente.PostAsJsonAsync("sendoc", doc);
                respuesta.Wait();
                var resultado = respuesta.Result;

                if (resultado.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = resultado.Content.ReadAsStringAsync();
                    readTask.Wait();
                    System.IO.File.Delete(SDES.RutaAbsolutaArchivoOperado);
                    return RedirectToAction("Conversaciones", "Conversaciones");
                }
                else
                {
                    return RedirectToAction("HomePerfil", "Perfil");
                }

            }
            catch
            {
                return RedirectToAction("HomePerfil", "Perfil");
            }
        }

        public ActionResult MisDocumentos()
        {
            var direccion = "getdocs/" + new Jwt().ObtenerId();
            var res = Data.Instancia.RocketChat.Cliente.GetAsync(direccion);
            res.Wait();

            var result = res.Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();

                var listaDocumentos = JsonConvert.DeserializeObject<List<Document>>(readTask.Result);
                return View(listaDocumentos);
            }

            return RedirectToAction("HomePerfil", "Perfil");

        }
        public FileResult Obtener(string nombre)// obtener los archivos de la base
        {
            var direccion = "getdoc/" + nombre;
            var respuesta = Data.Instancia.RocketChat.Cliente.GetAsync(direccion);
            respuesta.Wait();
            var resultado = respuesta.Result;
            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                var readTask = resultado.Content.ReadAsStringAsync();
                readTask.Wait();
                var documento = JsonConvert.DeserializeObject<Document>(readTask.Result);
                var texto = Encoding.UTF8.GetBytes(documento.Contenido);
                var path = Server.MapPath("~/MisArchivos/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var file = new FileStream(path + documento.DocNombre + ".sdes",FileMode.Create))
                {
                    using (var writer = new BinaryWriter(file, Encoding.UTF8))
                    {
                        writer.Write(texto);
                    }
                }
                var RutaAbsolutaServer = path;
                var RutaAbsoltutaArchivo = RutaAbsolutaServer + documento.DocNombre + ".sdes";
                var nombreDoc = documento.DocNombre + ".sdes";
                var sdes = new SDES.SDES(nombreDoc, RutaAbsoltutaArchivo, RutaAbsolutaServer, 250);
                sdes.Operar(2);

                LZW _lzw = new LZW();
                _lzw.Decompresion(documento.DocNombre + ".lzw", RutaAbsolutaServer);
                var fileS = new FileStream(RutaAbsolutaServer + documento.DocNombre + ".txt", FileMode.Open, FileAccess.Read);

                return File(fileS, "*.txt", documento.DocNombre + ".txt");
            }
            return null;
        }
    }
}