using System.IO;

namespace Snapfish.BL.Models
{
    /*
     * This is a composite subscription where target strength (TS) detection and integration parameters must be set. The Sa value is taken only from the accepted single echo trace inside the range.
     */
    public struct TargetsIntegration
    {
        public StructIntegrationDataHeader IntegrationDataHeader;
        public StructIntegrationDataBody IntegrationDataBody;

        public static TargetsIntegration FromArray(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            TargetsIntegration ts = new TargetsIntegration
            {
                IntegrationDataHeader = new StructIntegrationDataHeader() {dlTime = reader.ReadUInt64()},
                IntegrationDataBody = new StructIntegrationDataBody() {dSa = reader.ReadDouble()}
            };
            return ts;
        }
    }

    public struct StructIntegrationDataHeader
    {
        public ulong dlTime;
    }
    
    public struct StructIntegrationDataBody
    {
        public double dSa; // integrated value from single echo trace [m2/nmi2]
    };

    
}