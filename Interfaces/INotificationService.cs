namespace HotelReservation.Interfaces;

public interface IEmailNotificationService
{
    void SendEmail(string to, string subject, string body);
}

public interface ISmsNotificationService
{
    void SendSms(string phoneNumber, string message);
}

public interface IPushNotificationService
{
    void SendPushNotification(string deviceId, string message);
}

public interface ISendSlackMessageNotificationService
{
    void SendSlackMessage(string channel, string message);
}
