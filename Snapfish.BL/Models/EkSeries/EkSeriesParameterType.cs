using System;

namespace Snapfish.BL.Models.EkSeries
{
    public enum EkSeriesParameterType
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
        ApplicationType,
    }

    public static class ParameterTypeExtensions
    {
        public static string GetParameterName(this EkSeriesParameterType me, string channelId = "")
        {
            switch (me)
            {
                case EkSeriesParameterType.GetApplicationName:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationName";
                case EkSeriesParameterType.GetApplicationType:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationType";
                case EkSeriesParameterType.GetApplicationDescription:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationDescription";
                case EkSeriesParameterType.GetApplicationVersion:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationVersion";
                case EkSeriesParameterType.GetChannelId:
                    return "TransceiverMgr/Channels";
                case EkSeriesParameterType.GetFrequency:
                    return "TransceiverMgr/" + channelId + "/Frequency";
                case EkSeriesParameterType.GetPulseLength:
                    return "TransceiverMgr/" + channelId + "/PulseLength";
                case EkSeriesParameterType.GetSampleInterval:
                    return "TransceiverMgr/" + channelId + "/SampleInterval";
                case EkSeriesParameterType.GetTransmitPower:
                    return "TransceiverMgr/" + channelId + "/TransmitPower";
                case EkSeriesParameterType.AbsorptionCoefficient:
                    return "TransceiverMgr/" + channelId + "/AbsorptionCofficient";
                case EkSeriesParameterType.SoundVelocity:
                    return "TransceiverMgr/" + channelId + "/SoundVelocity";
                case EkSeriesParameterType.TransducerName:
                    return "TransceiverMgr/" + channelId + "/TransducerName";
                case EkSeriesParameterType.EquivalentBeamAngle:
                    return "TransceiverMgr/" + channelId + "/EquivalentBeamAngle";
                case EkSeriesParameterType.AngleSensitivityAlongship:
                    return "TransceiverMgr/" + channelId + "/AngleSensitivityAlongship"; //AngleSensitivityAthwartship
                case EkSeriesParameterType.AngleSensitivityAthwartship:
                    return "TransceiverMgr/" + channelId + "/AngleSensitivityAthwartship";
                case EkSeriesParameterType.BeamWidthAlongship:
                    return "TransceiverMgr/" + channelId + "/BeamWidthAlongship";
                case EkSeriesParameterType.AngleOffsetAlongship:
                    return "TransceiverMgr/" + channelId + "/AngleOffsetAlongship";
                case EkSeriesParameterType.Gain:
                    return "TransceiverMgr/" + channelId + "/Gain";
                case EkSeriesParameterType.SaCorrection:
                    return "TransceiverMgr/" + channelId + "/SaCorrection";
                case EkSeriesParameterType.PingTime:
                    return "TransceiverMgr/PingTime";
                case EkSeriesParameterType.Latitude:
                    return "TransceiverMgr/Latitude";
                case EkSeriesParameterType.Longitude:
                    return "TransceiverMgr/Longitude";
                case EkSeriesParameterType.Heave:
                    return "TransceiverMgr/Heave";
                case EkSeriesParameterType.Roll:
                    return "TransceiverMgr/Roll";
                case EkSeriesParameterType.Pitch:
                    return "TransceiverMgr/Pitch";
                case EkSeriesParameterType.Distance:
                    return "TransceiverMgr/Distance";
                case EkSeriesParameterType.ClientTimeoutLimit:
                    return "TransceiverMgr/Distance";
                case EkSeriesParameterType.NoiseEstimate:
                    return "ProcessingMgr/" + channelId + "/ChannelProcessingCommon/NoiseEstimate";
                case EkSeriesParameterType.ApplicationType:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationType";
                default:
                    throw new NotImplementedException("Invalid ParameterName");
            }
        }
    }
}