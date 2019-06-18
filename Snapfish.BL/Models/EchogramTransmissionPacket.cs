using System.Collections.Generic;

namespace Snapfish.BL.Models
{
    public class EchogramTransmissionPacket
    {
        public string ApplicationName { get; set; }
        public string ApplicationType { get; set; }
        public string ApplicationDescription  { get; set; }
        public string ApplicationVersion  { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public List<Echogram> Echograms { get; set; }
        public List<SampleDataContainerClass> SampleData { get; set; }
        public List<TargetsIntegration> Biomass { get; set; }
    }
}