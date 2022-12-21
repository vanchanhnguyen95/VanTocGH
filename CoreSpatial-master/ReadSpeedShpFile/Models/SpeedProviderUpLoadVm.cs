namespace ReadSpeedShpFile.Models
{
    public class SpeedProviderUpLoadVm
    {
        public decimal Lat { get; set; } // X
        public decimal Lng { get; set; } // Y
        public int? ProviderType { get; set; } = 1;//1:Navital; 2:VietMap
        public string Position { get; set; }
        public int? MinSpeed { get; set; }
        public int? MaxSpeed { get; set; }
        public bool? PointError { get; set; } = false;//True: Tọa độ cung cấp bị lỗi, False: Tọa độ cung cấp ko bị lỗi
        public long SegmentID { get; set; }
        public int Direction { get; set; }
    }
}
