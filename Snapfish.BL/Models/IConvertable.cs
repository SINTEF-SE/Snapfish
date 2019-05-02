namespace Snapfish.BL.Models
{
    public interface IConvertable<T>
    {
        T FromArray(byte[] bytes);
    }
}