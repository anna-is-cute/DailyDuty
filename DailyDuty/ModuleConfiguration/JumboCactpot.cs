﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyDuty.Data.ModuleSettings;
using DailyDuty.Enums;
using DailyDuty.Graphical;
using DailyDuty.Interfaces;
using DailyDuty.Localization;
using DailyDuty.Modules;
using DailyDuty.Utilities;
using Dalamud.Interface;
using ImGuiNET;
using ImGuiScene;

namespace DailyDuty.ModuleConfiguration
{
    internal class JumboCactpot : IConfigurable
    {
        public ModuleType ModuleType => ModuleType.JumboCactpot;
        public string ConfigurationPaneLabel => Strings.Module.JumboCactpotLabel;
        public InfoBox? AboutInformationBox { get; } = new()
        {
            Label = Strings.Common.InformationLabel,
            ContentsAction = () =>
            {
                ImGui.Text(Strings.Module.JumboCactpotInformation);
            }
        };

        public InfoBox? AutomationInformationBox { get; } = new()
        {
            Label = Strings.Common.AutomationInformationLabel,
            ContentsAction = () =>
            {
                ImGui.Text(Strings.Module.JumboCactpotAutomationInformation);
            }
        };

        public InfoBox? TechnicalInformation { get; } = new()
        {
            Label = Strings.Common.TechnicalInformationLabel,
            ContentsAction = () =>
            {
                ImGui.Text(Strings.Module.JumboCactpotTechnicalInformation);
            }
        };

        private readonly InfoBox completionStatus = new()
        {
            Label = Strings.Common.CompletionStatusLabel,
            ContentsAction = () =>
            {
                var module = Service.ModuleManager.GetModule<JumboCactpotModule>();
                if(module == null) return;

                if (ImGui.BeginTable($"", 2))
                {
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 125f * ImGuiHelpers.GlobalScale);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 100f * ImGuiHelpers.GlobalScale);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(Strings.Common.AllTasksLabel);

                    ImGui.TableNextColumn();
                    Draw.CompleteIncomplete(module.IsCompleted());

                    ImGui.EndTable();
                }
            }
        };

        private readonly InfoBox currentStatus = new()
        {
            Label = Strings.Module.JumboCactpotTicketsLabel,
            ContentsAction = () =>
            {
                var module = Service.ModuleManager.GetModule<JumboCactpotModule>();
                if(module == null) return;

                if (ImGui.BeginTable($"", 2))
                {
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 125f * ImGuiHelpers.GlobalScale);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 100f * ImGuiHelpers.GlobalScale);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(Strings.Module.JumboCactpotTicketsLabel);

                    ImGui.TableNextColumn();

                    ImGui.Text(Settings.Tickets.Count == 0
                        ? Strings.Common.NoneRecordedLabel
                        : string.Concat(Settings.Tickets.Select(i => $"[{i:D4}] ")));

                    ImGui.EndTable();
                }
            }
        };

        private readonly InfoBox options = new()
        {
            Label = Strings.Configuration.OptionsTabLabel,
            ContentsAction = () =>
            {
                if (Draw.Checkbox(Strings.Common.EnabledLabel, ref Settings.Enabled))
                {
                    Service.LogManager.LogMessage(ModuleType.JumboCactpot, Settings.Enabled ? "Enabled" : "Disabled");
                    Service.CharacterConfiguration.Save();
                }
            }
        };

        private readonly InfoBox notificationOptions = new()
        {
            Label = Strings.Common.NotificationOptionsLabel,
            ContentsAction = () =>
            {
                if(Draw.Checkbox(Strings.Common.NotifyOnLoginLabel, ref Settings.LoginReminder, Strings.Common.NotifyOnLoginHelpText))
                {
                    Service.LogManager.LogMessage(ModuleType.JumboCactpot, "Login Notifications " + (Settings.Enabled ? "Enabled" : "Disabled"));
                    Service.CharacterConfiguration.Save();
                }

                if(Draw.Checkbox(Strings.Common.NotifyOnZoneChangeLabel, ref Settings.ZoneChangeReminder, Strings.Common.NotifyOnZoneChangeHelpText))
                {
                    Service.LogManager.LogMessage(ModuleType.JumboCactpot, "Zone Change Notifications " + (Settings.Enabled ? "Enabled" : "Disabled"));
                    Service.CharacterConfiguration.Save();
                }
            }
        };

        public TextureWrap? AboutImage { get; }
        public TabFlags TabFlags => TabFlags.All;

        public static JumboCactpotSettings Settings => Service.CharacterConfiguration.JumboCactpot;


        public JumboCactpot()
        {
            AboutImage = Image.LoadImage("JumboCactpot");
        }

        public void DrawTabItem()
        {
            ImGui.TextColored(Settings.Enabled ? Colors.SoftGreen : Colors.SoftRed, Strings.Module.JumboCactpotLabel);

            if (Settings.Enabled)
            {
                var module = Service.ModuleManager.GetModule<JumboCactpotModule>();
                if(module == null) return;

                Draw.CompleteIncompleteRightAligned(module.IsCompleted());
            }
        }

        public void DrawStatusContents()
        {
            ImGuiHelpers.ScaledDummy(10.0f);
            completionStatus.DrawCentered();

            ImGuiHelpers.ScaledDummy(30.0f);
            currentStatus.DrawCentered();

            ImGuiHelpers.ScaledDummy(20.0f);
        }

        public void DrawOptionsContents()
        {
            ImGuiHelpers.ScaledDummy(10.0f);
            options.DrawCentered();

            ImGuiHelpers.ScaledDummy(30.0f);
            notificationOptions.DrawCentered();

            ImGuiHelpers.ScaledDummy(20.0f);
        }
    }
}
