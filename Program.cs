using SimplexNoise;
using ScottPlot;
using System.Globalization;

static void ConfigurePolarPlot(Plot plot, int revolutionSeconds, int revolutions, double step, double radiusAtZero, double radiusExtent)
{
    int samples = (int)Math.Floor(revolutionSeconds / step);
    double radiusAtMinusOne = radiusAtZero - radiusExtent;
    double radiusAtPlusOne = radiusAtZero + radiusExtent;

    var polar = plot.Add.PolarAxis(radiusAtPlusOne);
    polar.Clockwise = true;
    polar.Rotation = Angle.FromDegrees(-90);
    polar.SetCircles(
        [radiusAtMinusOne, radiusAtZero, radiusAtPlusOne],
        [$"−1 → {radiusAtMinusOne:g}", $"0 → {radiusAtZero:g}", $"+1 → {radiusAtPlusOne:g}"]);
    polar.SetSpokes(
        ["0 s", $"{revolutionSeconds / 4} s", $"{revolutionSeconds / 2} s", $"{revolutionSeconds * 3 / 4} s"],
        radiusAtPlusOne * 1.08);

    for (int revolution = 0; revolution < revolutions; revolution++)
    {
        Coordinates[] trace = new Coordinates[samples + 1];
        for (int sample = 0; sample <= samples; sample++)
        {
            double localTime = sample * step;
            float noise = Noise.Generate((float)(revolution * revolutionSeconds + localTime));
            double degrees = localTime / revolutionSeconds * 360;
            trace[sample] = polar.GetCoordinates(radiusAtZero + noise * radiusExtent, degrees);
        }

        var points = plot.Add.ScatterPoints(trace);
        points.Color = Color.FromHSL((float)revolution / revolutions, 0.8f, 0.45f, 1);
        points.MarkerSize = 1;
    }

    plot.Title($"1 revolution = {revolutionSeconds} s | interval = {step:g} s | {revolutions} revolutions overlaid");
}

string graphPath = Path.GetFullPath(args.FirstOrDefault() ?? "simplex-noise-polar-comparison.png");
double radiusAtZero = args.Length > 1 ? double.Parse(args[1], CultureInfo.InvariantCulture) : 2;
double radiusExtent = args.Length > 2 ? double.Parse(args[2], CultureInfo.InvariantCulture) : 1;
int firstRevolutionSeconds = args.Length > 3 ? int.Parse(args[3], CultureInfo.InvariantCulture) : 60;
int secondRevolutionSeconds = args.Length > 4 ? int.Parse(args[4], CultureInfo.InvariantCulture) : 256;
int revolutions = args.Length > 5 ? int.Parse(args[5], CultureInfo.InvariantCulture) : 5;
double step = args.Length > 6 ? double.Parse(args[6], CultureInfo.InvariantCulture) : 0.001;
if (!(0 < radiusExtent && radiusExtent < radiusAtZero))
    throw new ArgumentOutOfRangeException(nameof(args), "Radii must satisfy 0 < extent < radius(0).");
if (firstRevolutionSeconds <= 0 || secondRevolutionSeconds <= 0 || revolutions <= 0 || !double.IsFinite(step) || step <= 0)
    throw new ArgumentOutOfRangeException(nameof(args), "Revolution seconds, revolution count, and interval must be positive.");

Multiplot multiplot = new();
multiplot.AddPlots(2);
ConfigurePolarPlot(multiplot.Subplots.GetPlot(0), firstRevolutionSeconds, revolutions, step, radiusAtZero, radiusExtent);
ConfigurePolarPlot(multiplot.Subplots.GetPlot(1), secondRevolutionSeconds, revolutions, step, radiusAtZero, radiusExtent);
multiplot.Layout = new ScottPlot.MultiplotLayouts.Columns();
multiplot.SavePng(graphPath, 1280, 640);
Console.WriteLine($"Graph: {graphPath}");
