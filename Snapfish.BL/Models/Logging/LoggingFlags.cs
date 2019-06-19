using System;

namespace Snapfish.BL.Models.Logging
{
    [Flags]
    public enum LoggingFlags
    {
        Emergency = 1 << 0,
        Alert = 1 << 1,
        Critical = 1 << 2,
        Error = 1 << 3,
        Warn = 1 << 4,
        Notice = 1 << 5,
        Info = 1 << 6,
        Debug = 1 << 7,

        Iemergency = Emergency | Emergency - 1,
        Ialert = Alert | Alert - 1,
        Icritical = Critical | Critical - 1,
        Ierror = Error | Error - 1,
        Iwarn = Warn | Warn - 1,
        Inotice = Notice | Notice - 1,
        Iinfo = Info | Info - 1,
        Idebug = Debug | Debug - 1,
        All = -1,
        Non = 0,
        Default = Iwarn
    }
}