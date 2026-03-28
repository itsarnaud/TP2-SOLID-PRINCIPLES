namespace HotelReservation.Repositories;

using HotelReservation.Models;

public interface IReservationReadRepository
{
    Reservation? GetById(string id);
    List<Reservation> GetAll();
    List<Reservation> GetByDateRange(DateTime from, DateTime to);
    List<Reservation> GetByGuest(string guestName);
    bool IsRoomAvailable(string roomId, DateTime checkIn, DateTime checkOut);
    decimal GetTotalRevenue(DateTime from, DateTime to);
}

public interface IReservationWriteRepository
{
    void Add(Reservation reservation);
    void Update(Reservation reservation);
    void Delete(string id);
}

public interface IReservationStatsRepository
{
    Dictionary<string, int> GetOccupancyStats(DateTime from, DateTime to);
}