namespace Snapfish.BL.Models
{
    public interface ISendableStruct
    {
        byte[] ToArray();
        string GetName();
        string GetSequenceNumber();
    }
}