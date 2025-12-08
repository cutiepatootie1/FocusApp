using Microsoft.Maui.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Platforms.Windows;

    internal static class CursorExtension
    {
        public static void SetCursor(VisualElement element, InputSystemCursorShape cursorShape)
        {
            // 1. Get the native Windows control (works for Border, Grid, Button, etc.)
            if (element.Handler?.PlatformView is UIElement nativeView)
            {
                // 2. Use Reflection to access the hidden property
                var property = typeof(UIElement).GetProperty("ProtectedCursor", BindingFlags.NonPublic | BindingFlags.Instance);

                // 3. Set the cursor
                var cursor = InputSystemCursor.Create(cursorShape);
                property?.SetValue(nativeView, cursor);
            }
        }
    }

