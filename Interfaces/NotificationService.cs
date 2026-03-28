namespace HotelReservation.Interfaces;

public class NotificationService : IEmailNotificationService, ISmsNotificationService, IPushNotificationService, ISendSlackMessageNotificationService
{
    public void SendEmail(string to, string subject, string body)
    {
        Console.WriteLine($"[EMAIL] {subject} sent to {to}");
    }

    public void SendSms(string phoneNumber, string message)
    {
        Console.WriteLine($"[SMS] {message} sent to {phoneNumber}");
    }

    public void SendPushNotification(string deviceId, string message)
    {
        Console.WriteLine($"[PUSH] {message} sent to device {deviceId}");
    }

    public void SendSlackMessage(string channel, string message)
    {
        Console.WriteLine($"[SLACK] {message} sent to #{channel}");
    }
}
