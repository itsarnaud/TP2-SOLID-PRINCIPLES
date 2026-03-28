namespace HotelReservation.Services;

using HotelReservation.Infrastructure;
using HotelReservation.Models;

public interface ILogger
{
    void Log(string message);
}

public interface IReservationRepository
{
    void Add(Reservation reservation);
}

public class BookingService
{
    // Direct dependency on concrete implementations
    private readonly IReservationRepository _store;
    private readonly ILogger _logger;

    private int _counter = 0;

    public BookingService(IReservationRepository store, ILogger logger)
    {
        _store = store;
        _logger = logger;
    }

    public string CreateReservation(string guestName, string roomId, DateTime checkIn,
        DateTime checkOut, int guestCount, string roomType, string email)
    {
        _logger.Log($"Creating reservation for {guestName}...");

        var nights = (checkOut - checkIn).Days;
        var pricePerNight = roomType switch
        {
            "Standard" => 80m,
            "Suite" => 200m,
            "Family" => 120m,
            _ => throw new Exception($"Unknown room type: {roomType}")
        };

        _counter++;
        var reservation = new Reservation
        {
            Id = $"R-{_counter:D3}",
            GuestName = guestName,
            RoomId = roomId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            GuestCount = guestCount,
            RoomType = roomType,
            Status = "Confirmed",
            Email = email,
            TotalPrice = nights * pricePerNight
        };

        _store.Add(reservation);
        _logger.Log($"Reservation {reservation.Id} created.");

        return reservation.Id;
    }
}
