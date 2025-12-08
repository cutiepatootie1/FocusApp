using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocusApp.Models;
using FocusApp.Services;
using FocusApp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Maui.Graphics.Color;

namespace FocusApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {

        // DECLARATIONS
        private readonly JsonDataService _jsonDataService;
        private readonly IFocusService _focusService;
        private readonly IAppListService _appListService;

        private AppData _currentData = new AppData();
        // OBSERVABLE PROPERTIES
        [ObservableProperty]
        private bool isBusy;
        [ObservableProperty]
        private string newUrlInput;
        [ObservableProperty]
        private bool isActive;
        [ObservableProperty]
        private string focusStatusText = "Focus Mode: OFF";
        [ObservableProperty]
        private ObservableCollection<InstalledApp> detectedApps = new();
        [ObservableProperty]
        private bool isAppPickerVisible;

        [ObservableProperty]
        private Color focusStatusColor = Colors.Gray;

        [ObservableProperty]
        private string buttonText = "Start Focus"; // Default 

        public ObservableCollection<BlockedItems> BlockedItems { get; } = new();

        public DashboardViewModel(JsonDataService DataService, IFocusService focusService, IAppListService appListService)
        {
            _jsonDataService = DataService;
            _focusService = focusService;
            _appListService = appListService;
            // sync state on load in case app restarted while focus was on
            IsActive = _focusService.IsFocusModeActive;
            InitializeAsync();
            foreach (var item in _currentData.BlockedItems) BlockedItems.Add(item);
            UpdateVisualState();
        }

        private async void InitializeAsync()
        {
            try
            {
                // Load the raw data from the JSON file
                _currentData = await _jsonDataService.LoadDataAsync();

                // Clear the list to avoid duplicates if this runs twice
                BlockedItems.Clear();

                // Transfer data from the Model to the UI Collection
                foreach (var item in _currentData.BlockedItems)
                {
                    BlockedItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                // In a real app, log this error
                Console.WriteLine($"Failed to load data: {ex.Message}");
            }
        }
        // CODE FOR DASHBOARD STUFF

        //[RelayCommand] turns this method into an interface named "AddWebsiteCommand"
        // this can be bound to a button in XAML as "Command = {Binding AddWebsiteCommand}"
        [RelayCommand]
        private async Task AddWebsiteAsync()
        {
            Console.WriteLine("$Add button clicked nigger Inputted: '{NewUrlInput}'");
            // Validation: Don't add empty strings
            if (string.IsNullOrWhiteSpace(NewUrlInput))
            {
                Console.WriteLine("Input was empty, returning.");
                return;
            }

            // Create the new model object
            var newItem = new BlockedItems
            {
                Name = NewUrlInput,
                ItemType = ItemType.Website,
                isActive = true
            };

            // Step A: Update the UI immediately so the user feels it's fast
            BlockedItems.Add(newItem);

            // Step B: Update our internal memory model
            _currentData.BlockedItems.Add(newItem);

            // Step C: Save to disk asynchronously
            // We await this so if it fails, we could technically show an alert
            await _jsonDataService.SaveDataAsync(_currentData);
            NewUrlInput = string.Empty;
        }

        [RelayCommand]
        private async Task RemoveItemAsync(BlockedItems item)
        {
            if (item == null) return;

            // 1. Remove from UI (Instant feedback)
            BlockedItems.Remove(item);

            // 2. Remove from Data Model
            _currentData.BlockedItems.Remove(item);

            // 3. Save to Disk
            await _jsonDataService.SaveDataAsync(_currentData);
        }

        // CODE FOR FOCUS MODE STUFF !=========
        [RelayCommand]
        private async Task ToggleFocusAsync()
        {
            if (IsActive)
            {
                _focusService.StopFocusMode();
                IsActive = false;
            }
            else
            {
                await _focusService.StartFocusModeAsync();
                IsActive = true;
            }
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            FocusStatusText = IsActive ? "Focus Mode: ON" : "Focus Mode: OFF";
            FocusStatusColor = IsActive ? Colors.Green : Colors.Gray;

            // Add this line:
            ButtonText = IsActive ? "Stop Focus" : "Start Focus";
        }

        [RelayCommand]
        private async Task OpenAppPickerAsync()
        {
            IsBusy = true;
            IsAppPickerVisible = true; // Show the overlay

            var apps = await _appListService.GetAppsAsync();

            DetectedApps.Clear();
            foreach (var app in apps) DetectedApps.Add(app);

            IsBusy = false;
        }

        [RelayCommand]
        private void CloseAppPicker()
        {
            IsAppPickerVisible = false;
        }

        [RelayCommand]
        private async Task SelectAppAsync(InstalledApp app)
        {
            if (app == null) return;

            // Convert the Selected App to a BlockedItem
            var newItem = new BlockedItems
            {
                Name = app.PackageId, // ID goes here (e.g., "spotify")
                ItemType = ItemType.App,
                isActive = true,
                Platform = app.Platform
            };

            // Add logic from your AddWebsiteAsync
            BlockedItems.Add(newItem);
            _currentData.BlockedItems.Add(newItem);
            await _jsonDataService.SaveDataAsync(_currentData);

            // Close picker
            IsAppPickerVisible = false;
        }
    }
}
