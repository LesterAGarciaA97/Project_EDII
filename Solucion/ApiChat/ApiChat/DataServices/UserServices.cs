using ApiChat.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
