using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeddingHallsController : ControllerBase
    {
        private readonly WeddingHallDbContext _context;

        public WeddingHallsController(WeddingHallDbContext context)
        {
            _context = context;
        }

        // GET: api/WeddingHalls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeddingHall>>> GetWeddingHalls()
        {
            var weddingHall = await _context.WeddingHalls
                .Include(w => w.Foods)
                .ToListAsync();

            return weddingHall;
        }

        // GET: api/WeddingHalls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WeddingHall>> GetWeddingHall(int id)
        {
            var weddingHall = await _context.WeddingHalls.FindAsync(id);

            if (weddingHall == null)
            {
                return NotFound();
            }

            return weddingHall;
        }

        // PUT: api/WeddingHalls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWeddingHall(int id, WeddingHall weddingHall)
        {
            if (id != weddingHall.Id)
            {
                return BadRequest();
            }

            _context.Entry(weddingHall).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WeddingHallExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/WeddingHalls
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<WeddingHall>> PostWeddingHall(WeddingHall weddingHall)
        {
            _context.WeddingHalls.Add(weddingHall);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWeddingHall", new { id = weddingHall.Id }, weddingHall);
        }

        // DELETE: api/WeddingHalls/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeddingHall(int id)
        {
            var weddingHall = await _context.WeddingHalls.FindAsync(id);
            if (weddingHall == null)
            {
                return NotFound();
            }

            _context.WeddingHalls.Remove(weddingHall);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WeddingHallExists(int id)
        {
            return _context.WeddingHalls.Any(e => e.Id == id);
        }
    }
}
