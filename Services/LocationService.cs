using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeClockinSystem.Data;
using EmployeeClockinSystem.Interfaces;

namespace EmployeeClockinSystem.Services
{
    public class LocationService : ILocationService
    {

        public LocationService()
        {
            
        }
        public bool IsWithinAllowedLocation(double userLatitude, double userLongitude)
        {
            const double AllowedLatitude = -39.31192;  
            const double AllowedLongitude = 42.15511;  
            const double AllowedRadius = 100;  // 100 meters

            double distance = CalculateHaversineDistance(AllowedLatitude, AllowedLongitude, userLatitude, userLongitude);
            return distance <= AllowedRadius;
        }

        public double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km

            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c * 1000;  // Convert to meters
        }
    }
}