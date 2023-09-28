using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// Håndterer REST-forbindelser ved at initialisere og returnere en HttpClient.
    /// </summary>
    public static class RestManager
    {
        // Indeholder HttpClient-objektet der skal bruges til at lave API-kald.
        public static HttpClient restClient = new HttpClient();

        /// <summary>
        /// Konfigurerer HttpClient med basisadresse og timeout.
        /// </summary>
        public static void SetupRest()
        {
            // Initialiserer HttpClient med basisadresse og timeout.
            restClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7159/api/Scoreboard/"),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// Returnerer den indstillede HttpClient.
        /// </summary>
        /// <returns>HttpClient-objektet der er indstillet med basisadresse og timeout.</returns>
        public static HttpClient GetRestClient()
        {
            return restClient;
        }
    }
}
