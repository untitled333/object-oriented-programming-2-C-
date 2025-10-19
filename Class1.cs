using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public interface IReservable
{
    bool ReserveSeat(int seatNumber);
    void DisplayAvailableSeats();
}

abstract class Film
{
    public string Title { get; set; }
    public double BasePrice { get; set; }
    public int DurationMinutes { get; set; }
    public string Description { get; set; }
    public string AgeRestriction { get; protected set; }

    public Film(string title, double price, int duration, string desc)
    {
        Title = title;
        BasePrice = price;
        DurationMinutes = duration;
        Description = desc;
    }

    public override string ToString()
    {
        return $"{Title} ({DurationMinutes} min)\n{Description}\nAge: {AgeRestriction}, Base price: {BasePrice} grn";
    }
}

abstract class AdultFilm : Film
{
    public AdultFilm(string title, double price, int duration, string desc)
        : base(title, price, duration, desc)
    {
        AgeRestriction = "18+";
    }
}

abstract class KidsFilm : Film
{
    public KidsFilm(string title, double price, int duration, string desc)
        : base(title, price, duration, desc)
    {
        AgeRestriction = "0+";
    }
}

sealed class VariousAdultFilms : AdultFilm
{
    public VariousAdultFilms(string title, double price, int duration, string desc)
        : base(title, price, duration, desc) { }
}

sealed class VariousKidsFilms : KidsFilm
{
    public VariousKidsFilms(string title, double price, int duration, string desc)
        : base(title, price, duration, desc) { }
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

class CinemaHall : IReservable
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

    public int RegularViewerCount => regularViewers.Count;

    private List<(string viewerName, string filmTitle)> ticketHistory = new List<(string, string)>();
    private Schedule schedule;
    private Dictionary<string, List<string>> viewerFilms = new Dictionary<string, List<string>>();

    public static string LogFilePath { get; } = @"C:\Desktop\FullCinemaLog.txt";

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


    public RegularViewer this[int index]
    {
        get => regularViewers[index];
        set => regularViewers[index] = value;
    }

    public List<string> this[string viewerName]
    {
        get => viewerFilms.ContainsKey(viewerName) ? viewerFilms[viewerName] : null;
        set => viewerFilms[viewerName] = value;
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
    static void Main(string[] args)
    {
        var film1 = new VariousAdultFilms("Matrix", 150, 120, "Fantasy");
        var film2 = new VariousAdultFilms("The Exorcist", 150, 122, "Horror");
        var film3 = new VariousAdultFilms("Titanik", 150, 195, "Romance/Adventure");
        var film4 = new VariousAdultFilms("Five Feet Apart", 150, 117, "Romance/Drama");
        var film5 = new VariousKidsFilms("Lilo & Stitch", 150, 108, "Comedy/Adventure");

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

       
        Console.WriteLine("\nIndexers demonstration");

        Console.Write("\n\rEnter viewer index (0.." + (cashDesk.RegularViewerCount - 1) + "): ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < cashDesk.RegularViewerCount)
        {
            var viewerByIndex = cashDesk[index];
            Console.WriteLine($"Viewer with index: {index}: {viewerByIndex}");
        }
        else
        {
            Console.WriteLine("Cant find this index.");
        }

        Console.Write("\nEnter  the name of  viewer for check his sessions(viewed films): ");
        string nameInput = Console.ReadLine();
        var filmsByViewer = cashDesk[nameInput];

        if (filmsByViewer != null && filmsByViewer.Count > 0)
        {
            Console.WriteLine($"\r\nMovies watched by {nameInput}:");
            foreach (var f in filmsByViewer)
                Console.WriteLine($"- {f}");
        }
        else
        {
            Console.WriteLine("\r\nNo movies found for this viewer.");
        }
    }
}
