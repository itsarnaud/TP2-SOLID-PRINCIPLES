namespace HotelReservation.Interfaces;

public interface ICancellationPolicy
{
  decimal CalculateRefund(decimal totalAmount, int daysBeforeCheckIn);
}