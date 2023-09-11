

namespace EmployeeClockinSystem.Interfaces
{
    public interface ILocationService
    {
        bool IsWithinAllowedLocation(double userLatitude, double userLongitude);
        double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2);
    }
}