namespace HotelReservation.Interfaces;

public interface ICancellableReservation : IReservation
{
  void Cancel();
  decimal CalculateRefund();
}