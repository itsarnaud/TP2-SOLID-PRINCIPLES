namespace HotelReservation.Models;

// SRP VIOLATION (Example 3): This class serves THREE actors:
// - Receptionist: lifecycle (Cancel, Status management)
// - Accountant: billing (CalculateTotal, GenerateInvoiceLine)
// - Housekeeper: cleaning schedule (GetLinenChangeDays)

public class Reservation
{
    public string Id { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed"; // Confirmed, CheckedIn, CheckedOut, Cancelled
    public string CancellationPolicy { get; set; } = "Flexible";
    public string Email { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }

    // Actor: RECEPTIONIST — cancellation rules
    public void Cancel()
    {
        if (Status == "CheckedIn")
            throw new InvalidOperationException("Cannot cancel after check-in");
        Status = "Cancelled";
    }
}
