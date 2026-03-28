namespace HotelReservation.Services;

using HotelReservation.Infrastructure;
using HotelReservation.Models;

// DIP VIOLATION (Example 2): High-level housekeeping logic directly depends on
// low-level EmailSender. If we want to notify by SMS instead, we must modify this class.
public class HousekeepingService
{
    // Direct dependency on concrete EmailSender
    private readonly ICleaningNotifier _cleaningNotifier;

    public HousekeepingService(ICleaningNotifier cleaningNotifier)
    {
        _cleaningNotifier = cleaningNotifier;
    }

    public List<CleaningTask> GenerateLinenChangeSchedule(Reservation reservation)
    {
        var tasks = new List<CleaningTask>();
        var current = reservation.CheckIn.AddDays(3);
        while (current < reservation.CheckOut)
        {
            tasks.Add(new CleaningTask
            {
                RoomId = reservation.RoomId,
                Date = current,
                Type = "LinenChange",
                HousekeeperEmail = "housekeeping@masdesoliviers.fr",
                Time = new TimeSpan(10, 0, 0)
            });
            current = current.AddDays(3);
        }
        return tasks;
    }

    public void NotifyHousekeeper(CleaningTask task)
    {
        // Coupled to email — can't switch to SMS without changing this code
        _cleaningNotifier.NotifyCleaningDone(
            task.HousekeeperEmail,
            "New cleaning task",
            $"Room {task.RoomId} needs {task.Type} on {task.Date:dd/MM/yyyy}");
    }
}
