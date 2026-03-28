namespace HotelReservation.Services;

public interface ICleaningNotifier
{
  void NotifyCleaningDone(string to, string subject, string body);
}