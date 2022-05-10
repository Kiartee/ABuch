using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class Program
{
    static void Main()
    {
        var board = new string[]
        {
            ".S...",
            "...L.",
            ".Al.."
        };

        IParser parser = new Parser();
        IGame game = new Game(parser);
        game.Start(board);
        game.Play(10);
    }
}
public interface IField
{
    ICoords Coords { get; }
    string Display { get; }
}
public interface IAnimal : IField
{
    string Species { get; }
    bool IsMale { get; }
    bool IsPredator { get; }
    void Move();
    void Reproduction();
}
public interface IPredator : IAnimal
{
    void Eat();
}
public interface ICoords
{
    int X { get; }
    int Y { get; }
}
public interface IBoard
{
    List<IAnimal> GetNeighbours(IAnimal animal);
    string Display { get; }
    void AddField(IField field);
}
public interface IParser
{
    IBoard Parse(string[] input);
}
public interface IGame
{
    void Start(string[] input);
    void Play(int numberOfTurns);
}

public class Game : IGame
{
    private IBoard _board;
    private readonly IParser _parser;
    public Game(IParser parser)
    {
        _parser = parser;
    }
    public void Start(string[] board)
    {
        _board = _parser.Parse(board);
    }
    public void Play(int numberOfTurns)
    {
        List<IAnimal> listOfLivingAnimals = new();
        int moveCounter = 0;
        while (moveCounter == numberOfTurns)
        {
            moveCounter++;
            Console.WriteLine($"Step {moveCounter}: ");
            listOfLivingAnimals.ForEach(animal => animal.Move());
            listOfLivingAnimals.Where(animal => animal.IsPredator).ToList().ForEach(animal => animal.Move());
            listOfLivingAnimals.ForEach(animal => animal.Reproduction());
        }
    }
    public void Move(IAnimal animal)
    {
        animal.Move();
    }
}
public class Board : IBoard
{
    private readonly List<IField> _animals;
    public Board()
    {
        _animals = new List<IField>();
    }
    public List<IAnimal> GetNeighbours(IAnimal animal)
    {
        List<IAnimal> result = new();
        ICoords coords = animal.Coords;
        IAnimal? top = (IAnimal?)_animals.FirstOrDefault(f => f.Coords.X == coords.X && f.Coords.Y == coords.Y + 1);
        if (top != null) { result.Add(top); }
        IAnimal? bottom = (IAnimal?)_animals.FirstOrDefault(f => f.Coords.X == coords.X && f.Coords.Y == coords.Y - 1);
        if (bottom != null) { result.Add(bottom); }
        IAnimal? left = (IAnimal?)_animals.FirstOrDefault(f => f.Coords.X == coords.X + 1 && f.Coords.Y == coords.Y);
        if (left != null) { result.Add(left); }
        IAnimal? right = (IAnimal?)_animals.FirstOrDefault(f => f.Coords.X == coords.X - 1 && f.Coords.Y == coords.Y);
        if (right != null) { result.Add(right); }

        return result;
    }
    public void AddField(IField field)
    {
        _animals.Add(field);
    }
    public string Display
    {
        get
        {
            List<IField> ordered = _animals.OrderBy(f => f.Coords.X).ThenBy(f => f.Coords.Y).ToList();
            IField last = ordered.Last();
            int maxY = last.Coords.Y;
            StringBuilder sb = new();
            for (int i = 0; i < ordered.Count; i++)
            {
                IAnimal animal = (IAnimal)ordered[i];
                sb.Append(animal.Display);
                if (animal.Coords.Y == maxY)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}
public class Parser : IParser
{
    public Parser()
    {

    }
    private IBoard _board;
    public IBoard Parse(string[] input)
    {
        _board = new Board();
        for (int rowIndex = 0; rowIndex < input.Length; rowIndex++)
        {
            string row = input[rowIndex];
            for (int colIndex = 0; colIndex < row.Length; colIndex++)
            {
                var fieldChar = row[colIndex];
                Add(rowIndex, colIndex, fieldChar);
            }
        }
        return _board;
    }
    private void Add(int x, int y, char c)
    {
        var coords = new Coords(x, y);
        IField field = AnimalFactory.Create(c, coords);
        _board.AddField(field);
    }
}
public class Coords : ICoords
{
    public Coords(int x, int y)
    {
        X = x;
        Y = y;
    }
    public int X { get; private set; }
    public int Y { get; private set; }
}
public abstract class Field : IField
{
    protected Field(ICoords coords)
    {
        Coords = coords;
    }
    public ICoords Coords { get; private set; }
    public abstract string Display { get; protected set; }
}
public class EmptyField : Field
{
    protected virtual string InitialDisplay => ".";
    public EmptyField(ICoords coords) : base(coords)
    {
    }
    public override string Display
    {
        get
        {
            return InitialDisplay;
        }
        protected set
        {
        }
    }
}
public static class AnimalFactory
{
    public static IField Create(char c, ICoords coords)
    {
        return c switch
        {
            '.' => new EmptyField(coords),
            'L' => new MaleLion(coords),
            'l' => new FemaleLion(coords),
            'K' => new MaleCrocodile(coords),
            'k' => new FemaleCrocodile(coords),
            'S' => new MaleElephant(coords),
            's' => new FemaleElephant(coords),
            'A' => new MaleAntelope(coords),
            'a' => new FemaleAntelope(coords),
            _ => throw new ArgumentException($"Unrecognized char {c}."),
        };
    }
}
public abstract class Animal : Field, IAnimal
{
    protected Animal(ICoords coords) : base(coords)
    {
    }

    public string Species { get; protected set; }
    public bool IsMale { get; protected set; }
    public bool IsPredator { get; protected set; }

    public abstract void Move();
    public void Reproduction()
    {
        throw new NotImplementedException();

    }
}
public abstract class Predator : Animal, IPredator
{
    public Predator(ICoords coords) : base(coords)
    {
        IsPredator = true;
    }

    public void Eat()
    {
        throw new NotImplementedException();
    }
}
public abstract class Lion : Predator
{
    public Lion(ICoords coords) : base(coords)
    {
        Species = "Lion";
    }
    public override void Move()
    {
        throw new NotImplementedException();
    }
}
public abstract class Crocodile : Predator
{
    public Crocodile(ICoords coords) : base(coords)
    {
        Species = "Crocodile";
    }
    public override void Move()
    {
        throw new NotImplementedException();
    }
}
public abstract class Elephant : Animal
{
    public Elephant(ICoords coords) : base(coords)
    {
        Species = "Elephant";
    }
    public override void Move()
    {
        throw new NotImplementedException();
    }
}
public abstract class Antelope : Animal
{
    public Antelope(ICoords coords) : base(coords)
    {
        Species = "Antelope";
    }
    public override void Move()
    {
        throw new NotImplementedException();
    }
}
public class MaleLion : Lion
{
    public MaleLion(ICoords coords) : base(coords)
    {
        IsMale = true;
    }
    public override string Display { get; protected set; } = "L";
}
public class FemaleLion : Lion
{
    public FemaleLion(ICoords coords) : base(coords)
    {
        IsMale = false;
    }
    public override string Display { get; protected set; } = "l";
}
public class MaleCrocodile : Crocodile
{
    public MaleCrocodile(ICoords coords) : base(coords)
    {
        IsMale = true;
    }
    public override string Display { get; protected set; } = "C";
}
public class FemaleCrocodile : Crocodile
{
    public FemaleCrocodile(ICoords coords) : base(coords)
    {
        IsMale = false;
    }
    public override string Display { get; protected set; } = "c";
}
public class MaleElephant : Elephant
{
    public MaleElephant(ICoords coords) : base(coords)
    {
        IsMale = true;
    }
    public override string Display { get; protected set; } = "E";
}
public class FemaleElephant : Elephant
{
    public FemaleElephant(ICoords coords) : base(coords)
    {
        IsMale = false;
    }
    public override string Display { get; protected set; } = "e";
}
public class MaleAntelope : Antelope
{
    public MaleAntelope(ICoords coords) : base(coords)
    {
        IsMale = true;
    }
    public override string Display { get; protected set; } = "A";
}
public class FemaleAntelope : Antelope
{
    public FemaleAntelope(ICoords coords) : base(coords)
    {
        IsMale = false;
    }
    public override string Display { get; protected set; } = "a";
}