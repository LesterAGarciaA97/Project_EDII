using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MVCChat.Services
{
    public class ApiService
    {
        public HttpClient Cliente = new HttpClient();
        public ApiService(string _BaseAddress)
        {
            Cliente.BaseAddress = new Uri(_BaseAddress);
        }

    }
}