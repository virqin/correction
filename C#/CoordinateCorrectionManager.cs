using System;
using Echat.Infrastructure.Log;

namespace Echat.LBSService.Processor.Correction
{
    public class CoordinateCorrectionManager
    {

        const double XPi = 3.14159265358979324 * 3000.0 / 180.0;
        const double Pi = 3.14159265358979323;
        const double A = 6378245.0;
        const double Ee = 0.00669342162296594323;

        /// <summary>
        /// 将GCJ-02 坐标转换成 BD-09 坐标
        /// </summary>
        /// <param name="coord">原GCJ-02坐标</param>
        /// <returns>目标BD-09坐标,如果转换失败返回原坐标</returns>
        public static Coordinate CoordConverGcj02ToBd09(Coordinate coord)
        {
            var coordinate = new Coordinate {Loctype = LocationType.BD09};
            double lon = coord.Lon;
            double lat = coord.Lat;
            coordinate.Lat = lat;
            coordinate.Lon = lon;
            try
            {
                double bdx;
                double bdy;
                bd_encrypt(lat, lon, out bdy, out bdx);
                coordinate.Lat = bdy;
                coordinate.Lon = bdx;
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorException("将GCJ-02 坐标转换成 BD-09 坐标发生错误", ex);
            }
            return coordinate;
        }

        /// <summary>
        /// 将WGS-84坐标转换为BD-09坐标时发生了异常
        /// </summary>
        /// <param name="coord">原WGS-84坐标</param>
        /// <returns>目标BD-09坐标,如果转换失败返回原坐标</returns>
        public static Coordinate CoordConverWGS84_To_BD09(Coordinate coord)
        {
            var coordinate = new Coordinate {Loctype = LocationType.BD09};
            double lon = coord.Lon;
            double lat = coord.Lat;
            double g_x, g_y;
            coordinate.Lat = lat;
            coordinate.Lon = lon;
            try
            {
                Transform(lat, lon, out g_y, out g_x);
                double bdx;
                double bdy;
                bd_encrypt(g_y, g_x, out bdy, out bdx);
                coordinate.Lat = bdy;
                coordinate.Lon = bdx;
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorException("将WGS-84坐标转换为BD-09坐标时发生了异常", ex);
            }
            return coordinate;
        }


        /// <summary>
        /// 将WGS-84坐标转换为GCJ-02坐标时发生了异常
        /// </summary>
        /// <param name="coord">原WGS-84坐标</param>
        /// <returns>目标GCJ-02坐标,如果转换失败返回原坐标</returns>
        public static Coordinate CoordConverWGS84_To_GCJ02(Coordinate coord)
        {
            var coordinate = new Coordinate {Loctype = LocationType.GCJ02};
            double lon = coord.Lon;
            double lat = coord.Lat;
            double g_x = lon, g_y = lat;
            coordinate.Lat = lat;
            coordinate.Lon = lon;
            try
            {
                Transform(lat, lon, out g_y, out g_x);
                coordinate.Lat = g_y;
                coordinate.Lon = g_x;
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorException("将WGS-84坐标转换为GCJ-02坐标时发生了异常", ex);
            }
            return coordinate;
        }

        #region BD09 TO CGJ-02 or TO GPS84
        /** 
         * * 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 * * 将 BD-09 坐标转换成GCJ-02 坐标 * * @param 
         * bd_lat * @param bd_lon * @return 
         */
        public static Coordinate bd09_To_Gcj02(double bd_lat, double bd_lon)
        {
            double x = bd_lon - 0.0065, y = bd_lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Pi);
            double gg_lon = z * Math.Cos(theta);
            double gg_lat = z * Math.Sin(theta);
            return new Coordinate(gg_lat, gg_lon, LocationType.BD09);
        }
        /** 
           * * 火星坐标系 (GCJ-02) to 84 * * @param lon * @param lat * @return 
           * */
        public static Coordinate gcj_To_Gps84(Coordinate coord)
        {
            double g_x, g_y;
            Transform(coord.Lat, coord.Lon, out g_x, out g_y);

            //2015.01.30修改
            //double lontitude = coord.Lat * 2 - g_x;
            //double latitude = coord.Lon * 2 - g_y;
            double latitude = coord.Lat * 2 - g_x;
            double lontitude = coord.Lon * 2 - g_y;

            return new Coordinate(latitude, lontitude, LocationType.GCJ02);
        }

        /** 
         * (BD-09)-->84 
         * @param bd_lat 
         * @param bd_lon 
         * @return 
         */
        public static Coordinate bd09_To_Gps84(double bd_lat, double bd_lon)
        {

            Coordinate gcj02 = bd09_To_Gcj02(bd_lat, bd_lon);
            Coordinate map84 = gcj_To_Gps84(gcj02);
            return map84;

        }
        #endregion

        #region 私有方法

        /// <summary> 
        /// GCJ-02 坐标转换成 BD-09 坐标
        /// </summary> 
        /// <param name="ggLat"></param> 
        /// <param name="ggLon"></param> 
        /// <param name="bdLat"></param> 
        /// <param name="bdLon"></param> 
        private static void bd_encrypt(double ggLat, double ggLon, out double bdLat, out double bdLon)
        {
            double x = ggLon, y = ggLat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * XPi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * XPi);
            bdLon = z * Math.Cos(theta) + 0.0065;
            bdLat = z * Math.Sin(theta) + 0.006;
        }

        /// <summary> 
        /// WGS-84 到 GCJ-02 的转换 World Geodetic System ==> Mars Geodetic System 
        /// </summary> 
        /// <param name="wgLat"></param> 
        /// <param name="wgLon"></param> 
        /// <param name="mgLat"></param> 
        /// <param name="mgLon"></param> 
        private static void Transform(double wgLat, double wgLon, out double mgLat, out double mgLon)
        {
            if (OutOfChina(wgLat, wgLon))
            {
                mgLat = wgLat;
                mgLon = wgLon;
                return;
            }
            double dLat = TransformLat(wgLon - 105.0, wgLat - 35.0);
            double dLon = TransformLon(wgLon - 105.0, wgLat - 35.0);
            double radLat = wgLat / 180.0 * Pi;
            double magic = Math.Sin(radLat);
            magic = 1 - Ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((A * (1 - Ee)) / (magic * sqrtMagic) * Pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * Pi);
            mgLat = wgLat + dLat;
            mgLon = wgLon + dLon;
        }

        /// <summary>
        /// 坐标是否处理中国范围内
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        private static bool OutOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        /// <summary>
        /// 加密纬度
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double TransformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Pi) + 20.0 * Math.Sin(2.0 * x * Pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * Pi) + 40.0 * Math.Sin(y / 3.0 * Pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * Pi) + 320 * Math.Sin(y * Pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 加密经度
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double TransformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Pi) + 20.0 * Math.Sin(2.0 * x * Pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * Pi) + 40.0 * Math.Sin(x / 3.0 * Pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * Pi) + 300.0 * Math.Sin(x / 30.0 * Pi)) * 2.0 / 3.0;
            return ret;
        }

        #endregion


    }
}
