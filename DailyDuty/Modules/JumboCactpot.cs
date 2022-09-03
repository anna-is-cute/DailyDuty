﻿using System;
using System.Linq;
using DailyDuty.Addons.DataModels;
using DailyDuty.Addons.Enums;
using DailyDuty.Configuration.Components;
using DailyDuty.Configuration.Enums;
using DailyDuty.Configuration.ModuleSettings;
using DailyDuty.Interfaces;
using DailyDuty.Localization;
using DailyDuty.System;
using DailyDuty.UserInterface.Components;
using DailyDuty.UserInterface.Components.InfoBox;
using DailyDuty.UserInterface.Windows;
using DailyDuty.Utilities;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace DailyDuty.Modules;

internal class JumboCactpot : IModule
{
    public ModuleName Name => ModuleName.JumboCactpot;
    public IConfigurationComponent ConfigurationComponent { get; }
    public IStatusComponent StatusComponent { get; }
    public ILogicComponent LogicComponent { get; }
    public ITodoComponent TodoComponent { get; }
    public ITimerComponent TimerComponent { get; }

    private static JumboCactpotSettings Settings => Service.ConfigurationManager.CharacterConfiguration.JumboCactpot;
    public GenericSettings GenericSettings => Settings;

    public JumboCactpot()
    {
        ConfigurationComponent = new ModuleConfigurationComponent(this);
        StatusComponent = new ModuleStatusComponent(this);
        LogicComponent = new ModuleLogicComponent(this);
        TodoComponent = new ModuleTodoComponent(this);
        TimerComponent = new ModuleTimerComponent(this);
    }

    public void Dispose()
    {
        LogicComponent.Dispose();
    }

    private class ModuleConfigurationComponent : IConfigurationComponent
    {
        public IModule ParentModule { get; }
        public ISelectable Selectable => new ConfigurationSelectable(ParentModule, this);

        private readonly InfoBox optionsInfoBox = new();
        private readonly InfoBox clickableLink = new();
        private readonly InfoBox notificationOptionsInfoBox = new();

        public ModuleConfigurationComponent(IModule parentModule)
        {
            ParentModule = parentModule;
        }

        public void Draw()
        {
            optionsInfoBox
                .AddTitle(Strings.Configuration.Options)
                .AddConfigCheckbox(Strings.Common.Enabled, Settings.Enabled)
                .Draw();

            clickableLink
                .AddTitle(Strings.Module.JumboCactpot.ClickableLinkLabel)
                .AddString(Strings.Module.JumboCactpot.ClickableLink)
                .AddConfigCheckbox(Strings.Common.Enabled, Settings.EnableClickableLink)
                .Draw();

            notificationOptionsInfoBox
                .AddTitle(Strings.Configuration.NotificationOptions)
                .AddConfigCheckbox(Strings.Configuration.OnLogin, Settings.NotifyOnLogin)
                .AddConfigCheckbox(Strings.Configuration.OnZoneChange, Settings.NotifyOnZoneChange)
                .Draw();
        }
    }

    private class ModuleStatusComponent : IStatusComponent
    {
        public IModule ParentModule { get; }

        public ISelectable Selectable => new StatusSelectable(ParentModule, this, ParentModule.LogicComponent.GetModuleStatus);

        private readonly InfoBox status = new();
        private readonly InfoBox nextDrawing = new();

        public ModuleStatusComponent(IModule parentModule)
        {
            ParentModule = parentModule;
        }

        public void Draw()
        {
            if (ParentModule.LogicComponent is not ModuleLogicComponent logicModule) return;

            var moduleStatus = logicModule.GetModuleStatus();


            status
                .AddTitle(Strings.Status.Label)
                .BeginTable()

                .AddRow(
                    Strings.Status.ModuleStatus,
                    moduleStatus.GetTranslatedString(),
                    secondColor: moduleStatus.GetStatusColor())

                .AddRow(Strings.Module.JumboCactpot.Tickets,
                    Settings.Tickets.Count == 0 ? Strings.Module.JumboCactpot.NoTickets : logicModule.GetTicketsString()
                )

                .EndTable()
                .Draw();

            nextDrawing
                .AddTitle(Strings.Module.JumboCactpot.NextDrawing)
                .BeginTable()
                .AddRow(
                    Strings.Module.JumboCactpot.NextDrawing,
                    logicModule.GetNextJumboCactpot()
                    )
                .EndTable()
                .Draw();
        }
    }

    private unsafe class ModuleLogicComponent : ILogicComponent
    {
        public IModule ParentModule { get; }
        public DalamudLinkPayload? DalamudLinkPayload { get; } = Service.TeleportManager.GetPayload(TeleportLocation.GoldSaucer);

        private int ticketData = -1;

        public ModuleLogicComponent(IModule parentModule)
        {
            ParentModule = parentModule;

            Service.GoldSaucerEventManager.OnGoldSaucerUpdate += GoldSaucerUpdate;
            Service.AddonManager[AddonName.JumboCactpot].OnReceiveEvent += OnOnReceiveEvent;
        }

        public void Dispose()
        {
            Service.GoldSaucerEventManager.OnGoldSaucerUpdate -= GoldSaucerUpdate;
            Service.AddonManager[AddonName.JumboCactpot].OnReceiveEvent -= OnOnReceiveEvent;
        }

        public string GetStatusMessage() => $"{3 - Settings.Tickets.Count} {Strings.Module.JumboCactpot.TicketsAvailable}";

        public DateTime GetNextReset() => Time.NextJumboCactpotReset();

        public void DoReset() => Settings.Tickets.Clear();

        public ModuleStatus GetModuleStatus() => Settings.Tickets.Count == 3 ? ModuleStatus.Complete : ModuleStatus.Incomplete;

        public string GetTicketsString()
        {
            return string.Join(" ", Settings.Tickets.Select(num => string.Format($"[{num:D4}]")));
        }

        private void OnOnReceiveEvent(object? sender, ReceiveEventArgs e)
        {
            var data = e.EventArgs->Int;

            switch (e.SenderID)
            {
                // Message is from JumboCactpot
                case 0 when data >= 0:
                    ticketData = data;
                    break;

                // Message is from SelectYesNo
                case 5:
                    switch (data)
                    {
                        case -1:
                        case 1:
                            ticketData = -1;
                            break;

                        case 0 when ticketData >= 0:
                            Settings.Tickets.Add(ticketData);
                            ticketData = -1;
                            Service.ConfigurationManager.Save();
                            break;
                    }
                    break;
            }
        }

        private void GoldSaucerUpdate(object? sender, GoldSaucerEventArgs e)
        {
            //1010446 Jumbo Cactpot Broker
            if (Service.TargetManager.Target?.DataId != 1010446) return;
            Settings.Tickets.Clear();

            for(var i = 0; i < 3; ++i)
            {
                var ticketValue = e.Data[i + 2];

                if (ticketValue != 10000)
                {
                    Settings.Tickets.Add(ticketValue);
                }
            }

            Service.ConfigurationManager.Save();
        }

        public string GetNextJumboCactpot()
        {
            var span = Time.NextJumboCactpotReset() - DateTime.UtcNow;

            return TimersOverlayWindow.FormatTimespan(span, TimerStyle.Full);
        }
    }

    private class ModuleTodoComponent : ITodoComponent
    {
        public IModule ParentModule { get; }
        public CompletionType CompletionType => CompletionType.Weekly;
        public bool HasLongLabel => false;

        public ModuleTodoComponent(IModule parentModule)
        {
            ParentModule = parentModule;
        }

        public string GetShortTaskLabel() => Strings.Module.JumboCactpot.Label;

        public string GetLongTaskLabel() => Strings.Module.JumboCactpot.Label;
    }


    private class ModuleTimerComponent : ITimerComponent
    {
        public IModule ParentModule { get; }

        public ModuleTimerComponent(IModule parentModule)
        {
            ParentModule = parentModule;
        }

        public TimeSpan GetTimerPeriod() => TimeSpan.FromDays(7);

        public DateTime GetNextReset() => Time.NextJumboCactpotReset();
    }
}