﻿using System;
using System.Collections.Generic;
using Orcus.Server.Connection.Tasks.Triggers;

namespace TasksCore.Services.Shared.Transmission
{
    public class ScheduledTriggerInfo : TriggerInfo
    {
        public DateTimeOffset StartTime { get; set; }
        public ScheduleMode ScheduleMode { get; set; }
        public int RepetitionInterval { get; set; }
        public List<DayOfWeek> Days { get; set; }
    }

    public enum ScheduleMode
    {
        Once,
        Daily,
        Weekly,
        Monthly
    }
}