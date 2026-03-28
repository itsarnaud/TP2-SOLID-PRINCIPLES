namespace HotelReservation.Infrastructure;

using HotelReservation.Models;

public class InMemoryReservationStore : Services.IReservationRepository
{
    private readonly Dictionary<string, Reservation> _reservations = new();

    public void Add(Reservation reservation)
    {
        _reservations[reservation.Id] = reservation;
    }

    public Reservation? GetById(string id)
    {
        return _reservations.TryGetValue(id, out var reservation) ? reservation : null;
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

    public void Update(Reservation reservation)
    {
        _reservations[reservation.Id] = reservation;
    }

    public void Delete(string id)
    {
        _reservations.Remove(id);
    }
}
