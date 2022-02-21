﻿using System.Collections.Generic;
using System.Numerics;
using DailyDuty.Interfaces;

namespace DailyDuty.Components.Graphical;

internal class FormattedWeeklyTasks : ITaskCategoryDisplay
{
    private readonly List<ICompletable> tasks;
    List<ICompletable> ITaskCategoryDisplay.Tasks => tasks;

    public Vector4 HeaderColor
    {
        get => Service.Configuration.TodoWindowSettings.HeaderColor;
        set => Service.Configuration.TodoWindowSettings.HeaderColor = value;
    }

    public Vector4 ItemIncompleteColor
    {
        get => Service.Configuration.TodoWindowSettings.IncompleteColor;
        set => Service.Configuration.TodoWindowSettings.IncompleteColor = value;
    }

    public Vector4 ItemCompleteColor
    {
        get => Service.Configuration.TodoWindowSettings.CompleteColor;
        set => Service.Configuration.TodoWindowSettings.CompleteColor = value;
    }

    public FormattedWeeklyTasks(List<ICompletable> tasks)
    {
        this.tasks = tasks;
    }

    public string HeaderText => "Weekly Tasks";
}