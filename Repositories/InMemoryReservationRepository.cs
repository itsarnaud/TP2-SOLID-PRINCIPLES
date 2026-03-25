namespace HotelReservation.Repositories;

using HotelReservation.Models;

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly Dictionary<string, Reservation> _reservations = new();

    public Reservation? GetById(string id)
    {
        return _reservations.TryGetValue(id, out var r) ? r : null;
    }

    public List<Reservation> GetAll()
    {
        return _reservations.Values.ToList();
    }

    public List<Reservation> GetByDateRange(DateTime from, DateTime to)
    {
        return _reservations.Values
            .Where(r => r.CheckIn < to && r.CheckOut > from && r.Status != "Cancelled")
            .ToList();
    }

    public List<Reservation> GetByGuest(string guestName)
    {
        return _reservations.Values
            .Where(r => r.GuestName == guestName)
            .ToList();
    }

    public void Add(Reservation reservation)
    {
        _reservations[reservation.Id] = reservation;
    }

    public void Update(Reservation reservation)
    {
        _reservations[reservation.Id] = reservation;
    }

    public void Delete(string id)
    {
        _reservations.Remove(id);
    }

    public decimal GetTotalRevenue(DateTime from, DateTime to)
    {
        var calculator = new Services.BillingCalculator();
        return _reservations.Values
            .Where(r => r.CheckIn >= from && r.CheckOut <= to && r.Status != "Cancelled")
            .Sum(r => calculator.CalculateTotal(r));
    }

    public Dictionary<string, int> GetOccupancyStats(DateTime from, DateTime to)
    {
        var reservations = GetByDateRange(from, to);
        return reservations
            .GroupBy(r => r.RoomType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public bool IsRoomAvailable(string roomId, DateTime checkIn, DateTime checkOut)
    {
        return !_reservations.Values
            .Any(r => r.RoomId == roomId && r.CheckIn < checkOut && r.CheckOut > checkIn && r.Status != "Cancelled");
    }
}
