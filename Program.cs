using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;



public interface IFoodGift
{
    string GetName();
}
public interface IToyGift
{
    string GetName();
}



public class Candy : IFoodGift
{
    public string GetName()
    {
        return "Candy";
    }
}
public class Toy : IToyGift
{
    public string GetName()
    {
        return "Toy";
    }
}




public class BitterPill : IFoodGift
{
    public string GetName()
    {
        return "Bitter pill";
    }
}
public class Rod : IToyGift
{
    public string GetName()
    {
        return "Birch";
    }
}



public interface IGiftFactory
{
    IFoodGift CreateFoodGift();
    IToyGift CreateToyGift();
}



public class GoodGiftFactory : IGiftFactory
{
    public IFoodGift CreateFoodGift() => new Candy();
    public IToyGift CreateToyGift() => new Toy();
}
public class BadGiftFactory : IGiftFactory
{
    public IFoodGift CreateFoodGift() => new BitterPill();
    public IToyGift CreateToyGift() => new Rod();
}




public class Child
{
    public string Name { get; }
    public int GoodDeeds { get; }
    public int BadDeeds { get; }

    public Child(string name, int goodDeeds, int badDeeds)
    {
        Name = name;
        GoodDeeds = goodDeeds;
        BadDeeds = badDeeds;
    }
}




public class ChildrenList : IEnumerable<Child>
{
    private List<Child> children = new List<Child>();

    public void AddChild(Child child) => children.Add(child);

    public IEnumerator<Child> GetEnumerator() => children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}




public class SaintNicholas
{
    private static SaintNicholas instance;
    private ChildrenList childrenList = new ChildrenList();

    private SaintNicholas() { }

    public static SaintNicholas Instance
    {
        get
        {
            if (instance == null)
                instance = new SaintNicholas();
            return instance;
        }
    }

    public void AddChild(Child child) => childrenList.AddChild(child);

    public void DeliverGifts()
    {
        Console.WriteLine("Gift giving:");

        foreach (var child in childrenList)
        {
            IGiftFactory factoryGood = new GoodGiftFactory();
            IGiftFactory factoryBad = new BadGiftFactory();

            if (child.GoodDeeds > child.BadDeeds)
            {
                var foodGift = factoryGood.CreateFoodGift();
                var toyGift = factoryGood.CreateToyGift();
                Console.WriteLine($"{child.Name} get: {foodGift.GetName()} and {toyGift.GetName()}");
            }
            else if (child.GoodDeeds < child.BadDeeds)
            {
                var foodGift = factoryBad.CreateFoodGift();
                var toyGift = factoryBad.CreateToyGift();
                Console.WriteLine($"{child.Name} get: {foodGift.GetName()} and {toyGift.GetName()}");
            }
            else
            {
                var foodGift = factoryGood.CreateFoodGift();
                var toyGift = factoryBad.CreateToyGift();
                Console.WriteLine($"{child.Name} get: {foodGift.GetName()} and {toyGift.GetName()}");
            }



        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mikolai = SaintNicholas.Instance;

            Console.WriteLine();

            mikolai.AddChild(new Child("Vika", 5, 1));
            mikolai.AddChild(new Child("Katya", 1, 4));
            mikolai.AddChild(new Child("Nazar", 3, 3));
            mikolai.AddChild(new Child("Mariya", 1, 0));

            mikolai.DeliverGifts();

        }
    }
}
