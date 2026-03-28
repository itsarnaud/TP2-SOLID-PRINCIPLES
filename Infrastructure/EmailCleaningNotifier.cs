using HotelReservation.Services;

namespace HotelReservation.Infrastructure;

public class EmailCleaningNotifier : ICleaningNotifier
{
  private readonly EmailSender _emailSender = new();
  public void NotifyCleaningDone(string to, string subject, string body)
  {
    _emailSender.Send(to, subject, body);
  }
}