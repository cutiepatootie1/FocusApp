using FocusApp.Models;
using FocusApp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Platforms.Windows
{
    public class AppListService : IAppListService
    {
        public async Task<List<InstalledApp>> GetAppsAsync()
        {
            return await Task.Run(() =>
            {
                var results = new List<InstalledApp>();
                var processes = Process.GetProcesses();
                var addedIds = new HashSet<string>();

                foreach (var p in processes)
                {
                    // Filter: Must have a window title (visible app)
                    if (p.MainWindowHandle == IntPtr.Zero)
                        continue;

                    try
                    {
                        string processName = p.ProcessName.ToLower();

                        // Avoid duplicates (e.g. 10 chrome processes)
                        if (addedIds.Contains(processName)) continue;

                        var app = new InstalledApp
                        {
                            Name = p.MainWindowTitle, // "Spotify Free"
                            PackageId = processName,  // "spotify"
                            Platform = "Windows"
                        };

                        // Extract Icon
                        try
                        {
                            string path = p.MainModule?.FileName;
                            if (!string.IsNullOrEmpty(path))
                            {
                                var icon = Icon.ExtractAssociatedIcon(path);
                                if (icon != null)
                                {
                                    // Convert to Bitmap
                                    using var bmp = icon.ToBitmap();
                                    var stream = new MemoryStream();
                                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                    stream.Position = 0;

                                    // Make a copy for MAUI
                                    var byteArray = stream.ToArray();
                                    app.Icon = ImageSource.FromStream(() => new MemoryStream(byteArray));
                                }
                            }
                        }
                        catch(Exception) { app.Icon = "dotnet_bot.png"; }

                        results.Add(app);
                        addedIds.Add(processName);
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipped system process: {ex.Message}");
                        continue;
                    }
                }

                return results.OrderBy(x => x.Name).ToList();
            });
        }
    }
}
