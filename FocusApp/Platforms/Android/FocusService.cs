using FocusApp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Platforms.Android
{
    public class FocusService : IFocusService
    {
        public bool IsFocusModeActive => Preferences.Get("is_focus_active", false );
        
        public Task StartFocusModeAsync()
        {
            Preferences.Set("is_focus_active", true);
            return Task.CompletedTask;
        }

        public void StopFocusMode()
        {
            Preferences.Set("is_focus_active", false);
        }

    }
}
