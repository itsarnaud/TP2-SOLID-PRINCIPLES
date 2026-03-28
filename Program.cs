using HotelReservation.Models;
using HotelReservation.Services;
using HotelReservation.Interfaces;
using HotelReservation.Events;
using HotelReservation.Repositories;

Console.WriteLine("=== Le Mas des Oliviers - Hotel Management System ===");
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 1: Creating Reservations (uses ReservationService — SRP violation fixed)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 1: Creating Reservations ---");

// Setup Repositories & Domain Service
var reservationRepo = new InMemoryReservationRepository();
var roomRepo = new InMemoryRoomRepository(reservationRepo);
roomRepo.SeedRooms(new List<Room>
{
    new Room { Id = "101", Type = "Standard", MaxGuests = 2, PricePerNight = 80m },
    new Room { Id = "102", Type = "Standard", MaxGuests = 2, PricePerNight = 80m },
    new Room { Id = "201", Type = "Suite", MaxGuests = 2, PricePerNight = 200m },
    new Room { Id = "301", Type = "Family", MaxGuests = 4, PricePerNight = 120m }
});
var domainService = new ReservationDomainService(reservationRepo);

var reservationService = new ReservationService(reservationRepo, roomRepo, domainService);

var id1 = reservationService.CreateReservation(
    "Alice Martin", "101", new DateTime(2025, 6, 15), new DateTime(2025, 6, 18),
    2, "Standard", "alice.martin@email.com");
Console.WriteLine($"[OK] Reservation {id1}: Alice Martin, Room 101 (Standard), 15/06 -> 18/06, 2 guests");

var id2 = reservationService.CreateReservation(
    "Bob Dupont", "201", new DateTime(2025, 6, 15), new DateTime(2025, 6, 22),
    2, "Suite", "bob.dupont@email.com");
Console.WriteLine($"[OK] Reservation {id2}: Bob Dupont, Room 201 (Suite), 15/06 -> 22/06, 2 guests");

var id3 = reservationService.CreateReservation(
    "Famille Durand", "301", new DateTime(2025, 6, 20), new DateTime(2025, 6, 25),
    4, "Family", "durand@email.com");
Console.WriteLine($"[OK] Reservation {id3}: Famille Durand, Room 301 (Family), 20/06 -> 25/06, 4 guests");

Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 2: Double Booking Attempt
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 2: Double Booking Attempt ---");
try
{
    reservationService.CreateReservation(
        "Charlie Test", "101", new DateTime(2025, 6, 16), new DateTime(2025, 6, 19),
        2, "Standard", "charlie@email.com");
}
catch (Exception ex)
{
    Console.WriteLine($"[CONFLICT] {ex.Message}");
}
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 3: Cancellation (uses CancellationService — OCP violation)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 3: Cancellation ---");
var cancellationService = new CancellationService();
var aliceReservation = reservationService.GetReservation(id1)!;
aliceReservation.CancellationPolicy = "Flexible";

ICancellationPolicy policy = aliceReservation.CancellationPolicy switch
{
    "Flexible" => new FlexiblePolicy(),
    "Moderate" => new ModeratePolicy(),
    "Strict" => new StrictPolicy(),
    "NonRefundable" => new NonRefundablePolicy(),
    _ => throw new ArgumentException()
};

cancellationService.CancelReservation(aliceReservation, new DateTime(2025, 6, 10), policy);
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 4: Check-In / Check-Out (uses CheckInService — SRP violation)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 4: Check-In / Check-Out ---");
var bobReservation = reservationService.GetReservation(id2)!;
var checkInService = new CheckInService(new Dictionary<string, Reservation>());
checkInService.ProcessCheckIn(bobReservation);
Console.WriteLine($"[OK] {bobReservation.GuestName} checked in to Room {bobReservation.RoomId}");
checkInService.ProcessCheckOut(bobReservation);
Console.WriteLine($"[OK] {bobReservation.GuestName} checked out from Room {bobReservation.RoomId}");
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 5: Billing (uses InvoiceGenerator — ISP violation)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 5: Billing ---");
var invoiceGenerator = new InvoiceGenerator();
// Reset Bob's reservation for billing demo
var bobForBilling = new Reservation
{
    Id = id2,
    GuestName = "Bob Dupont",
    RoomId = "201",
    CheckIn = new DateTime(2025, 6, 15),
    CheckOut = new DateTime(2025, 6, 22),
    GuestCount = 2,
    RoomType = "Suite",
    Status = "CheckedOut"
};
var invoice = invoiceGenerator.Generate(bobForBilling);
invoiceGenerator.PrintInvoice(invoice, bobForBilling);
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 6: Housekeeping Schedule (uses Reservation.GetLinenChangeDays — SRP violation)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 6: Housekeeping Schedule ---");
var housekeepingScheduler = new HousekeepingScheduler();

var bobForHousekeeping = new Reservation
{
    Id = id2,
    GuestName = "Bob Dupont",
    RoomId = "201",
    CheckIn = new DateTime(2025, 6, 15),
    CheckOut = new DateTime(2025, 6, 22),
    GuestCount = 2,
    RoomType = "Suite"
};
var bobLinenDays = housekeepingScheduler.GetLinenChangeDays(bobForHousekeeping);
Console.WriteLine($"Linen change schedule for Bob Dupont (Room 201, 15/06 -> 22/06):");
foreach (var day in bobLinenDays)
    Console.WriteLine($"  - {day:dd/MM/yyyy}");

var durandForHousekeeping = new Reservation
{
    Id = id3,
    GuestName = "Famille Durand",
    RoomId = "301",
    CheckIn = new DateTime(2025, 6, 20),
    CheckOut = new DateTime(2025, 6, 25),
    GuestCount = 4,
    RoomType = "Family"
};
var durandLinenDays = housekeepingScheduler.GetLinenChangeDays(durandForHousekeeping);
Console.WriteLine($"Cleaning tasks for Famille Durand (Room 301, 20/06 -> 25/06):");
foreach (var day in durandLinenDays)
    Console.WriteLine($"  - {day:dd/MM/yyyy}");
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 7: Event Dispatching (OCP good example — Observer pattern)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 7: Event Dispatching ---");
var dispatcher = new ReservationEventDispatcher();
dispatcher.Register(new EmailConfirmationHandler());
dispatcher.Register(new HousekeepingSetupHandler());
dispatcher.Dispatch(new ReservationCreatedEvent
{
    ReservationId = "R-001",
    GuestName = "Alice Martin",
    RoomId = "101",
    Email = "alice.martin@email.com",
    CheckIn = new DateTime(2025, 6, 15),
    CheckOut = new DateTime(2025, 6, 18)
});
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 8: Pricing with Seasonal Surcharge (OCP good example — Decorator)
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 8: Pricing with Seasonal Surcharge ---");
var aliceForPricing = new Reservation
{
    GuestName = "Alice Martin",
    RoomId = "101",
    CheckIn = new DateTime(2025, 6, 15),
    CheckOut = new DateTime(2025, 6, 18),
    RoomType = "Standard"
};

IPriceCalculator baseCalculator = new BasePriceCalculator();
var basePrice = baseCalculator.Calculate(aliceForPricing);
Console.WriteLine($"Base price for Alice (3 nights Standard): {basePrice:F2} EUR");

IPriceCalculator withSurcharge = new SeasonalSurchargeDecorator(baseCalculator, 0.20m);
var surchargedPrice = withSurcharge.Calculate(aliceForPricing);
Console.WriteLine($"With 20% summer surcharge: {surchargedPrice:F2} EUR");
Console.WriteLine();

// ---------------------------------------------------------------
// Scenario 9: LSP Violation Demo
// ---------------------------------------------------------------
Console.WriteLine("--- Scenario 9: LSP Violation Demo ---");
ICancellableReservation flexibleRes = new FlexibleReservation
{
    Id = "FLEX-001", GuestName = "Test Flexible", TotalPrice = 200m
};
flexibleRes.Cancel();
Console.WriteLine($"[OK] Flexible reservation cancelled, refund: {flexibleRes.CalculateRefund():F2} EUR");

IReservation nonRefundableRes = new NonRefundableReservation
{
    Id = "NR-001", GuestName = "Test NonRefundable", TotalPrice = 200m
};
Console.WriteLine($"[OK] Created {nonRefundableRes.GuestName} reservation, but it has no Cancel() method!");
Console.WriteLine();

Console.WriteLine("=== End of Demo ===");
