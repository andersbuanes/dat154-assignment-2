using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;

using SpaceLibrary;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer timer;
        SpaceObject obj;
        double DaysPassed = 0;
        double Speed = 0.5;
        bool showOrbit = true, showText = true;

        public event EventHandler? MyTick;

        protected void OnTick(object? sender, EventArgs e)
        {
            DaysPassed += Speed;
            DaysPassedText.Text = "Day: " + ((int)(DaysPassed)).ToString();
            MyTick?.Invoke(sender, e);
            Draw();
        }

        public void Subscribe(SpaceObject obj)
        {
            MyTick += (object? sender, EventArgs e) => obj.CalculatePosition(DaysPassed);
        }

        public void Unsubscribe(SpaceObject obj)
        {
            MyTick -= (object? sender, EventArgs e) => obj.CalculatePosition(DaysPassed);
        }

        public MainWindow()
        {
            InitializeComponent();
            obj = CreateSpace(); // Create Solar System with Sun as parent object
            InitDropDown();
            timer = new DispatcherTimer();
            

            SizeChanged += new SizeChangedEventHandler((object? sender, SizeChangedEventArgs e) => Draw());
            MouseRightButtonDown += Window_MouseRightButtonDown;
            KeyDown += Window_KeyDown;
            //PlanetInfoCanvas.MouseRightButtonDown += PlanetInfoCanvas_MouseRightButtonDown;
            PlanetTextButton.Click += PlanetTextButton_Click;
            PlanetOrbitButton.Click += PlanetOrbitButton_Click;

            timer.Interval = new TimeSpan(15000);
            timer.Tick += OnTick;
            timer.Start();

            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }

        private void PlanetOrbitButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in SolarSystemCanvas.Children)
            {
                if (element is Ellipse && (element as Ellipse).Name.Equals("Orbit"))
                {
                    element.Visibility = showOrbit ? Visibility.Visible : Visibility.Hidden;
                }
            }

            showOrbit = !showOrbit;

            PlanetOrbitButton.Content = showOrbit ? "Hide Orbits" : "Show Orbits";
        }

        private void PlanetTextButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in SolarSystemCanvas.Children)
            {
                if (element is TextBlock)
                {
                    element.Visibility = showText ? Visibility.Visible : Visibility.Hidden;
                }
            }

            showText = !showText;

            PlanetTextButton.Content = showText ? "Hide Text" : "Show Text";
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlanetInfoText.Visibility = Visibility.Hidden;

            if (obj.Parent != null)
            {
                obj = obj.Parent;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double change = 0.1;
            switch (e.Key)
            {
                case Key.Up or Key.Right:
                    if (Speed + change <= 5)
                        Speed += change;
                    break;
                case Key.Down or Key.Left:
                    if (Speed - change >= 0.1)
                        Speed -= change;
                    break;
                default:
                    break;
            }
        }

        public void Draw()
        {
            Width = Width < 800 ? 800 : Width;
            Height = Height < 450 ? 450 : Height;

            SolarSystemCanvas.Children.Clear();

            if (showOrbit)
                DrawOrbits();

            if (showText)
                DrawObjectNames();

            DrawObject(obj);
            obj.Children.ForEach(child => DrawObject(child));

        }

        private void DrawObject(SpaceObject obj)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = new SolidColorBrush(ToMediaColor(obj.color));
            ellipse.Width = ScaleValue(obj.ObjectRadius, 2.0);
            ellipse.Height = ellipse.Width;

            if (obj.Parent != null)
                obj.CalculatePosition(DaysPassed);

            Canvas.SetLeft(ellipse, obj.Position.X + (Width / 2) - (ellipse.Width / 2));
            Canvas.SetTop(ellipse, obj.Position.Y + (Height / 2) - (ellipse.Height / 2));

            SolarSystemCanvas.Children.Add(ellipse);
        }

        private void DrawOrbits()
        {
            obj.Children.ForEach(obj => {
                if (obj is not Moon)
                    DrawOrbit(obj);
            });
        }

        private void DrawOrbit(SpaceObject obj)
        {
            Ellipse orbit = new Ellipse();
            orbit.Name = "Orbit";
            orbit.Stroke = new SolidColorBrush(ToMediaColor(DColor.White));

            double scaledRadius = ScaleValue(obj.OrbitalRadius, 2.2, -100);

            orbit.Width = scaledRadius;
            orbit.Height = scaledRadius;

            Canvas.SetLeft(orbit, (Width / 2) - (orbit.Width / 2));
            Canvas.SetTop(orbit, (Height / 2) - (orbit.Height / 2));

            SolarSystemCanvas.Children.Add(orbit);
        }

        private void DrawObjectNames()
        {
            DrawObjectName(obj);
            obj.Children.ForEach(obj => DrawObjectName(obj));
        }

        private void DrawObjectName(SpaceObject obj)
        {
            TextBlock text = new TextBlock();
            text.Foreground = new SolidColorBrush(ToMediaColor(DColor.White));
            text.Background = Brushes.Transparent;
            text.Text = obj.Name;
            text.Width = 100;
            text.Height = 20;

            double left = obj.Position.X;
            double top = obj.Position.Y;

            Canvas.SetLeft(text, left + (Width / 2) - (text.Width / 2));
            Canvas.SetTop(text, top + (Height / 2) - (text.Height / 2));

            SolarSystemCanvas.Children.Add(text);
        }

        private SpaceObject CreateSpace()
        {
            SpaceObject obj = new Star("Sun", 0f, 0f, 696_340f, 27f, DColor.Red);

            obj.AddChild(new Planet("Mercury", 57_910_000f, 87.97f, 2439f, 58.6f, DColor.OrangeRed));
            obj.AddChild(new Planet("Venus", 108_200_000f, 224.7f, 6052f, 243f, DColor.DimGray));

            SpaceObject earth = new Planet("Earth", 149_600_000f, 365.26f, 6387f, 0.99f, DColor.Green);
            earth.AddChild(new Moon("The Moon", 348_000f, 27f, 1737.4f, 29.5f, DColor.White));
            obj.AddChild(earth);

            SpaceObject mars = new Planet("Mars", 227_940_000f, 686.98f, 3393f, 1.025f, DColor.DarkOrange);
            mars.AddChild(new Moon("Phobos", 9375f, 0.38f, 11.266f, 0f, DColor.White));
            mars.AddChild(new Moon("Deimos", 23457.8f, 1.263f, 6200f, 0f, DColor.White));
            obj.AddChild(mars);

            SpaceObject jupiter = new Planet("Jupiter", 778_500_000f, 4331f, 69_911f, 1.025f, DColor.SandyBrown);
            jupiter.AddChild(new Moon("Io", 421_700f, 1.769f, 1821.6f, 0f, DColor.White));
            jupiter.AddChild(new Moon("Europa", 670_900f, 3.551f, 1560.8f, 0f, DColor.White));
            jupiter.AddChild(new Moon("Ganymede", 1_070_000f, 7.154f, 2634.1f, 0f, DColor.White));
            jupiter.AddChild(new Moon("Callisto", 1_883_000f, 16.689f, 2410.3f, 0f, DColor.White));
            obj.AddChild(jupiter);

            SpaceObject saturn = new Planet("Saturn", 1_432_000_000f, 10_747f, 58_232f, 1.025f, DColor.Brown);
            saturn.AddChild(new Moon("Mimas", 185.539f, 0.942f, 198.2f, 0f, DColor.White));
            saturn.AddChild(new Moon("Enceladus", 237.948f, 1.370f, 252.1f, 0f, DColor.White));
            saturn.AddChild(new Moon("Tethys", 294.619f, 1.887f, 531.1f, 0f, DColor.White));
            obj.AddChild(saturn);

            SpaceObject uranus = new Planet("Uranus", 2_867_000_000f, 30_589f, 24_622f, 1.025f, DColor.AliceBlue);
            uranus.AddChild(new Moon("Miranda", 129_900f, 1.413f, 235.8f, 0f, DColor.White));
            uranus.AddChild(new Moon("Ariel", 190_900f, 2.520f, 578.9f, 0f, DColor.White));
            uranus.AddChild(new Moon("Umbriel", 266_000f, 4.144f, 584.7f, 0f, DColor.White));
            uranus.AddChild(new Moon("Titania", 436_300f, 8.706f, 788.4f, 0f, DColor.White));
            uranus.AddChild(new Moon("Oberon", 583_500f, 13.463f, 761.4f, 0f, DColor.White));
            obj.AddChild(uranus);

            SpaceObject neptune = new Planet("Neptune", 4_515_000_000f, 59_800f, 25_622f, 1.025f, DColor.DeepSkyBlue);
            neptune.AddChild(new Moon("Triton", 354_800f, 5.88f, 1353.4f, 0f, DColor.White));
            neptune.AddChild(new Moon("Proteus", 117_647f, 1.122f, 210f, 0f, DColor.White));
            neptune.AddChild(new Moon("Nereid", 5_513_400f, 360.11f, 180f, 0f, DColor.White));
            obj.AddChild(neptune);

            obj.AddChild(new Planet("Pluto", 5_905_400_000f, 90_560f, 1188f, 1.025f, DColor.CadetBlue));

            foreach (SpaceObject child in obj.Children)
                Subscribe(child);

            return obj;
        }

        private void InitDropDown()
        {
            obj.Children.FindAll(obj => obj is Planet).ForEach(obj => PlanetDropDown.Items.Add(obj));
            PlanetDropDown.SelectionChanged += PlanetDropDown_SelectionChanged;
        }

        private void PlanetDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            obj = (Planet)(PlanetDropDown.SelectedItem);
            PlanetInfoText.Text = obj.ToInfoString();
            PlanetInfoText.Visibility = Visibility.Visible;
            /*    
            TransformGroup g = new TransformGroup();

                translateTransform = new TranslateTransform(-(scale * obj.Position.X), -(scale * obj.Position.Y));
                scaleTransform = new ScaleTransform(scale, scale);

                g.Children.Add(translateTransform);
                g.Children.Add(scaleTransform);

                SolarSystemCanvas.RenderTransform = g;
                // TODO add info text to PlanetInfoCanvas
                // TODO add info method to SpaceObject library
                obj.Children.ForEach(moon =>
                {
                    // TODO add info to PlanetInfoCanvas
                    // TODO add info method to SpaceObject library
                });
            */
        }

        private MColor ToMediaColor(DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        private double ScaleValue(double value, double factor, double optionalvalue = 0)
        {
            return (Math.Pow(value, 1.0 / factor) / Math.Log(value) + optionalvalue);
        }
    }
}
