namespace HotelReservation.Repositories;

using HotelReservation.Models;

// ISP VIOLATION: This interface is too large. HousekeepingService only needs
// GetByDateRange, BillingService only needs GetById and GetTotalRevenue,
// yet both depend on all 9 methods.
public interface IReservationRepository
{
    Reservation? GetById(string id);
    List<Reservation> GetAll();
    List<Reservation> GetByDateRange(DateTime from, DateTime to);
    List<Reservation> GetByGuest(string guestName);
    void Add(Reservation reservation);
    void Update(Reservation reservation);
    void Delete(string id);
    decimal GetTotalRevenue(DateTime from, DateTime to);
    Dictionary<string, int> GetOccupancyStats(DateTime from, DateTime to);
    bool IsRoomAvailable(string roomId, DateTime checkIn, DateTime checkOut);
}
