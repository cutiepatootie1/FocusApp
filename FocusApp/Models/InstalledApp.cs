using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Models
{
    public class InstalledApp
    {
        public string Name { get; set; }      // "Spotify"
        public string PackageId { get; set; } // "com.spotify..." or "spotify"
        public ImageSource Icon { get; set; } // The visual icon
        public string Platform { get; set; }  // "Android" or "Windows"
    }
}
