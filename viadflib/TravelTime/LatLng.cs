using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace viadflib.TravelTime
{
    public class LatLng
    {
        /// <summary>
        /// Aprox. the same for lat / lng
        /// </summary>
        private const double _kmDegreesLatMin = 110.574;
        private const double _kmDegreesLatMax = 111.694;

        private const double _kmDegreesLng = 111.320;

        private static readonly double[] _latKmLookup = Enumerable.Range(0, 90).Select(x => _kmDegreesLatMin + (_kmDegreesLatMax - _kmDegreesLatMin) * ((x + 0.5) / 90.0)).ToArray();
        private static readonly double[] _lngKmLookup = Enumerable.Range(0, 90).Select(x => Math.Cos(Math.PI * (x + 0.5) / 180) * _kmDegreesLng).ToArray();

        public static double GetWalkingDistanceLatLng(double lat, double timeMin, double walkSpeedKmH)
        {
            double kms = walkSpeedKmH * (timeMin / 60.0);
            return (kms / _latKmLookup[(int)lat] + kms / _lngKmLookup[(int)lat]) / 2.0;
        }

        public LatLng MoveKm(double latKm, double lngKm)
        {
            return new LatLng((Lat * _latKmLookup[(int)Lat] + latKm) / _latKmLookup[(int)Lat], ((Lng * _lngKmLookup[(int)Lat]) + lngKm) / _lngKmLookup[(int)Lat]);
        }

        public LatLng(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public LatLng(string ll)
        {
            var c = ll.Split(',');
            var lat = double.Parse(c[0], CultureInfo.InvariantCulture);
            var lng = double.Parse(c[1], CultureInfo.InvariantCulture);
            Lat = lat;
            Lng = lng;
        }

        public double DistanceInKmTo(LatLng other)
        {
            double distanceLat = Math.Abs(Lat - other.Lat) * _latKmLookup[(int)Lat];
            double distanceLng = Math.Abs(Lng - other.Lng) * _lngKmLookup[(int)Lat];

            return Math.Sqrt(distanceLat * distanceLat + distanceLng * distanceLng);
        }

        public double DistanceInKmSquaredTo(LatLng other)
        {
            double distanceLat = Math.Abs(Lat - other.Lat) * _latKmLookup[(int)Lat];
            double distanceLng = Math.Abs(Lng - other.Lng) * _lngKmLookup[(int)Lat];

            return distanceLat * distanceLat + distanceLng * distanceLng;
        }

        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
