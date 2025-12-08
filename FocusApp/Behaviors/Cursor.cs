using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using View = Microsoft.Maui.Controls.View;
using Microsoft.Maui.Controls;
#if WINDOWS
using Microsoft.UI.Input;
using FocusApp.Platforms.Windows;
#endif

namespace FocusApp.Behaviors;
    internal static class Cursor
    {
        // 1. Define the Property we will use in XAML
        public static readonly BindableProperty StyleProperty =
            BindableProperty.CreateAttached(
                "Style",
                typeof(CursorIcon),
                typeof(Cursor),
                CursorIcon.Arrow,
                propertyChanged: OnStyleChanged);

        // Standard Getters/Setters for XAML
        public static CursorIcon GetStyle(BindableObject view) => (CursorIcon)view.GetValue(StyleProperty);
        public static void SetStyle(BindableObject view, CursorIcon value) => view.SetValue(StyleProperty, value);

        // 2. What happens when you add this property to a control?
        private static void OnStyleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is View view)
            {
                // We attach a GestureRecognizer to listen for Hover events automatically
                var recognizer = new PointerGestureRecognizer();

                // On Enter: Switch to the requested cursor (e.g., Hand)
                recognizer.PointerEntered += (s, e) => SetCursor(view, (CursorIcon)newValue);

                // On Exit: Switch back to Arrow
                recognizer.PointerExited += (s, e) => SetCursor(view, CursorIcon.Arrow);

                view.GestureRecognizers.Add(recognizer);
            }
        }

        // 3. The method that calls your REFLECTION HELPER
        private static void SetCursor(VisualElement element, CursorIcon icon)
        {
#if WINDOWS
        // Map our simple Enum to the Windows Native Enum
        InputSystemCursorShape shape = icon switch
        {
            CursorIcon.Hand => InputSystemCursorShape.Hand,
            CursorIcon.IBeam => InputSystemCursorShape.IBeam,
            CursorIcon.Wait => InputSystemCursorShape.Wait,
            _ => InputSystemCursorShape.Arrow
        };

        // CALL YOUR REFLECTION HELPER HERE
        CursorExtension.SetCursor(element, shape);
#endif
        }
    }
public enum CursorIcon { Arrow, Hand, IBeam, Wait}
