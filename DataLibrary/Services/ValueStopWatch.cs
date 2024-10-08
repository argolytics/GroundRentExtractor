﻿// Inspired from: https://github.com/dotnet/runtime/blob/26b6e4ea97a627ab800362b2c10f32ebecea041d/src/libraries/Common/src/Extensions/ValueStopwatch/ValueStopwatch.cs
using System.Diagnostics;

namespace DataLibrary.Services;

public readonly struct ValueStopwatch
{
    private static readonly double s_timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

    private readonly long _startTimestamp;

    private ValueStopwatch(long startTimestamp) => _startTimestamp = startTimestamp;

    public static ValueStopwatch StartNew() => new ValueStopwatch(GetTimestamp());

    public static long GetTimestamp() => Stopwatch.GetTimestamp();

    public static TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
    {
        var timestampDelta = endTimestamp - startTimestamp;
        var ticks = (long)(s_timestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
    }



    public TimeSpan GetElapsedTime() => GetElapsedTime(_startTimestamp, GetTimestamp());
}