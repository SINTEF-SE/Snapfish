using System;

namespace Snapfish.BL.Models
{
    public enum ParameterType
    {
        GetApplicationName, GetChannelId, ClientTimeoutLimit, ApplicationType
        
    }

    public static class ParameterTypeExtensions
    {
        public static string GetParameterName(this ParameterType me)
        {
            switch (me)
            {
                case ParameterType.GetApplicationName:
                    return "RemoteCommandDispatcher/BrowseInfoProvider/ApplicationName";
                case ParameterType.GetChannelId:
                    return "TransceiverMgr/Channels";
                case ParameterType.ClientTimeoutLimit:
                    return "RemoteCommandDispatcher/ClientTimeoutLimit";
                case ParameterType.ApplicationType:
                    return "RemoteCommandDispatcher / BrowseInfoProvider / ApplicationType";
                default:
                    throw new NotImplementedException("Invalid ParameterName");
            }       
        }
    }
}