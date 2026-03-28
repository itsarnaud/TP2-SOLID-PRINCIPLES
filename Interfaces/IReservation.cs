namespace HotelReservation.Interfaces;

public interface IReservation
{
  string Id { get; }
  string GuestName { get; }
  string Status { get; }
  decimal TotalPrice { get; }
}