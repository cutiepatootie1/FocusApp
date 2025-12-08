using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using FocusApp.Models;
using System.Threading.Tasks;

namespace FocusApp.Services
{
    public class JsonDataService
    {
        // Get the path to the file in the internal private storage
        private readonly string _filePath = Path.Combine(FileSystem.AppDataDirectory, "app_data.json");

        // Save Data
        public async Task SaveDataAsync(AppData data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true // Makes the JSON human-readable 
                });

                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        // Load Data
        public async Task<AppData> LoadDataAsync()
        {
            if (!File.Exists(_filePath))
            {
                // If file doesn't exist (first run), return a new empty object
                return new AppData();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new AppData(); // Return safe default on error
            }
        }
    }
}
