using System;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;

namespace SpaceLibrary
{
    public struct Position
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Position(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static Position operator +(Position self, Position other)
        {
            return new Position(self.X + other.X, self.Y + other.Y);
        }

        public override string ToString()
        {
            return $"(x: {X}, y: {Y})";
        }
    }

    /// <summary>
    /// Class <c>SpaceObject</c> models a SpaceObject in two-dimensional space.
    /// </summary>
    /// <param name="Name">Name</param>
    /// <param name="OrbitalRadius">The objects orbital radius in km</param>
    /// <param name="OrbitalPeriod">The measure of an orbit in days</param>
    /// <param name="ObjectRadius">The objects physical radius in km</param>
    /// <param name="RotationalPeriod">Length of a day</param>
    /// <param name="Color">Color of the object</param>
    /// <param name="Children">The objects' child objects</param>
    /// <param name="Parent">The objects' parent, if it has one</param>
    public class SpaceObject
    {
        public string Name { get; }
        public double OrbitalRadius { get; set; }
        public double OrbitalPeriod { get; }
        public double ObjectRadius { get; }
        public double RotationalPeriod { get; }
        public Color color { get; }
        public bool isCenterObject;
        public List<SpaceObject> Children { get; set; }
        public SpaceObject? Parent { get; set; }
        public Position Position { get; set; }

        public SpaceObject(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
        {
            this.Name = Name;
            this.OrbitalRadius = OrbitalRadius;
            this.OrbitalPeriod = OrbitalPeriod;
            this.ObjectRadius = ObjectRadius;
            this.RotationalPeriod = RotationalPeriod;
            this.color = color;
            isCenterObject = false;
            Position = new Position(OrbitalRadius, 0);
            Children = new List<SpaceObject>();
            Parent = null;
        }

        public virtual Position CalculatePosition(double Time)
        {
            double Progress = OrbitalPeriod == 0 ? 0 : Time / OrbitalPeriod;
            double Angle = Progress * Math.PI * 2;

            double XPos;
            double YPos;

            if (isCenterObject)
            {
                XPos = 0;
                YPos = 0;
            }
            else
            {
                XPos = scale(OrbitalRadius) * Math.Cos(Angle);
                YPos = scale(OrbitalRadius) * Math.Sin(Angle);
            }
            
            Position position = new Position(XPos, YPos);

            if (Parent != null && this is Moon)
            {
                position += Parent.Position;
            }

            Position = position;

            return position;
        }

        public void AddChild(SpaceObject child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public virtual void Draw()
        {
            Console.WriteLine(Name);
            Console.WriteLine($"Position: {Position.ToString()}");
            Console.WriteLine($"Orbital radius: {OrbitalRadius}");
            Console.WriteLine($"Orbital period: {OrbitalPeriod}");
            Console.WriteLine($"Object radius: {ObjectRadius}");
            Console.WriteLine($"Rotational period: {RotationalPeriod}");
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual string ToInfoString()
        {
            return String.Format(
                "Name: {0}\n" +
                "Orbital radius: {1}km\n" +
                "Orbital period: {2} earth days\n" +
                "Polar radius: {3}km\n" +
                "Rotational period: {4} earth days\n",
                Name, 
                OrbitalRadius.ToString("0,0.0", CultureInfo.InvariantCulture), 
                OrbitalPeriod.ToString("0,0.0", CultureInfo.InvariantCulture),
                ObjectRadius.ToString("0,0.0", CultureInfo.InvariantCulture), 
                RotationalPeriod
                );
        }

        private double scale(double x)
        {
            return (Math.Pow(x, 1.0 / 2.2) / Math.Log(x) - 100) / 2;
        }
    }

    public class Star : SpaceObject
    {
        public Star(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Star: ");
            base.Draw();
        }
    }

    public class Planet : SpaceObject
    {
        public Planet(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Planet: ");
            base.Draw();
        }

        public override string ToInfoString()
        {
            return base.ToInfoString() + "Moons: " + String.Join(", ", Children);
        }
    }

    public class Moon : Planet
    {
        public Moon(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Moon: ");
            base.Draw();
        }
    }

    public class Comet : SpaceObject
    {
        public Comet(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Asteroid: ");
            base.Draw();
        }
    }

    public class Asteroid : SpaceObject
    {
        public Asteroid(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Asteroid: ");
            base.Draw();
        }
    }

    public class AsteroidBelt : SpaceObject
    {
        public AsteroidBelt(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Asteroid Belt: ");
            base.Draw();
        }
    }

    public class DwarfPlanet : Planet
    {
        public DwarfPlanet(string Name, double OrbitalRadius, double OrbitalPeriod, double ObjectRadius, double RotationalPeriod, Color color)
            : base(Name, OrbitalRadius, OrbitalPeriod, ObjectRadius, RotationalPeriod, color) { }
        public override void Draw()
        {
            Console.Write("Dwarf Planet: ");
            base.Draw();
        }
    }
}
