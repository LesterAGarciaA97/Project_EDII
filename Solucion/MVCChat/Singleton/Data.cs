using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVCChat.Services;

namespace MVCChat.Singleton
{
    public class Data
    {
        private static Data _instancia = null;
        public static Data Instancia {
            get {
                if (_instancia == null)
                {
                    _instancia = new Data();
                }
                return _instancia;
            }
        }
        public ApiService RocketChat = new ApiService("http://localhost:44347/RocketChat/User/");
    }
}
