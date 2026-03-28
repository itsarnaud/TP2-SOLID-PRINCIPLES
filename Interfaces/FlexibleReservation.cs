namespace HotelReservation.Interfaces;

public class FlexibleReservation : ICancellableReservation
{
    public string Id { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";
    public decimal TotalPrice { get; set; }

    public void Cancel()
    {
        Status = "Cancelled";
    }

    public decimal CalculateRefund()
    {
        return TotalPrice; // Full refund
    }
}
