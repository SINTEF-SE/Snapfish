using System.Collections.Generic;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.BL.Models
{
    public class EchogramTransmissionPacket
    {
        public int Id { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationType { get; set; }
        public string ApplicationDescription  { get; set; }
        public string ApplicationVersion  { get; set; }
        
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<string> Longitudes { get; set; } //TODO
        public List<string> Latitudes { get; set; } // TODO
        public List<Echogram> Echograms { get; set; }
        public List<SampleDataContainerClass> SampleData { get; set; }
        public List<StructIntegrationData> Biomasses { get; set; }
        public List<TargetsIntegration> TargetsBiomass { get; set; }
    }
}