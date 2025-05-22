using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;
using WeddingHallAPI.Services;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeddingHallsController : ControllerBase
    {
        private readonly WeddingHallDbContext _context;
        private readonly IDataService _dataService;

        public WeddingHallsController(WeddingHallDbContext context, IDataService dataService)
        {
            _context = context;
            _dataService = dataService;
        }



        // GET: api/WeddingHalls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeddingHall>>> GetWeddingHalls()
        {
            _dataService.CheckFixData();
            var weddingHalls = await _context.WeddingHalls
                .Include(w => w.Foods)
                .ToListAsync();

            return weddingHalls;
        }

        // GET: api/WeddingHalls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WeddingHall>> GetWeddingHall(int id)
        {
            _dataService.CheckFixData();
            var weddingHall = await _context.WeddingHalls
                .Include(w => w.Foods)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weddingHall == null)
            {
                return NotFound();
            }

            return weddingHall;
        }

        // PUT: api/WeddingHalls/5
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
                _dataService.SaveDataToJSON();
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
        [HttpPost]
        public async Task<ActionResult<WeddingHall>> PostWeddingHall(WeddingHall weddingHall)
        {
            _context.WeddingHalls.Add(weddingHall);
            await _context.SaveChangesAsync();
            _dataService.SaveDataToJSON();

            return CreatedAtAction("GetWeddingHall", new { id = weddingHall.Id }, weddingHall);
        }

        // POST: api/WeddingHallsArray
        [HttpPost("Array")]
        public async Task<ActionResult<IEnumerable<WeddingHall>>> PostWeddingHallsArray(IEnumerable<WeddingHall> weddingHalls)
        {
            _context.WeddingHalls.AddRange(weddingHalls);
            await _context.SaveChangesAsync();
            _dataService.SaveDataToJSON();
            return CreatedAtAction("GetWeddingHalls", weddingHalls);
        }

        // POST: api/WeddingHalls/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromServices] IWebHostEnvironment hostEnvironment)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var weddingHall = await _context.WeddingHalls.FindAsync(id);

            if (weddingHall == null)
            {
                return NotFound();
            }

            var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(hostEnvironment.ContentRootPath, "wwwroot");
            var imagePath = Path.Combine(webRootPath, "images");
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            // Restrict file size (example: 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return BadRequest("File size exceeds the maximum limit of 5MB.");
            }

            var uploadsFolder = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", "images", "weddinghalls");
            Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            weddingHall.ImageUrl = $"/images/weddinghalls/{fileName}";
            await _context.SaveChangesAsync();
            _dataService.SaveDataToJSON();
            return Ok(new { ImageUrl = weddingHall.ImageUrl });
        }



        // PUT: api/WeddingHalls/5/image-url
        [HttpPut("{id}/image-url")]
        public async Task<IActionResult> PutWeddingHallImageUrl(int id, [FromBody] string imageUrl)
        {
            var weddingHall = await _context.WeddingHalls.FindAsync(id);
            if (weddingHall == null)
            {
                return NotFound();
            }
            weddingHall.ImageUrl = imageUrl;
            _context.Entry(weddingHall).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                _dataService.SaveDataToJSON();
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
            _dataService.SaveDataToJSON();

            return NoContent();
        }

        [HttpDelete("Clear-All-Data")]
        public IActionResult ClearAllData(string password)
        {
            if (password != "KHalilov0548")
            {
                return Unauthorized("Invalid password.");
            }

            _dataService.ClearData();
            return Ok("All data cleared.");
        }

        private bool WeddingHallExists(int id)
        {
            return _context.WeddingHalls.Any(e => e.Id == id);
        }
    }
}
