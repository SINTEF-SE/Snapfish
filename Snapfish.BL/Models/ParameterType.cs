using System;

namespace Snapfish.BL.Models
{
    public enum ParameterType
    {
        GetApplicationName,
        GetApplicationType,
        GetApplicationDescription,
        GetApplicationVersion, // ApplicationParameters
        GetChannelId,
        GetFrequency,
        GetPulseLength,
        GetSampleInterval,
        GetTransmitPower,
        AbsorptionCoefficient,
        SoundVelocity,
        TransducerName,
        EquivalentBeamAngle,
        AngleSensitivityAlongship,
        AngleSensitivityAthwartship,
        BeamWidthAlongship,
        AngleOffsetAlongship,
        Gain,
        SaCorrection,
        PingTime,
        Latitude,
        Longitude,
        Heave,
        Roll,
        Pitch,
        Distance,
        NoiseEstimate,
        ClientTimeoutLimit,
        ApplicationType
    }

    public static class ParameterTypeExtensions
    {
        public static string GetParameterName(this ParameterType me, string channelId = "")
        {
            switch (me)
            {
                case ParameterType.GetApplicationName:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationName";
                case ParameterType.GetApplicationType:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationType";
                case ParameterType.GetApplicationDescription:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationDescription";
                case ParameterType.GetApplicationVersion:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationVersion";
                case ParameterType.GetChannelId:
                    return "TransceiverMgr/Channels";
                case ParameterType.GetFrequency:
                    return "TransceiverMgr/" + channelId + "/Frequency";
                case ParameterType.GetPulseLength:
                    return "TransceiverMgr/" + channelId + "/PulseLength";
                case ParameterType.GetSampleInterval:
                    return "TransceiverMgr/" + channelId + "/SampleInterval";
                case ParameterType.GetTransmitPower:
                    return "TransceiverMgr/" + channelId + "/TransmitPower";
                case ParameterType.AbsorptionCoefficient:
                    return "TransceiverMgr/" + channelId + "/AbsorptionCofficient";
                case ParameterType.SoundVelocity:
                    return "TransceiverMgr/" + channelId + "/SoundVelocity";
                case ParameterType.TransducerName:
                    return "TransceiverMgr/" + channelId + "TransducerName";
                case ParameterType.EquivalentBeamAngle:
                    return "TransceiverMgr/" + channelId + "EquivalentBeamAngle";
                case ParameterType.AngleSensitivityAlongship:
                    return "TransceiverMgr/" + channelId + "AngleSensitivityAlongship";
                case ParameterType.BeamWidthAlongship:
                    return "TransceiverMgr/" + channelId + "BeamWidthAlongship";
                case ParameterType.AngleOffsetAlongship:
                    return "TransceiverMgr/" + channelId + "AngleOffsetAlongship";
                case ParameterType.Gain:
                    return "TransceiverMgr/" + channelId + "Gain";
                case ParameterType.SaCorrection:
                    return "TransceiverMgr/" + channelId + "SaCorrection";
                case ParameterType.PingTime:
                    return "TransceiverMgr/PingTime";
                case ParameterType.Latitude:
                    return "TransceiverMgr/Latitude";
                case ParameterType.Longitude:
                    return "TransceiverMgr/Longitude";
                case ParameterType.Heave:
                    return "TransceiverMgr/Heave";
                case ParameterType.Roll:
                    return "TransceiverMgr/Roll";
                case ParameterType.Pitch:
                    return "TransceiverMgr/Pitch";
                case ParameterType.Distance:
                    return "TransceiverMgr/Distance";
                case ParameterType.ClientTimeoutLimit:
                    return "TransceiverMgr/Distance";
                case ParameterType.NoiseEstimate:
                    return "ProcessingMgr/" + channelId + "/ChannelProcessingCommon/NoiseEstimate";
                case ParameterType.ApplicationType:
                    return "RemoteCommandDispatcher / BrowseInfoProvider / ApplicationType";
                default:
                    throw new NotImplementedException("Invalid ParameterName");
            }
        }
    }
}