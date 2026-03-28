namespace HotelReservation.Interfaces;

public class ModeratePolicy : ICancellationPolicy
{
  public decimal CalculateRefund(decimal totalAmount, int daysBeforeCheckIn)
  {
    if (daysBeforeCheckIn >= 5) return totalAmount;
    if (daysBeforeCheckIn >= 2) return totalAmount * 0.5m;
    return 0m;
  }
}

