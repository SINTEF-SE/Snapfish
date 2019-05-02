using System.IO;

namespace Snapfish.BL.Models
{
    public struct CommandRequest : ISendableStruct
    {
        public char[] Header; // REQ\0
        public char[] MsgControl; //22
        public char[] MsgRequest; //1400
        
        
        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(Header);
            if (MsgControl.Length < 22)
            {
                string control = new string(MsgControl);
                while (control.Length < 21)
                {
                    control += " ";
                }

                control += "\0";
                MsgControl = control.ToCharArray();
            }
            writer.Write(MsgControl);
            writer.Write(MsgRequest);
            return stream.ToArray();
        }

        public string GetName()
        {
            return "COMMAND REQUEST";
        }

        public string GetSequenceNumber()
        {
            return new string(MsgControl);
        }
    }
}