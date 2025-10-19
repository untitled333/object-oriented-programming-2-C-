using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


interface ISellable
{
    double CalculatePrice();
}

interface IDescribable
{
    string GetDescription();
}

abstract class Product
{
    public string Title { get; set; }
    public double BasePrice { get; set; }

    public Product(string title, double price)
    {
        Title = title;
        BasePrice = price;
    }

    public abstract string GetInfo();
}

abstract class FilmProduct : Product
{
    public int DurationMinutes { get; set; }

    public FilmProduct(string title, double price, int duration)
        : base(title, price)
    {
        DurationMinutes = duration;
    }

    public abstract bool IsForChildren();
}

sealed class CinemaFilm : FilmProduct, ISellable, IDescribable
{
    public string Description { get; set; }
    public string AgeRestriction { get; set; }

    public CinemaFilm(string title, double price, int duration, string desc, string restriction)
        : base(title, price, duration)
    {
        Description = desc;
        AgeRestriction = restriction;
    }

    public override bool IsForChildren() => AgeRestriction == "0+";

    public override string GetInfo()
    {
        return $"{Title} - {DurationMinutes} mins - Age: {AgeRestriction}";
    }

    public double CalculatePrice() => BasePrice;

    public string GetDescription() => Description;
}


class Film
{
    public string Title { get; set; }
    public double BasePrice { get; set; }
    public int DurationMinutes { get; set; }
    public string Description { get; set; }
    public string AgeRestriction { get; set; }


    public Film() { }
    public Film(string title, double price, int duration, string desc, string restriction)
    {
        Title = title;
        BasePrice = price;
        DurationMinutes = duration;
        Description = desc;
        AgeRestriction = restriction;
    }

    public Film(Film other)
    {
        Title = other.Title;
        BasePrice = other.BasePrice;
        DurationMinutes = other.DurationMinutes;
        Description = other.Description;
        AgeRestriction = other.AgeRestriction;
    }

    public override string ToString()
    {
        return $"{Title} ({DurationMinutes} min.)\n{Description}\nAge: {AgeRestriction}, Based cost: {BasePrice} grn";
    }
}

class Seat
{
    public int Number { get; set; }
    public bool IsOccupied { get; set; }

    public double GetPriceMultiplier()
    {
        return Number <= 60 ? 1.2 : 1.0;
    }
}

class CinemaHall
{
    private List<Seat> seats = new List<Seat>();
    public List<Seat> Seats => seats;

    public CinemaHall()
    {
        for (int i = 1; i <= 120; i++)
            seats.Add(new Seat { Number = i, IsOccupied = false });
    }

    public bool ReserveSeat(int number)
    {
        var seat = seats.FirstOrDefault(s => s.Number == number && !s.IsOccupied);
        if (seat != null)
        {
            seat.IsOccupied = true;
            return true;
        }
        return false;
    }

    public void DisplayAvailableSeats()
    {
        Console.WriteLine("\nFree place:");
        foreach (var seat in seats.Where(s => !s.IsOccupied))
            Console.Write($"{seat.Number} ");
        Console.WriteLine();
    }
}

class Schedule
{
    private Dictionary<DateTime, (Film film, CinemaHall hall)> sessions = new Dictionary<DateTime, (Film film, CinemaHall hall)>();
    public Dictionary<DateTime, (Film, CinemaHall)> Sessions => sessions;

    public void AddSession(DateTime time, Film film, CinemaHall hall)
    {
        sessions[time] = (film, hall);
    }

    public void ShowSchedule()
    {
        foreach (var kvp in sessions)
            Console.WriteLine($"{kvp.Key}: {kvp.Value.film.Title}");
    }
}

class Viewer
{

    public string Name { get; set; }

    protected int viewerId;

    private static int nextId = 1;

    public static int TotalViewers { get; private set; }


    public virtual bool IsRegular => false;


    public Viewer()
    {
        viewerId = nextId++;
        TotalViewers++;
    }

    public Viewer(string name) : this()
    {
        Name = name;
    }

    public Viewer(Viewer other) : this(other.Name) { }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? "Secret viewer" : Name;
    }

    public static void ShowViewerStats()
    {
        Console.WriteLine($"Total number of viewers: {TotalViewers}");
    }
}

class RegularViewer : Viewer, IComparable<RegularViewer>
{
    public int TicketsBought { get; set; }

    public override bool IsRegular => true;


    public RegularViewer() : base() { }
    public RegularViewer(string name, int bought) : base(name) => TicketsBought = bought;

    public int CompareTo(RegularViewer other) => TicketsBought.CompareTo(other.TicketsBought);

    public static bool operator ==(RegularViewer a, RegularViewer b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return a.TicketsBought == b.TicketsBought;
    }

    public static bool operator !=(RegularViewer a, RegularViewer b)
    {
        return !(a == b);
    }

    public static bool operator >(RegularViewer a, RegularViewer b) => a.TicketsBought > b.TicketsBought;
    public static bool operator <(RegularViewer a, RegularViewer b) => a.TicketsBought < b.TicketsBought;


    public override string ToString() => $"{Name} (Regular viewer), Tickets: {TicketsBought}";
    public override bool Equals(object obj) => obj is RegularViewer rv && rv.Name == this.Name;
    public override int GetHashCode() => Name.GetHashCode();
}

class CashDesk
{
    private List<RegularViewer> regularViewers = new List<RegularViewer>();
    private List<(string viewerName, string filmTitle)> ticketHistory = new List<(string, string)>();
    private Schedule schedule;
    private Dictionary<string, List<string>> viewerFilms = new Dictionary<string, List<string>>();

    public static string LogFilePath { get; } = @"C:\Users\Дім\Desktop\FullCinemaLog.txt";

    static CashDesk()
    {
        if (!File.Exists(LogFilePath))
        {
            File.WriteAllText(LogFilePath, " Cinema Sales Log \n\n");
        }
    }

    public CashDesk(Schedule schedule)
    {
        this.schedule = schedule;
    }

    public void Start()
    {
        Console.WriteLine("Welcome to the cinema!");

        while (true)
        {
            Console.WriteLine("Would you like to buy a ticket? (yes/no)");
            var answer = Console.ReadLine()?.Trim().ToLower();
            if (answer != "yes")
                break;

            Console.WriteLine("\nHere are the available movies:\n");
            schedule.ShowSchedule();

            Console.WriteLine("\nEnter the date and time of the session:");
            if (!DateTime.TryParse(Console.ReadLine(), out var selectedTime) || !schedule.Sessions.ContainsKey(selectedTime))
            {
                Console.WriteLine("Wrong time.");
                continue;
            }

            var (film, hall) = schedule.Sessions[selectedTime];
            Console.WriteLine(film.ToString());

            Console.WriteLine("\nIs it suitable? (yes/no)");
            if (Console.ReadLine()?.Trim().ToLower() != "yes")
                continue;

            hall.DisplayAvailableSeats();
            Console.WriteLine("\nChoose place (!!!If u want place less than 60, it's +30 grn to base cost!!!):");
            if (!int.TryParse(Console.ReadLine(), out int seatNumber) || !hall.ReserveSeat(seatNumber))
            {
                Console.WriteLine("Wrong number.");
                continue;
            }

            Console.WriteLine("\nIs the ticket personal? (yes/no)");
            var isNamed = Console.ReadLine()?.Trim().ToLower() == "yes";

            Viewer viewer;
            string viewerName;
            if (isNamed)
            {
                Console.Write("\nEnter your name please:");
                var name = Console.ReadLine();
                var existing = regularViewers.FirstOrDefault(v => v.Name == name);
                if (existing == null)
                {
                    existing = new RegularViewer(name, 0);
                    regularViewers.Add(existing);
                }
                existing.TicketsBought++;
                viewer = existing;

                if (!viewerFilms.ContainsKey(viewer.Name))
                    viewerFilms[viewer.Name] = new List<string>();
                viewerFilms[viewer.Name].Add(film.Title);

                viewerName = existing.Name;
            }
            else
            {
                viewer = new Viewer();
                if (!viewerFilms.ContainsKey("Anonymous"))
                    viewerFilms["Anonymous"] = new List<string>();
                viewerFilms["Anonymous"].Add(film.Title);

                viewerName = "Anonymous";
            }

            ticketHistory.Add((viewerName, film.Title));
            var price = film.BasePrice * hall.Seats.First(s => s.Number == seatNumber).GetPriceMultiplier();

            Console.WriteLine($"Cost: {price} grn. \r\nPurchased for: {viewer}");
            Console.WriteLine("Ticket successfully purchased!\n");

            using (StreamWriter writer = File.AppendText(LogFilePath))
            {
                writer.WriteLine($"{DateTime.Now}: Film: {film.Title}, Seat: {seatNumber}, Viewer: {viewer}, Price: {price} grn");
            }
        }
    }

    public void ShowTopViewers()
    {
        Console.WriteLine("\nRegular viewer rating:");
        foreach (var v in regularViewers.OrderByDescending(v => v.TicketsBought).Take(5))
            Console.WriteLine(v);
    }

    public void SaveSessionInfo()
    {
        using (StreamWriter writer = File.AppendText(LogFilePath))
        {
            writer.WriteLine("\n Final Session Info ");
            foreach (var record in ticketHistory)
                writer.WriteLine($"Viewer: {record.viewerName}, Film: {record.filmTitle}");

            writer.WriteLine($"\nTotal viewers: {Viewer.TotalViewers}");

            writer.WriteLine("\nTop regular viewers:");
            foreach (var v in regularViewers.OrderByDescending(v => v.TicketsBought).Take(5))
                writer.WriteLine(v);

            writer.WriteLine("\n End of Session \n");
        }
    }
}


class Program
{
    static void Main()
    {
        var film1 = new Film("Matrix", 150, 120, "Fantasy", "16+");
        var film2 = new Film("Requiem for a dream", 150, 102, "\rHorror/Drama", "18+");
        var film3 = new Film("Titanik", 150, 195, "\r\nRomance/Adventure", "16+");
        var film4 = new Film("Five Feet Apart", 150, 117, "\r\nRomance/Drama", "16+");
        var film5 = new Film("Lilo & Stitch", 150, 108, "Comedy/Adventure", "0+");

        var hall1 = new CinemaHall();
        var hall2 = new CinemaHall();
        var hall3 = new CinemaHall();
        var hall4 = new CinemaHall();
        var hall5 = new CinemaHall();

        var schedule = new Schedule();
        schedule.AddSession(DateTime.Today.AddHours(18), film1, hall1);
        schedule.AddSession(DateTime.Today.AddHours(20), film2, hall2);
        schedule.AddSession(DateTime.Today.AddHours(23), film3, hall3);
        schedule.AddSession(DateTime.Today.AddHours(15), film4, hall4);
        schedule.AddSession(DateTime.Today.AddHours(10), film5, hall5);

        var cashDesk = new CashDesk(schedule);


        cashDesk.Start();

        Console.WriteLine();
        cashDesk.ShowTopViewers();
        Viewer.ShowViewerStats();

        cashDesk.SaveSessionInfo();


    }
}
