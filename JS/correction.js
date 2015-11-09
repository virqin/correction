/*
 pi: 圆周率。
 a: 卫星椭球坐标投影到平面地图坐标系的投影因子。
 ee: 椭球的偏心率。
 x_pi: 圆周率转换量。
 transformLat(lat, lon): 转换方法，比较复杂，不必深究了。输入：横纵坐标，输出：转换后的横坐标。
 transformLon(lat, lon): 转换方法，同样复杂，自行脑补吧。输入：横纵坐标，输出：转换后的纵坐标。
 wgs2gcj(lat, lon): WGS坐标转换为GCJ坐标。
 gcj2bd(lat, lon): GCJ坐标转换为百度坐标。
*/

var pi = 3.14159265358979324;
var a = 6378245.0;
var ee = 0.00669342162296594323;
var x_pi = 3.14159265358979324 * 3000.0 / 180.0;

function wgs2bd(lat, lon) {
       _wgs2gcj = wgs2gcj(lat, lon);
       _gcj2bd = gcj2bd(_wgs2gcj[0], _wgs2gcj[1]);
       return _gcj2bd;
}

function gcj2bd(lat, lon) {
       x = lon, y = lat;
       z = Math.sqrt(x * x + y * y) + 0.00002 * Math.sin(y * x_pi);
       theta = Math.atan2(y, x) + 0.000003 * Math.cos(x * x_pi);
       bd_lon = z * Math.cos(theta) + 0.0065;
       bd_lat = z * Math.sin(theta) + 0.006;
       return [ bd_lat, bd_lon ];
}

function bd2gcj(lat, lon) {
       x = lon - 0.0065, y = lat - 0.006;
       z = Math.sqrt(x * x + y * y) - 0.00002 * Math.sin(y * x_pi);
       theta = Math.atan2(y, x) - 0.000003 * Math.cos(x * x_pi);
       gg_lon = z * Math.cos(theta);
       gg_lat = z * Math.sin(theta);
       return [ gg_lat, gg_lon ];
}

function wgs2gcj(lat, lon) {
       dLat = transformLat(lon - 105.0, lat - 35.0);
       dLon = transformLon(lon - 105.0, lat - 35.0);
       radLat = lat / 180.0 * pi;
       magic = Math.sin(radLat);
       magic = 1 - ee * magic * magic;
       sqrtMagic = Math.sqrt(magic);
       dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
       dLon = (dLon * 180.0) / (a / sqrtMagic * Math.cos(radLat) * pi);
       mgLat = lat + dLat;
       mgLon = lon + dLon;
       return [ mgLat, mgLon ];
}

function transformLat(lat, lon) {
       ret = -100.0 + 2.0 * lat + 3.0 * lon + 0.2 * lon * lon + 0.1 * lat * lon + 0.2 * Math.sqrt(Math.abs(lat));
       ret += (20.0 * Math.sin(6.0 * lat * pi) + 20.0 * Math.sin(2.0 * lat * pi)) * 2.0 / 3.0;
       ret += (20.0 * Math.sin(lon * pi) + 40.0 * Math.sin(lon / 3.0 * pi)) * 2.0 / 3.0;
       ret += (160.0 * Math.sin(lon / 12.0 * pi) + 320 * Math.sin(lon * pi  / 30.0)) * 2.0 / 3.0;
       return ret;
}

function transformLon(lat, lon) {
       ret = 300.0 + lat + 2.0 * lon + 0.1 * lat * lat + 0.1 * lat * lon + 0.1 * Math.sqrt(Math.abs(lat));
       ret += (20.0 * Math.sin(6.0 * lat * pi) + 20.0 * Math.sin(2.0 * lat * pi)) * 2.0 / 3.0;
       ret += (20.0 * Math.sin(lat * pi) + 40.0 * Math.sin(lat / 3.0 * pi)) * 2.0 / 3.0;
       ret += (150.0 * Math.sin(lat / 12.0 * pi) + 300.0 * Math.sin(lat / 30.0 * pi)) * 2.0 / 3.0;
       return ret;
}