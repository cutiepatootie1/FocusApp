using FocusApp.Services;
using FocusApp.Services.Interface;
using FocusApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Clipboard = Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
#if WINDOWS
using Data = Windows.Data.Xml.Dom;
using Notifications = Windows.UI.Notifications;
#endif

#if WINDOWS
using GregsStack.InputSimulatorStandard;
using VirtualKeyCode = GregsStack.InputSimulatorStandard.Native.VirtualKeyCode;
#endif

namespace FocusApp.Platforms.Windows
{
    public class FocusService : IFocusService
    {
        private readonly JsonDataService _jsonDataService;
        private System.Timers.Timer _timer;
        private string _blockPageUrl;

        public FocusService(JsonDataService jsonDataService)
        {
            _jsonDataService = jsonDataService;
        }

        // These hold the "Active" block list loaded from your JSON
        private List<string> _blockedWebsites = new();
        private List<string> _blockedApps = new();

        // Static list of browsers
        private readonly HashSet<string> _browserProcessNames = new()
        {
            "chrome", "msedge", "firefox", "opera", "brave", "vivaldi"
        };

        // Win32 API to get window title
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        public bool IsFocusModeActive { get; private set; }

        public async Task StartFocusModeAsync()
        {
            // 1. Load data from JSON into Memory Cache
            _blockPageUrl = BlockPageGenerator.GetBlockPagePath();
            var data = await _jsonDataService.LoadDataAsync();

            // 1. FIX: Check for null BEFORE accessing properties
            if (data == null || data.BlockedItems == null)
            {
                System.Diagnostics.Debug.WriteLine("--- [ERROR] DATA IS NULL ---");
                return;
            }

            // (Do not use a generic _blockedCache, use the ones the loop checks)
            _blockedWebsites.Clear();
            _blockedApps.Clear();

            foreach (var item in data.BlockedItems)
            {
                if (!item.isActive) continue;

                string rawName = item.Name.ToLower().Trim();

                if (item.ItemType == ItemType.Website)
                {
                    // === 3. OPTIMIZATION: Clean the URL here ===

                    // Remove Protocol
                    if (rawName.Contains("://"))
                        rawName = rawName.Split(new[] { "://" }, StringSplitOptions.None)[1];

                    // Remove "www."
                    rawName = rawName.Replace("www.", "");

                    // Remove TLD (.com) 
                    if (rawName.Contains('.'))
                        rawName = rawName.Split('.')[0];

                    _blockedWebsites.Add(rawName);
                    System.Diagnostics.Debug.WriteLine($"Added Website to Block List: '{rawName}'");
                }
                else
                {
                    _blockedApps.Add(rawName);
                    System.Diagnostics.Debug.WriteLine($"Added App to Block List: '{rawName}'");
                }
            }
            IsFocusModeActive = true;

            System.Diagnostics.Debug.WriteLine($"Starting Timer... Websites: {_blockedWebsites.Count}, Apps: {_blockedApps.Count}");

            // 4. Start the Timer
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += CheckForBlockedWindows;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void StopFocusMode()
        {
            IsFocusModeActive = false;
            _timer?.Stop();
        }
        // LOGIC FOR CHECKING BLOCKED STUFF
        private async void CheckForBlockedWindows(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("START OF BLOCKING CHECKS");
            Process[] processes = Process.GetProcesses();

            // 1. Check if lists are actually populated (DEBUGGING)
            if (_blockedWebsites.Count == 0 && _blockedApps.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Block lists are empty. Nothing to check.");
                return;
            }

            foreach (Process process in processes)
            {
                if (process.MainWindowHandle == IntPtr.Zero) continue;

                string processName = "";
                string windowTitle = "";

                try
                {
                    processName = process.ProcessName.ToLower();
                    windowTitle = process.MainWindowTitle.ToLower();
                }
                catch(Exception)
                {
                    continue;
                }

                if (_browserProcessNames.Contains(processName))
                {
                    foreach (var site in _blockedWebsites)
                    {
                        string searchTerm = site.ToLower().Trim();

                        if (searchTerm.Contains("://"))
                        {
                            searchTerm = searchTerm.Split(new[] { "://" }, StringSplitOptions.None)[1];
                        }

                        searchTerm = searchTerm.Replace("www.", "");

                        int dotIndex = searchTerm.IndexOf('.');
                        if (dotIndex > 0)
                        {
                            searchTerm = searchTerm.Substring(0, dotIndex);
                        }

                        if (windowTitle.Contains(searchTerm) && !windowTitle.Contains("focus mode"))
                        {
                            System.Diagnostics.Debug.WriteLine("================== MATCH DETECTED =========================");
                            RedirectBrowser(process.MainWindowHandle);
                            break;
                        }
                    }
                }
                else
                {
                    // APP LOGIC
                    foreach (var app in _blockedApps)
                    {
                        if (processName.Contains(app))
                        {
                            System.Diagnostics.Debug.WriteLine($"Found forbidden app---'{processName}'");
                            try
                            {
                                // 4. Kill it immediately
                                process.Kill();
                                ShowWindowsSystemToast("App Blocked", "Get back to work.");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Could not kill {processName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        private void RedirectBrowser(IntPtr handle)
        {
            // 1. Bring Browser to Front
            SetForegroundWindow(handle);
            Thread.Sleep(50); // Wait for focus

            // 2. Copy Block Page URL to Clipboard (Fast & Reliable)
            // Needs to run this on the UI thread because Clipboard is a UI concept
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Clipboard.SetTextAsync(_blockPageUrl);
            });

            // Wait slightly for clipboard to update
            Thread.Sleep(50);

            // 3. Simulate User Actions:
            // Alt + D (Focus Address Bar) -> Ctrl + V (Paste) -> Enter

            System.Diagnostics.Debug.WriteLine("Simulating keys");
            // Simulate Alt + D to focus address bar
            InputSimulator sim = new();
            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.VK_D);

            // ctrl + v
            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

            // enter
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        private void ShowWindowsSystemToast(string title, string body)
        {
#if WINDOWS
            var toastXml = $@"
    <toast>
        <visual>
            <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{body}</text>
            </binding>
        </visual>
    </toast>";

            var xmlDoc = new Data.XmlDocument();
            xmlDoc.LoadXml(toastXml);

            var toast = new Notifications.ToastNotification(xmlDoc);
            Notifications.ToastNotificationManager.CreateToastNotifier().Show(toast);
#endif
        }
    }
}
