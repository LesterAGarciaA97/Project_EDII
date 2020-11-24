using ApiChat.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiChat.DataServices
{
    public class UserServices
    {
        private readonly IMongoCollection<User> _Users;
        private readonly IMongoCollection<Document> _Documents;
        private readonly IMongoDatabase _db;
        private readonly AppConfiguration _AppSettings;
        private GridFSBucket gfs;

        public UserServices(IConfiguration config, IOptions<AppConfiguration> appSettings) {
            _AppSettings = appSettings.Value;

            var client = new MongoClient(config.GetConnectionString("RocketChat"));

            var dataBase = client.GetDatabase("RocketChat");
            _Users = dataBase.GetCollection<User>("UsuariosRocketChat");
            _Documents = dataBase.GetCollection<Document>("DocumentosRocketChat");

            _db = dataBase;
            gfs = new GridFSBucket(dataBase);
        }

        public List<User> GetAllUsers(string id) {
            return _Users.Find(users => users.Id != id).ToList();
        }

        public User Get(string id) {
            return _Users.Find(usuario => usuario.Id == id).FirstOrDefault();
        }
        public User Create(User usuario) {
            var ListadoUsuarios = _Users.Find(usuario1 => true).ToList();
            if (ListadoUsuarios.Find(_usuario => _usuario.Username == usuario.Username) == null)
            {
                _Users.InsertOne(usuario);
            }
            else
            {
                usuario = null;
            }
            return usuario;
        }
        public void UpdateUser(string id, User usuario) {
            _Users.ReplaceOne(_usuario => _usuario.Id == id, usuario);
        }

        public void Remove(string id) {
            _Users.DeleteOne(_usuario => _usuario.Id == id);
        }

        public string UpdateMessageEmisor(string id, Msg objMensaje) {
            var EmisorFilter = Builders<User>.Filter.Eq("Id", id);
            var Update = Builders<User>.Update.Push("Mensaje", objMensaje);

            _Users.UpdateOne(EmisorFilter, Update);
            var ReceptorFilter = Builders<User>.Filter.Eq("Id", objMensaje.Receptor);
            if (_Users.Find(ReceptorFilter).ToList().Count != 0)
            {
                var _Receptor = _Users.Find(ReceptorFilter).ToList()[0];
                var InfoReceptor = _Receptor.Username + "." + _Receptor.Id;
                var ConversacionesFilter = Builders<User>.Filter.Eq("Id", objMensaje.Emisor) & Builders<User>.Filter.Eq("Conversaciones", InfoReceptor);
                if (_Users.Find(ConversacionesFilter).ToList().Count == 0)
                {
                    var UpdateConversacion = Builders<User>.Update.Push("Conversaciones", InfoReceptor);
                    _Users.UpdateOne(EmisorFilter, UpdateConversacion);
                }
            }
            var User1 = _Users.Find(EmisorFilter).ToList();
            return User1[0].Username + "." + User1[0].Id;
        }
        public bool UpdateMessageReceptor(string IdReceptor, string UsernameComEmisor, Msg objMensaje) {
            var UserFilter = Builders<User>.Filter.Eq("Id", IdReceptor);
            if (_Users.Find(UserFilter).ToList().Count == 1)
            {
                var ConversacionesFilter = Builders<User>.Filter.Eq("Id", IdReceptor) & Builders<User>.Filter.Eq("Conversaciones", UsernameComEmisor);
                if (_Users.Find(ConversacionesFilter).ToList().Count == 0)
                {
                    var UpdateConversacion = Builders<User>.Update.Push("Conversaciones", UsernameComEmisor);
                    _Users.UpdateOne(UserFilter, UpdateConversacion);
                }
                var UpdateM = Builders<User>.Update.Push("Mensajes", objMensaje);
                _Users.UpdateOne(UserFilter, UpdateM);

                return true;
            }
            
            return false;
        }

        public List<Msg> GetMessages(string IdEmisor, string IdReceptor) {
            var Usuario = _Users.Find(user => user.Id == IdEmisor).FirstOrDefault();
            var Enviados = Usuario.Mensajes.FindAll(mensaje => (mensaje.Emisor == IdEmisor && mensaje.Receptor == IdReceptor) || (mensaje.Emisor == IdReceptor && mensaje.Receptor == IdEmisor));
            return Enviados;
        }

        public List<string> GetConversations(string idEmisor) {
            var usuario = _Users.Find(user => user.Id == idEmisor).FirstOrDefault();
            return usuario.Conversaciones;
        }

        //Procesos de autenticacion y tambien inicio de sesion
        public Jwt Authenticate(string UserName, string Password) {
            var user = _Users.Find(_User => _User.Username == UserName && _User.Contrasena == Password).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            var _jwt = new Jwt();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_AppSettings.Secret);
            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var Token = tokenHandler.CreateToken(TokenDescriptor);
            _jwt.Token = tokenHandler.WriteToken(Token);
            return _jwt;
        }

        public bool SendDocuments(string FileName, byte[] FileBytes) {
            GridFSBucket GridFs = new GridFSBucket(_db);
            var ObjectId = GridFs.UploadFromBytesAsync(FileName, FileBytes);
            return true;
        }

        public Document SendDocuments(Document document)
        {
            _Documents.InsertOne(document);

            return document;
        }

        public List<Document> GetDocuments(string usId)
        {
            var listaDocs = _Documents.Find(doc => doc.EmisorId == usId || doc.ReceptorId == usId).ToList();

            return listaDocs;
        }

        public Document DownloadDocument(string fileName)
        {
            var doc = _Documents.Find(docX => docX.DocNombre == fileName).FirstOrDefault();

            return doc;
        }
    }
}
