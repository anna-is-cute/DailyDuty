﻿using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using System.Numerics;
using DailyDuty.Commands;
using DailyDuty.DataModels;
using DailyDuty.Localization;
using KamiLib;
using KamiLib.Configuration;
using KamiLib.InfoBoxSystem;

namespace DailyDuty.UserInterface.Windows;

public class TimersOverlaySettings
{
    public Setting<bool> Enabled = new(false);
    public Setting<bool> HideWhileInDuty = new(true);
    public Setting<bool> LockWindowPosition = new(false);
    public Setting<bool> AutoResize = new(true);
    public Setting<bool> HideCompleted = new(false);
    public Setting<float> Opacity = new(1.0f);
    public Setting<TimersOrdering> Ordering = new(TimersOrdering.Alphabetical);
}

internal class TimersConfigurationWindow : Window
{
    private static TimersOverlaySettings Settings => Service.ConfigurationManager.CharacterConfiguration.TimersOverlay;

    public TimersConfigurationWindow() : base("DailyDuty Timers Configuration", ImGuiWindowFlags.AlwaysVerticalScrollbar)
    {
        KamiCommon.CommandManager.AddCommand(new TimersWindowCommand());
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 350),
            MaximumSize = new Vector2(9999, 9999)
        };
    }
    
    public override void PreOpenCheck()
    {
        if (!Service.ConfigurationManager.CharacterDataLoaded) IsOpen = false;
        if (Service.ClientState.IsPvP) IsOpen = false;
    }

    public override void Draw()
    {
        InfoBox.Instance
            .AddTitle(Strings.UserInterface.Timers.MainOptions)
            .AddConfigCheckbox(Strings.Common.Enabled, Settings.Enabled)
            .AddConfigCheckbox(Strings.UserInterface.Timers.HideCompleted, Settings.HideCompleted)
            .Draw();

        var enabledModules = Service.ModuleManager.GetTimerComponents()
            .Where(module => module.ParentModule.GenericSettings.Enabled);

        InfoBox.Instance
            .AddTitle(Strings.UserInterface.Timers.Label)
            .BeginTable(0.65f)
            .AddRows(enabledModules, Strings.UserInterface.Todo.NoTasksEnabled)
            .EndTable()
            .Draw();
        
        InfoBox.Instance
            .AddTitle(Strings.UserInterface.Timers.Ordering)
            .AddConfigRadio(TimersOrdering.Alphabetical.GetTranslatedString(), Settings.Ordering, TimersOrdering.Alphabetical)
            .AddConfigRadio(TimersOrdering.AlphabeticalDescending.GetTranslatedString(), Settings.Ordering, TimersOrdering.AlphabeticalDescending)
            .AddConfigRadio(TimersOrdering.TimeRemaining.GetTranslatedString(), Settings.Ordering, TimersOrdering.TimeRemaining)
            .AddConfigRadio(TimersOrdering.TimeRemainingDescending.GetTranslatedString(), Settings.Ordering, TimersOrdering.TimeRemainingDescending)
            .Draw();
        
        InfoBox.Instance
            .AddTitle(Strings.UserInterface.Timers.WindowOptions, out var innerWidth)
            .AddConfigCheckbox(Strings.UserInterface.Timers.HideWindowInDuty, Settings.HideWhileInDuty)
            .AddConfigCheckbox(Strings.UserInterface.Timers.LockWindow, Settings.LockWindowPosition)
            .AddConfigCheckbox(Strings.UserInterface.Timers.AutoResize, Settings.AutoResize)
            .AddDragFloat(Strings.UserInterface.Timers.Opacity, Settings.Opacity, 0.0f, 1.0f, innerWidth / 2.0f)
            .Draw();
    }

    public override void OnClose()
    {
        Service.ConfigurationManager.Save();
    }
}