using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace REST_API.Controllers
{
    /// <summary>
    /// Kontrollerer API-Endpoint for scoretavlen. Håndterer CRUD-operationer for spilscore.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScoreboardController : ControllerBase
    {
        private static  Dictionary<int, ScoreboardModel> _scoreboard = new Dictionary<int, ScoreboardModel>();

        /// <summary>
        /// Initialiserer en ny instans af ScoreboardController klassen.
        /// </summary>
        public ScoreboardController()
        {

        }

        /// <summary>
        /// Henter alle scoreposter fra scoretavlen.
        /// </summary>
        /// <returns>En liste af ScoreboardModel objekter.</returns>
        [HttpGet("GetScoreboard")]
        public ActionResult<IEnumerable<ScoreboardModel>> Get()
        {
            return Ok(_scoreboard.Values);
        }

        /// <summary>
        /// Tilføjer en ny scorepost til scoretavlen.
        /// </summary>
        /// <param name="newEntry">Det nye ScoreboardModel objekt, der skal tilføjes.</param>
        /// <returns>ActionResult repræsentation af operationens udfald.</returns>
        [HttpPost("PostScore")]
        public IActionResult Post([FromBody] ScoreboardModel newEntry)
        {
            if (newEntry == null)
            {
                return BadRequest("Invalid Data");
            }

            int newId = _scoreboard.Count + 1;
            newEntry.ID = newId;

            _scoreboard[newId] = newEntry;

            return CreatedAtAction(nameof(Get), new { id = newEntry.ID }, newEntry);

        }

        /// <summary>
        /// Opdaterer en eksisterende scorepost i scoretavlen.
        /// </summary>
        /// <param name="id">ID'et for den scorepost, der skal opdateres.</param>
        /// <param name="updateClient">Det opdaterede ScoreboardModel objekt.</param>
        /// <returns>ActionResult repræsentation af operationens udfald.</returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] ScoreboardModel updateClient)
        {
            if (!_scoreboard.ContainsKey(id)) 
            { 
                return NotFound();
            }

            updateClient.ID = id;
            _scoreboard[id] = updateClient;
            return NoContent();
        }

        
    }
}
