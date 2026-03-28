namespace HotelReservation.Infrastructure;

// Simulates file-based logging (uses Console for demo purposes).
// The SOLID violation is the direct coupling, not the I/O mechanism.
public class FileLogger : Services.ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[LOG] {message}");
    }
}
