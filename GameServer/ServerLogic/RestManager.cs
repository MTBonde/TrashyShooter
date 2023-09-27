using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class RestManager
    {
        public static HttpClient restClient = new HttpClient();

        public static void SetupRest()
        {
            restClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7159/api/Scoreboard/"),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public static HttpClient GetRestClient()
        {
            return restClient;
        }
    }
}
