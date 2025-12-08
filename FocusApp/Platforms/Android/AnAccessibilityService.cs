using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Widget;
using FocusApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FocusApp.Platforms.Android
{
    public class AnAccessibilityService : AccessibilityService
    {
        private List<string> _blockedApps;
        private List<string> _blockedCache;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        public override void OnAccessibilityEvent(AccessibilityEvent? e)
        {
            bool isFocusOn = Preferences.Get("is_focus_active", false);
            if (!isFocusOn) return;

            // Gets the package name
            string currentPackage = e.PackageName?.ToString();

            if (string.IsNullOrEmpty(currentPackage)) return;

            if (_blockedApps.Contains(currentPackage))
            {
                System.Diagnostics.Debug.WriteLine($"BLOCKED: User tried to open {currentPackage}");

                // Emulates home button press
                PerformGlobalAction(GlobalAction.Home);

                Toast.MakeText(ApplicationContext, "Focus Mode Active!", ToastLength.Short).Show();
            }
        }

        public override void OnInterrupt()
        {
            throw new NotImplementedException();
        }

        private void UpdateBlockedList()
        {
            // READ JSON HERE
            // Note: You need to manually read the file using standard System.IO
            string path = Path.Combine(FileSystem.AppDataDirectory, "app_data.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<AppData>(json);
                _blockedCache = data.BlockedItems.Select(x => x.Name).ToList();
                _lastCacheUpdate = DateTime.Now;
            }
        }
    }
}
