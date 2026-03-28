namespace HotelReservation.Interfaces;

public class StrictPolicy : ICancellationPolicy
{
  public decimal CalculateRefund(decimal totalAmount, int daysBeforeCheckIn)
  {
    if (daysBeforeCheckIn >= 14) return totalAmount;
    if (daysBeforeCheckIn >= 7) return totalAmount * 0.5m;
    return 0m;
  }
}