using WeddingHallAPI.Models;
using WeddingHallAPI.Data;
using System.Text.Json;

namespace WeddingHallAPI.Services
{
    public interface IDataService
    {
        void SaveDataToJSON();
        void LoadDataFromJSON();
        void ClearData();
        void CheckFixData();
    }

    public class DataService : IDataService
    {
        private readonly WeddingHallDbContext _context;
        private readonly string _filePath = Path.Combine("DONT_TOUCH", "File.json");

        public DataService(WeddingHallDbContext context)
        {
            _context = context;
        }

        public void SaveDataToJSON()
        {
            var data = new
            {
                WeddingHalls = _context.WeddingHalls.ToList(),
                Foods = _context.Foods.ToList(),
                Users = _context.Users.ToList()
            };

            WeddingHallData weddingHallData = new WeddingHallData(data.WeddingHalls, data.Foods, data.Users);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            string json = JsonSerializer.Serialize(weddingHallData, options);

            if (File.Exists(_filePath))
                File.Delete(_filePath);
            
            
            if(!File.Exists(_filePath))
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath) ?? string.Empty);

            CheckFixData();
            File.WriteAllText(_filePath, json);
        }
        public void LoadDataFromJSON()
        {
            
            if (!File.Exists(_filePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath) ?? string.Empty);
                Console.WriteLine("File not found: " + _filePath);
                return;
            }


            string json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<WeddingHallData>(json);
            if (data != null)
            {
                _context.WeddingHalls.AddRange(data.WeddingHalls);
                _context.Users.AddRange(data.Users);
                _context.SaveChanges();
            }
        }
        public void ClearData()
        {
            _context.WeddingHalls.RemoveRange(_context.WeddingHalls);
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        public void CheckFixData()
        {
            var weddingHalls = _context.WeddingHalls.ToList();
            var foods = _context.Foods.ToList();
            var users = _context.Users.ToList();
            if (weddingHalls.Count == 0 && foods.Count == 0 && users.Count == 0)
                LoadDataFromJSON();
        }

        private class WeddingHallData
        {
            public List<WeddingHall> WeddingHalls { get; set; }
            public List<Food> Foods { get; set; }
            public List<User> Users { get; set; }

            public WeddingHallData()
            {
                WeddingHalls = new List<WeddingHall>();
                Foods = new List<Food>();
                Users = new List<User>();
            }

            public WeddingHallData(List<WeddingHall> weddingHalls, List<Food> foods, List<User> users)
            {
                WeddingHalls = weddingHalls;
                Foods = foods;
                Users = users;
            }
        }
    }
}
