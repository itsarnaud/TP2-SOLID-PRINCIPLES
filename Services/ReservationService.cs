namespace HotelReservation.Services;

using HotelReservation.Models;
using HotelReservation.Infrastructure;
using HotelReservation.Repositories;

// SRP VIOLATION (Example 1): This class mixes three levels of concern:
// - INFRASTRUCTURE: direct data access, logging
// - BUSINESS: availability check, price calculation, validation
// - APPLICATION: workflow orchestration
public class ReservationService
{
    private readonly IReservationRepository _reservationRepo;
    private readonly IRoomRepository _roomsRepo;
    private readonly ReservationDomainService _domainService;

    public ReservationService(IReservationRepository reservationRepo, IRoomRepository roomsRepo, ReservationDomainService domainService)
    {
        _reservationRepo = reservationRepo;
        _roomsRepo = roomsRepo;
        _domainService = domainService;
    }

    public string CreateReservation(string guestName, string roomId, DateTime checkIn,
        DateTime checkOut, int guestCount, string roomType, string email)
    {
        Console.WriteLine($"[LOG] Creating reservation for {guestName}...");

        var room = _roomsRepo.GetById(roomId);
        if (room == null)
            throw new Exception($"Room {roomId} not found");

        _domainService.ValidateReservation(room, guestCount, checkIn, checkOut);

        var total = _domainService.CalculateTotalPrice(room, checkIn, checkOut);

        var counter = _reservationRepo.GetAll().Count + 1; 
        
        var reservation = new Reservation
        {
            Id = $"R-{counter:D3}",
            GuestName = guestName,
            RoomId = roomId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            GuestCount = guestCount,
            RoomType = roomType,
            Status = "Confirmed",
            Email = email,
            TotalPrice = total
        };
        
        _reservationRepo.Add(reservation);

        Console.WriteLine($"[LOG] Reservation {reservation.Id} created.");

        return reservation.Id;
    }

    public Reservation? GetReservation(string id)
    {
        return _reservationRepo.GetById(id);
    }

    public List<Reservation> GetAllReservations()
    {
        return _reservationRepo.GetAll();
    }
}

public class ReservationDomainService
{
    private readonly IReservationRepository _reservationRepo;

    public ReservationDomainService(IReservationRepository reservationRepo)
    {
        _reservationRepo = reservationRepo;
    }

    public void ValidateReservation(Room room, int guestCount, DateTime checkIn, DateTime checkOut)
    {
        if (guestCount > room.MaxGuests)
            throw new Exception($"Room {room.Id} max capacity is {room.MaxGuests}");

        // On vérifie s'il y a déjà une réservation pour cette chambre sur ces dates
        var overlappingReservations = _reservationRepo.GetByDateRange(checkIn, checkOut)
                                        .Where(r => r.RoomId == room.Id && r.Status != "Cancelled");

        if (overlappingReservations.Any())
            throw new Exception($"Room {room.Id} is not available for {checkIn:dd/MM} -> {checkOut:dd/MM}");
    }

    public decimal CalculateTotalPrice(Room room, DateTime checkIn, DateTime checkOut)
    {
        var nights = (checkOut - checkIn).Days;
        return nights * room.PricePerNight;
    }
}

