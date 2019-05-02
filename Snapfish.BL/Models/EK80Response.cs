using System.IO;

namespace Snapfish.BL.Models
{
    public struct EK80Response: IConvertable<EK80Response>
    {
        public char[] Header; //"RES\0"
        public char[] Request; //4
        public char[] MsgControl; // 22
        public char[] MsgResponse; //1400
        
        public EK80Response FromArray(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            EK80Response s = default(EK80Response);
            s.Header = reader.ReadChars(4);
            s.Request = reader.ReadChars(4);
            s.MsgControl = reader.ReadChars(22);
            s.MsgResponse = reader.ReadChars(1400);
            return s;
        }

        public string GetResponse()
        {
            string retval = "";
            if (MsgResponse != null && MsgResponse.Length > 0)
            {
                retval = new string(MsgResponse);
            }

            return retval;
        }
        
    }
}