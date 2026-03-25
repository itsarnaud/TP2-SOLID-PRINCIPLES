namespace HotelReservation.Services;

using HotelReservation.Models;

// SRP VIOLATION (Example 2): A single method mixes multiple levels of abstraction.
// High-level business rules sit next to low-level cache manipulation and config reading.
public class CheckInService
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly Dictionary<string, Reservation> _dataStore;

    public CheckInService(Dictionary<string, Reservation> dataStore)
    {
        _dataStore = dataStore;
    }

    public void ProcessCheckIn(Reservation reservation)
    {
        ValidateCheckIn(reservation);
        ApplyLateCheckInFeeIfNeeded(reservation);
        UpdateReservationCache(reservation, "CheckedIn");
        NotifyRoomStatus(reservation.RoomId, "occupied");
    }

    public void ProcessCheckOut(Reservation reservation)
    {
        ValidateCheckOut(reservation);
        UpdateReservationCache(reservation, "CheckedOut");
        NotifyRoomStatus(reservation.RoomId, "free");
    }

    private void ValidateCheckIn(Reservation reservation)
    {
        if (reservation.Status != "Confirmed")
            throw new Exception($"Cannot check in: reservation is {reservation.Status}");
    }

    private void ValidateCheckOut(Reservation reservation)
    {
        if (reservation.Status != "CheckedIn")
            throw new Exception($"Cannot check out: reservation is {reservation.Status}");
    }

    private void ApplyLateCheckInFeeIfNeeded(Reservation reservation)
    {
        var lateCheckInFee = 25m;
        if (DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 6)
            reservation.TotalPrice += lateCheckInFee;
    }

    private void UpdateReservationCache(Reservation reservation, string newStatus)
    {
        reservation.Status = newStatus;

        if (_cache.ContainsKey(reservation.Id))
        {
            _cache.Remove(reservation.Id);
        }

        if (newStatus == "CheckedIn")
        {
            _cache[reservation.Id] = new CacheEntry(DateTime.Now, newStatus);
        }
    }

    private void NotifyRoomStatus(string roomId, string status)
    {
        Console.WriteLine($"[SMS] Room {roomId} is now {status}");
    }
}
