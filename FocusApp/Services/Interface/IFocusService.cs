using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Services.Interface
{
    public interface IFocusService
    {
        Task StartFocusModeAsync();

        void StopFocusMode();

        bool IsFocusModeActive { get; }
    }
}
