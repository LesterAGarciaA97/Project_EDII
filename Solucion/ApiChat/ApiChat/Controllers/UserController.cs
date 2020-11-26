using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiChat.DataServices;
using ApiChat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BibliotecaDeClases;

namespace ApiChat.Controllers
{
    [Authorize]
    [Route("RocketChat/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserServices _userService;

        public UserController(UserServices userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtener todos los usuarios
        /// </summary>
        /// <param name="idEmisor"></param>
        /// <returns></returns>
        [HttpGet("AllUsers/{idEmisor:length(24)}")]
        public ActionResult<List<User>> GetAllUsers([FromRoute] string idEmisor)
        {
            return Ok(_userService.GetAllUsers(idEmisor));
        }
        /// <summary>
        /// Obtener Usuario emisor
        /// </summary>
        /// <param name="idEmisor"></param>
        /// <returns></returns>
        //Get : RocketChat/User/Chat/id
        [HttpGet("Perfil/{idEmisor:length(24)}")]
        public ActionResult<User> GetUser([FromRoute] string idEmisor)
        {
            return Ok(_userService.Get(idEmisor));
        }

        /// <summary>
        /// Obtener mensajes
        /// </summary>
        /// <param name="idEmisor"></param>
        /// <param name="idReceptor"></param>
        /// <returns></returns>
        [HttpGet("Chat/{idEmisor:length(24)}/{idReceptor:length(24)}", Name = "GetUser")]
        public ActionResult<List<Msg>> Get([FromRoute] string idEmisor, [FromRoute] string idReceptor)
        {
            var mensajesEnConversacion = _userService.GetMessages(idEmisor, idReceptor);

            foreach (var mensaje in mensajesEnConversacion)
            {
                var sdes = new SDES(mensaje.Contenido, 250);
                mensaje.Contenido = sdes.OperarMensaje(2);
            }

            return Ok(mensajesEnConversacion);

        }

        /// <summary>
        /// Crear usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Create")]
        public IActionResult Create([FromBody] User usuario)
        {
            try
            {
                var sdes = new SDES(usuario.Contrasena, 250);
                usuario.Contrasena = sdes.OperarMensaje(1);
                var USER = _userService.Create(usuario);

                if (USER != null)
                {
                    return Created("Chat/" + USER.Id, USER);
                }

                return Conflict();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "INFORMACION INCORRECTA, POR FAVOR REVISE LA INFORMACION" });
            }

        }
        /// <summary>
        /// Actualizacion usuario
        /// </summary>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        [HttpPut("Perfil/{id:length(24)}")]
        public ActionResult Update(string id, [FromBody] User usuario)
        {
            try
            {
                var Usuario = _userService.Get(id);

                if (Usuario == null)
                {
                    return NotFound();
                }

                var sdes = new SDES(usuario.Contrasena, 250);
                usuario.Contrasena = sdes.OperarMensaje(1);
                _userService.UpdateUser(id, usuario);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "INFORMACION INCORRECTA, POR FAVOR REVISE LA INFORMACION" });
            }
        }

         /// <summary>
         /// Eliminar usuario
         /// </summary>
         /// <param name="id"></param>
         /// <returns></returns>
        [HttpDelete("Perfil/{id:length(24)}")]
        public ActionResult Delete(string id)
        {
            var usuario = _userService.Get(id);

            if (usuario == null)
            {
                return NotFound();
            }

            _userService.Remove(id);
            return Ok(NoContent());
        }

        /// <summary>
        /// Modificacion mensaje
        /// </summary>
        /// <param name="idEmisor"></param>
        /// <param name="idReceptor"></param>
        /// <param name="mensaje"></param>
        /// <returns></returns>
        [HttpPut("Chat/{idEmisor:length(24)}/{idReceptor:length(24)}", Name = "PutMessage")]
        public ActionResult UpdateMessage([FromRoute] string idEmisor, [FromRoute] string idReceptor, [FromBody] Msg mensaje)
        {
            mensaje.Emisor = idEmisor;
            mensaje.Receptor = idReceptor;
            mensaje.Fecha = DateTime.Now.ToString();

            var sdes = new SDES(mensaje.Contenido, 250);
            mensaje.Contenido = sdes.OperarMensaje(1);

           
            var usernameCompuesto = _userService.UpdateMessageEmisor(mensaje.Emisor, mensaje);

            var correct = _userService.UpdateMessageReceptor(mensaje.Receptor, usernameCompuesto, mensaje);

            if (correct)
            {
                return Ok(); //Se actualizaron emisor y receptor
            }

            return NoContent(); //Solo se actulizó emisor
        }

        /// <summary>
        /// Obtener conversaciones
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Conversaciones/{id:length(24)}")]
        public ActionResult<List<string>> GetConversations([FromRoute] string id)
        {
            return Ok(_userService.GetConversations(id));
        }

        /// <summary>
        /// Proceso de autenticacion 
        /// </summary>
        /// <param name="userParameter"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] User userParameter)
        {
            var sdes = new SDES(userParameter.Contrasena, 250);
            var jwt = _userService.Authenticate(userParameter.Username, sdes.OperarMensaje(1));

            if (jwt == null)
            {
                return BadRequest(new { message = "USUARIO O CONTRASENA INCORRECTOS" });
            }

            return Accepted(jwt);
        }
        /// <summary>
        /// Enviar documento
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        [HttpPost("sendoc", Name = "PutDocument")]
        public ActionResult<Document> SendDocument([FromBody] Document document)
        {
            if (document != null)
            {
                _userService.SendDocuments(document);
                return Ok(document);
            }
            else
            {
                return BadRequest();
            }


        }
        /// <summary>
        /// Obtener documentos
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getdocs/{id:length(24)}")]
        public ActionResult<List<Document>> GetDocuments([FromRoute] string id)
        {
            return Ok(_userService.GetDocuments(id));
        }

        [HttpGet("getdoc/{filename}")]
        public ActionResult<Document> GetSingleDoc([FromRoute] string filename)
        {
            return Ok(_userService.DownloadDocument(filename));
        }
    }
}
