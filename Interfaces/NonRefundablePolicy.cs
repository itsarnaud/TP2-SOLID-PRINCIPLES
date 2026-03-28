namespace HotelReservation.Interfaces;

public class NonRefundablePolicy : ICancellationPolicy
{
  public decimal CalculateRefund(decimal totalAmount, int daysBeforeCheckIn)
  {
    return 0m;
  }
}