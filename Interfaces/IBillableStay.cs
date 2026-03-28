namespace HotelReservation.Interfaces;

public interface IBillableStay
{
    string Id { get; }
    string GuestName { get; }
    string RoomId { get; }
    string RoomType { get; }
    DateTime CheckIn { get; }
    DateTime CheckOut { get; }
    int GuestCount { get; }
}