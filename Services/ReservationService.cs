namespace HotelReservation.Services;

using HotelReservation.Models;
using HotelReservation.Repositories;

public class ReservationService
{
    private readonly IReservationReadRepository _readRepo;
    private readonly IReservationWriteRepository _writeRepo;
    private readonly IRoomRepository _roomsRepo;
    private readonly ReservationDomainService _domainService;

    public ReservationService(IReservationReadRepository readRepo, IReservationWriteRepository writeRepo, IRoomRepository roomsRepo, ReservationDomainService domainService)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
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

        var counter = _readRepo.GetAll().Count + 1; 
        
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
        
        _writeRepo.Add(reservation);

        Console.WriteLine($"[LOG] Reservation {reservation.Id} created.");

        return reservation.Id;
    }

    public Reservation? GetReservation(string id)
    {
        return _readRepo.GetById(id);
    }

    public List<Reservation> GetAllReservations()
    {
        return _readRepo.GetAll();
    }
}

public class ReservationDomainService
{
    private readonly IReservationReadRepository _reservationRepo;

    public ReservationDomainService(IReservationReadRepository reservationRepo)
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

