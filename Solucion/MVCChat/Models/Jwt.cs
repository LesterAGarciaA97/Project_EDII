using MVCChat.Singleton;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace MVCChat.Models
{
    public class Jwt
    {
        public string Token { get; set; }

        public string ObtenerId() {
            var id = "";
            if (Data.Instancia.RocketChat.Cliente.DefaultRequestHeaders.Authorization != null)
            {
                var TokenHeader = Data.Instancia.RocketChat.Cliente.DefaultRequestHeaders.Authorization.Parameter;
                var TokenHandler = new JwtSecurityTokenHandler();
                var JwtToken = TokenHandler.ReadJwtToken(TokenHeader);
                var ListaClaims = JwtToken.Claims.ToList();
                id = ListaClaims.Find(x => x.Type == "unique_name").Value.ToString();
            }
            return id;
        }
    }
}
