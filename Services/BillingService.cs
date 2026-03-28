namespace HotelReservation.Services;

using HotelReservation.Repositories;

// ISP VIOLATION: Depends on IReservationRepository (9 methods) but only uses
// GetById and GetTotalRevenue.
public class BillingService
{
    private readonly IReservationReadRepository _repo;

    public BillingService(IReservationReadRepository repo)
    {
        _repo = repo;
    }

    public decimal GetRevenueForPeriod(DateTime from, DateTime to)
    {
        return _repo.GetTotalRevenue(from, to);
    }
}
