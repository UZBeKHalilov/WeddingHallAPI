using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;
using WeddingHallAPI.Services;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly WeddingHallDbContext _context;
        private readonly IDataService _dataService;

        public FoodsController(WeddingHallDbContext context, IDataService dataService)
        {
            _context = context;
            _dataService = dataService;
        }
            
        // GET: api/Foods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
        {
            _dataService.CheckFixData();
            return await _context.Foods.ToListAsync();
        }

        // GET: api/Foods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Food>> GetFood(int id)
        {
            _dataService.CheckFixData();
            var food = await _context.Foods.FindAsync(id);

            if (food == null)
            {
                return NotFound();
            }

            return food;
        }

        // PUT: api/Foods/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFood(int id, Food food)
        {
            if (id != food.Id)
            {
                return BadRequest();
            }

            _context.Entry(food).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _dataService.SaveDataToJSON();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodExists(id))
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

        //PUT: api/Foods/5/image-url   
        [HttpPut("{id}/image-url")]
        public async Task<IActionResult> PutFoodImageUrl(int id, [FromBody] string imageUrl)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest("ImageUrl cannot be nothing");
            }

            food.ImageUrl = imageUrl;
            _context.Entry(food).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _dataService.SaveDataToJSON();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodExists(id))
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

        // POST: api/Foods
        [HttpPost]
        public async Task<ActionResult<Food>> PostFood(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            _dataService.SaveDataToJSON();

            return CreatedAtAction("GetFood", new { id = food.Id }, food);
        }

        // POST: api/WeddingHalls/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromServices] IWebHostEnvironment hostEnvironment)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var foods = await _context.Foods.FindAsync(id);

            if (foods == null)
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
            foods.ImageUrl = $"/images/weddinghalls/{fileName}";
            await _context.SaveChangesAsync();
            _dataService.SaveDataToJSON();
            return Ok(new { ImageUrl = foods.ImageUrl });
        }

        // DELETE: api/Foods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            _dataService.SaveDataToJSON();

            return NoContent();
        }

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.Id == id);
        }
    }
}
