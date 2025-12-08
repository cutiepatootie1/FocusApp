using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FocusApp.Models;

namespace FocusApp.Services.Interface
{
    public interface IAppListService
    {
        Task<List<InstalledApp>> GetAppsAsync();
    }
}
