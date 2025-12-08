using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Services
{
    public static class BlockPageGenerator
    {
        public static string GetBlockPagePath()
        {
            // path for the block html page
            string path = Path.Combine(FileSystem.AppDataDirectory, "blocked.html");

            // TODO: Edit HTML page further
            if (!File.Exists(path))
            {
                string htmlContent = @"
            <html>
            <body style='background-color:#1e1e1e; color:white; font-family:sans-serif; text-align:center; padding-top:100px;'>
                <h1 style='font-size:50px; color:#ff5252;'>FOCUS MODE ACTIVE</h1>
                <p>This site is blocked by your settings.</p>
                <p>Get back to work!</p>
            </body>
            </html>";

                File.WriteAllText(path, htmlContent);
            }

            // Return the file URI (e.g., file:///C:/Users/...)
            return new Uri(path).AbsoluteUri;
        }
    }
}
