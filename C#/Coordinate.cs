using System;

namespace Echat.LBSService.Processor.Correction
{
    public class Coordinate
    {

        public Coordinate() 
        { }

        public Coordinate(double lat, double lon, LocationType loctype) 
        {
            Lat = lat;
            Lon = lon;
            Loctype = loctype;
        }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public LocationType Loctype { get; set; }

    }

    public enum LocationType
    {
        /// <summary>
        /// WGS84国际通用坐标系
        /// </summary>
        WGS84 = 1,
        /// <summary>
        /// 国测局02标准(国测局对WGS84加密后的国内坐标系)
        /// </summary>
        GCJ02 = 2,
        /// <summary>
        /// 西安80坐标系
        /// </summary>
        XA80 = 3,
        /// <summary>
        /// 北京54坐标系
        /// </summary>
        BG54 = 4,
        /// <summary>
        /// 百度09标准，百度在GCJ02标准坐标系基础上二次加密的坐标系
        /// </summary>
        BD09 = 5,
    }
}
