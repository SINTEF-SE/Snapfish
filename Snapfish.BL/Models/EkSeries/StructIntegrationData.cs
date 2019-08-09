using System.IO;

namespace Snapfish.BL.Models.EkSeries
{
    public struct IntegrationStructIntegrationDataHeader
    {
        public ulong dlTime;
    };

    public struct IntegrationStructIntegrationDataBody
    {
        public double dSa; // integrated value [m2/nmi2]
    };

    public struct StructIntegrationData
    {
        public IntegrationStructIntegrationDataHeader IntegrationDataHeader;
        public IntegrationStructIntegrationDataBody IntegrationDataBody;

        public static StructIntegrationData FromArray(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            StructIntegrationData ts = new StructIntegrationData
            {
                IntegrationDataHeader = new IntegrationStructIntegrationDataHeader() {dlTime = reader.ReadUInt64()},
                IntegrationDataBody = new IntegrationStructIntegrationDataBody() {dSa = reader.ReadDouble()}
            };
            return ts;
        }
    }
}