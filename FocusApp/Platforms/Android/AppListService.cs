using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidBitmap = Android.Graphics.Bitmap;
using AndroidCanvas = Android.Graphics.Canvas;
using AndroidConfig = Android.Graphics.Bitmap.Config;
using AndroidDrawable = Android.Graphics.Drawables.Drawable;
using AndroidBitmapDrawable = Android.Graphics.Drawables.BitmapDrawable;
using FocusApp.Models;
using FocusApp.Services.Interface;
using Android.Content.PM;
using Android.Graphics;

namespace FocusApp.Platforms.Android
{
    public class AppListService : IAppListService
    {
        public Task<List<InstalledApp>> GetAppsAsync()
        {
            return Task.Run(() =>
            {
                var results = new List<InstalledApp>();
                var context = Platform.CurrentActivity.ApplicationContext;
                var packageManager = context.PackageManager;

                // Get ALL installed apps
                var apps = packageManager.GetInstalledApplications(PackageInfoFlags.MetaData);

                foreach (var appInfo in apps)
                {
                    // Filter: Ignore System Apps (unless you want to allow blocking Settings, etc.)
                    // (appInfo.Flags & ApplicationInfoFlags.System) == 0 checks if it is user-installed
                    bool isSystemApp = (appInfo.Flags & ApplicationInfoFlags.System) != 0;

                    // Optional: Allow updating system apps like Chrome/YouTube
                    bool isUpdatedSystemApp = (appInfo.Flags & ApplicationInfoFlags.UpdatedSystemApp) != 0;

                    if (isSystemApp && !isUpdatedSystemApp) continue;

                    var app = new InstalledApp
                    {
                        Name = appInfo.LoadLabel(packageManager),
                        PackageId = appInfo.PackageName,
                        Platform = "Android"
                    };

                    // Convert Android Drawable to MAUI ImageSource
                    try
                    {
                        var drawable = appInfo.LoadIcon(packageManager);
                        var bitmap = DrawableToBitmap(drawable);

                        // Convert Android Bitmap to MAUI ImageSource
                        using (var stream = new MemoryStream())
                        {
                            bitmap.Compress(AndroidBitmap.CompressFormat.Png, 100, stream);
                            stream.Position = 0;
                            var byteArray = stream.ToArray();
                            app.Icon = ImageSource.FromStream(() => new MemoryStream(byteArray));
                        }
                    }
                    catch
                    {
                        // Fallback if icon fails
                        app.Icon = "dotnet_bot.png";
                    }

                    results.Add(app);
                }

                return results.OrderBy(x => x.Name).ToList();
            });
        }
        // Helper method to handle Adaptive Icons
        private AndroidBitmap DrawableToBitmap(AndroidDrawable drawable)
        {
            if (drawable is AndroidBitmapDrawable bitmapDrawable)
            {
                if (bitmapDrawable.Bitmap != null) return bitmapDrawable.Bitmap;
            }

            // If it's not a simple bitmap (e.g. AdaptiveIconDrawable), draw it onto a canvas
            var bitmap = AndroidBitmap.CreateBitmap(
                drawable.IntrinsicWidth <= 0 ? 1 : drawable.IntrinsicWidth,
                drawable.IntrinsicHeight <= 0 ? 1 : drawable.IntrinsicHeight,
                AndroidConfig.Argb8888);

            var canvas = new AndroidCanvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            return bitmap;
        }
    }
}
