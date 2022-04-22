﻿using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace DailyDuty.Interfaces
{
    internal interface ITab
    {
        IConfigurable? SelectedTabItem { get; set; }
        List<IConfigurable> TabItems { get; set; }
        string TabName { get; }

        string Description { get; }

        public void DrawTabContents()
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Vector4.Zero);

            ImGui.BeginListBox("", new Vector2(-1, -1));

            ImGui.PopStyleColor(1);

            foreach (var item in TabItems)
            {

                ImGui.PushID(item.ConfigurationPaneLabel);

                var headerHoveredColor = ImGui.GetStyle().Colors[(int) ImGuiCol.HeaderHovered];
                var textSelectedColor = ImGui.GetStyle().Colors[(int) ImGuiCol.Header];
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, headerHoveredColor with {W = 0.1f});
                ImGui.PushStyleColor(ImGuiCol.Header, textSelectedColor with {W = 0.1f});

                if (ImGui.Selectable("", SelectedTabItem == item))
                {
                    SelectedTabItem = item;
                }

                ImGui.PopStyleColor(2);

                ImGui.SameLine();

                item.DrawTabItem();

                ImGui.Spacing();

                ImGui.PopID();
            }

            ImGui.EndListBox();
        }
    }
}
