using System.ComponentModel.DataAnnotations;

namespace ReadSpeedShpFile.Models
{
    public class Speed3PointUpLoadVm
    {
        public double Lat { get; set; } // Y
        public double Lng { get; set; } // X
        public long SegmentID { get; set; }
        public int? ProviderType { get; set; } = 1;//1:Navital; 2:VietMap
        public string Position { get; set; }
    }
}
