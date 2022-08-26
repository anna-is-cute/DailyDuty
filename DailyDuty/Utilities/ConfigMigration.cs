﻿using System;
using DailyDuty.Configuration;
using DailyDuty.Configuration.Components;
using DailyDuty.Configuration.Enums;
using DailyDuty.Configuration.ModuleSettings;
using DailyDuty.DataStructures;
using Newtonsoft.Json.Linq;

namespace DailyDuty.Utilities;

internal static class ConfigMigration
{
    private static JObject? _parsedJson;

    public static CharacterConfiguration Convert(string fileText)
    {
        _parsedJson = JObject.Parse(fileText);

        return new CharacterConfiguration
        {
            Version = 2,
            CharacterData = GetCharacterData(),
            
            BeastTribe = GetBeastTribe(),
            CustomDelivery = GetCustomDelivery(),
            DomanEnclave = GetDomanEnclave(),
            DutyRoulette = GetDutyRoulette(),
            FashionReport = GetFashionReport(),
            HuntMarksDaily = GetHuntMarksDaily(),
            HuntMarksWeekly = GetHuntMarksWeekly(),
        };
    }

    private static HuntMarksWeeklySettings GetHuntMarksWeekly()
    {
        return new HuntMarksWeeklySettings
        {
            Enabled = GetSettingValue<bool>("WeeklyHuntMarks.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("WeeklyHuntMarks.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("WeeklyHuntMarks.LoginReminder"),
            TodoUseLongLabel = GetSettingValue<bool>("WeeklyHuntMarks.ExpandedDisplay"),
            TrackedHunts = GetTrackedHunts("WeeklyHuntMarks.TrackedHunts"),
        };
    }

    private static HuntMarksDailySettings GetHuntMarksDaily()
    {
        return new HuntMarksDailySettings
        {
            Enabled = GetSettingValue<bool>("DailyHuntMarks.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("DailyHuntMarks.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("DailyHuntMarks.LoginReminder"),
            TodoUseLongLabel = GetSettingValue<bool>("DailyHuntMarks.ExpandedDisplay"),
            TrackedHunts = GetTrackedHunts("DailyHuntMarks.TrackedHunts"),
        };
    }

    private static FashionReportSettings GetFashionReport()
    {
        return new FashionReportSettings
        {
            Enabled = GetSettingValue<bool>("FashionReport.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("FashionReport.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("FashionReport.LoginReminder"),
            EnableClickableLink = GetSettingValue<bool>("FashionReport.EnableClickableLink"),
            Mode = GetSettingEnum<FashionReportMode>("FashionReport.Mode"),
        };
    }

    private static DutyRouletteSettings GetDutyRoulette()
    {
        return new DutyRouletteSettings
        {
            Enabled = GetSettingValue<bool>("DutyRoulette.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("DutyRoulette.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("DutyRoulette.LoginReminder"),
            EnableClickableLink = GetSettingValue<bool>("DutyRoulette.EnableClickableLink"),
            HideExpertWhenCapped = GetSettingValue<bool>("DutyRoulette.HideWhenCapped"),
            TodoUseLongLabel = GetSettingValue<bool>("DutyRoulette.ExpandedDisplay"),
            TrackedRoulettes = GetTrackedRoulettes("DutyRoulette.TrackedRoulettes"),
        };
    }

    private static CharacterData GetCharacterData()
    {
        return new CharacterData
        {
            LocalContentID = GetValue<ulong>("LocalContentID"),
            Name = GetValue<string>("CharacterName"),
            World = GetValue<string>("World"),
        };
    }

    private static DomanEnclaveSettings GetDomanEnclave()
    {
        return new DomanEnclaveSettings
        {
            EnableClickableLink = GetSettingValue<bool>("DomanEnclave.EnableClickableLink"),
            Enabled = GetSettingValue<bool>("DomanEnclave.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("DomanEnclave.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("DomanEnclave.LoginReminder"),
        };
    }

    private static CustomDeliverySettings GetCustomDelivery()
    {
        return new CustomDeliverySettings
        {
            NotificationThreshold = GetSettingValue<int>("CustomDelivery.NotificationThreshold"),
            Enabled = GetSettingValue<bool>("CustomDelivery.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("CustomDelivery.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("CustomDelivery.LoginReminder"),
            ComparisonMode = GetSettingEnum<ComparisonMode>("CustomDelivery.ComparisonMode"),
        };
    }

    private static BeastTribeSettings GetBeastTribe()
    {
        return new BeastTribeSettings
        {
            NotificationThreshold = GetSettingValue<int>("BeastTribe.NotificationThreshold"),
            Enabled = GetSettingValue<bool>("BeastTribe.Enabled"),
            NotifyOnZoneChange = GetSettingValue<bool>("BeastTribe.ZoneChangeReminder"),
            NotifyOnLogin = GetSettingValue<bool>("BeastTribe.LoginReminder"),
            ComparisonMode = GetSettingEnum<ComparisonMode>("BeastTribe.ComparisonMode"),
        };
    }

    private static Setting<T> GetSettingValue<T>(string key) where T : struct
    {
        return new Setting<T>(_parsedJson!.SelectToken(key)!.Value<T>());
    }

    private static Setting<T> GetSettingEnum<T>(string key) where T : struct
    {
        var readValue = _parsedJson!.SelectToken(key)!.Value<int>();

        return new Setting<T>((T) Enum.ToObject(typeof(T), readValue));
    }

    private static T GetValue<T>(string key)
    {
        return _parsedJson!.SelectToken(key)!.Value<T>()!;
    }

    private static JArray GetArray(string key)
    {
        return (JArray) _parsedJson!.SelectToken(key)!;
    }

    private static TrackedRoulette[] GetTrackedRoulettes(string key)
    {
        var array = GetArray(key);

        var resultArray = new TrackedRoulette[array.Count];

        for (var i = 0; i < array.Count; ++i)
        {
            var element = array[i];

            var tracked = element["Tracked"]!.Value<bool>();
            var completed = element["Completed"]!.Value<bool>();
            var type = element["Type"]!.Value<int>();

            resultArray[i] = new TrackedRoulette((RouletteType)type, new Setting<bool>(tracked), completed);
        }

        return resultArray;
    }

    private static TrackedHunt[] GetTrackedHunts(string key)
    {
        var array = GetArray(key);

        var resultArray = new TrackedHunt[array.Count];

        for (var i = 0; i < array.Count; ++i)
        {
            var element = array[i];

            var tracked = element["Tracked"]!.Value<bool>();
            var state = element["State"]!.Value<int>();
            var type = element["Type"]!.Value<int>();

            resultArray[i] = new TrackedHunt((HuntMarkType)type, (TrackedHuntState)state, new Setting<bool>(tracked));
        }

        return resultArray;
    }
}