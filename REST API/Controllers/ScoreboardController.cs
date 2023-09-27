using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace REST_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoreboardController : ControllerBase
    {
        private readonly Dictionary<int, ScoreboardModel> _scoreboard;

        public ScoreboardController()
        {
            _scoreboard = new Dictionary<int, ScoreboardModel>();
        }


        [HttpGet("GetScoreboard")]
        public ActionResult<IEnumerable<ScoreboardModel>> Get()
        {
            return Ok(_scoreboard.Values);
        }


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
