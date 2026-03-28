namespace HotelReservation.Services;

using HotelReservation.Models;
using HotelReservation.Interfaces;

// OCP VIOLATION: Adding a new cancellation policy (e.g., "SuperFlexible")
// requires opening this class and adding a new case to the switch.
public class CancellationService
{
    public decimal CalculateRefund(Reservation reservation, DateTime now, ICancellationPolicy policy)
    {
        var daysBeforeCheckIn = (reservation.CheckIn - now).Days;
        return policy.CalculateRefund(reservation.TotalPrice, daysBeforeCheckIn);
    }

    public void CancelReservation(Reservation reservation, DateTime now, ICancellationPolicy policy)
    {
        var refund = CalculateRefund(reservation, now, policy);
        reservation.Cancel();
        Console.WriteLine(
            $"[OK] Reservation {reservation.Id} cancelled " +
            $"({reservation.CancellationPolicy} policy: " +
            $"{(refund == reservation.TotalPrice ? "full" : "partial")} refund of {refund:F2} EUR)");
    }
}
