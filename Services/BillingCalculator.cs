namespace HotelReservation.Services;

using HotelReservation.Models;

public class BillingCalculator
{
    public decimal CalculateTotal(Reservation reservation)
    {
        var nights = (reservation.CheckOut - reservation.CheckIn).Days;
        var pricePerNight = reservation.RoomType switch
        {
            "Standard" => 80m,
            "Suite" => 200m,
            "Family" => 120m,
            _ => 0m
        };
        var subtotal = nights * pricePerNight;
        var tva = subtotal * 0.10m;
        var touristTax = reservation.GuestCount * nights * 1.50m;
        return subtotal + tva + touristTax;
    }

    public string GenerateInvoiceLine(Reservation reservation)
    {
        return $"{reservation.GuestName} | {reservation.CheckIn:dd/MM} -> {reservation.CheckOut:dd/MM} | {CalculateTotal(reservation):F2} EUR";
    }
}