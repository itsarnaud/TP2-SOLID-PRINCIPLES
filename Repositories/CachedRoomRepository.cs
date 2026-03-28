namespace HotelReservation.Repositories;

using HotelReservation.Models;

// LSP VIOLATION (Example 2): This implementation does not respect the contract
// of IRoomRepository.GetAvailableRooms. It returns cached data that may be stale
// and ignores the date parameters entirely. Substituting this for InMemoryRoomRepository
// produces semantically incorrect results.
public class CachedRoomRepository : IRoomRepository
{
    private readonly IRoomRepository _inner;
    private readonly Dictionary<string, Room> _cache = new();

    public CachedRoomRepository(IRoomRepository inner)
    {
        _inner = inner;
    }

    public Room? GetById(string roomId)
    {
        if (!_cache.ContainsKey(roomId))
        {
            var room = _inner.GetById(roomId);
            if (room != null)
                _cache[roomId] = room;
            return room;
        }
        return _cache[roomId];
    }

    public List<Room> GetAvailableRooms(DateTime from, DateTime to)
    {
        return _inner.GetAvailableRooms(from, to);
    }

    public void Save(Room room)
    {
        _inner.Save(room);
        _cache.Remove(room.Id);
    }
}
