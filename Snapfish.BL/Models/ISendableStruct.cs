namespace Snapfish.BL.Models
{
    public interface ISendableStruct
    {
        byte[] ToArray();
        string GetName();
        string GetSequenceNumber();
        string GetRequestType(); //Parameter, Subscription etc
        void SetRequestType(string requestType);
        string GetMethodInvocationType(); // SV, TS , ECHOGRAM ETC
        void SetMethodInvocationType(string methodInvocationType);
    }
}