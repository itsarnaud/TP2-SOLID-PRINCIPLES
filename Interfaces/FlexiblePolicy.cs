namespace HotelReservation.Interfaces;

public class FlexiblePolicy : ICancellationPolicy
{
  public decimal CalculateRefund(decimal totalAmount, int daysBeforeCheckIn)
  {
    return daysBeforeCheckIn >= 1 ? totalAmount : 0m;
  }
}