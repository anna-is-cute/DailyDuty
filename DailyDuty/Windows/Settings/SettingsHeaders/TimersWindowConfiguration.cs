﻿using DailyDuty.Data.SettingsObjects.WindowSettings;
using DailyDuty.Interfaces;
using DailyDuty.Utilities;
using Dalamud.Interface;
using ImGuiNET;

namespace DailyDuty.Windows.Settings.SettingsHeaders
{
    internal class TimersWindowConfiguration : ICollapsibleHeader
    {
        private TimersWindowSettings Settings => Service.Configuration.TimersWindowSettings;
        public string HeaderText => "Timers Window Configuration";

        public void Dispose()
        {
        }

        void ICollapsibleHeader.DrawContents()
        {
            ImGui.Indent(15 * ImGuiHelpers.GlobalScale);

            ShowHideWindow();

            DisableEnableClickThrough();

            HideInDuty();

            OpacitySlider();

            ShowHideSeconds();

            UseShortStrings();
            
            ImGui.Indent(-15 * ImGuiHelpers.GlobalScale);
        }

        private void OpacitySlider()
        {
            ImGui.PushItemWidth(150);
            ImGui.DragFloat($"Opacity##{HeaderText}", ref Settings.Opacity, 0.01f, 0.0f, 1.0f);
            ImGui.PopItemWidth();
        }

        private void HideInDuty()
        {
            ImGui.Checkbox("Hide when Bound By Duty", ref Settings.HideInDuty);
        }

        private void DisableEnableClickThrough()
        {
            Draw.NotificationField("Enable Click-through", HeaderText, ref Settings.ClickThrough, "Enables/Disables the ability to move the Timers Window");
        }

        private void ShowHideWindow()
        {
            Draw.NotificationField("Show Timers Window", HeaderText, ref Settings.Open, "Shows/Hides the Timers Window");
        }
        
        private void UseShortStrings()
        {
            Draw.NotificationField("Show Less Text", HeaderText, ref Settings.ShortStrings, "Make Timer labels less verbose (Weekly Reset: nDays, HH:MM:SS -> Week: D:HH:MM:SS)");
        }

        private void ShowHideSeconds()
        {
            Draw.NotificationField("Hide Seconds", HeaderText, ref Settings.HideSeconds, "Omit Seconds from timer display (HH:MM:SS -> HH:MM)");
        }
    }
}
