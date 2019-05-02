namespace Snapfish.BL.Models
{
    public struct RequestFailure
    {
        public string ResponseType { get; set; }
        public int SequenceNumber { get; set; }
        public int ErrorCode { get; set; }
        public int ErrorCode1 { get; set; }
    }
}