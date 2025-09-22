using Xunit.Abstractions;

namespace FitSyncHub.Common.UnitTests;

[Obsolete("Delete later")]
public class _100LevelPredictorUnitTest
{
    private static readonly Dictionary<int, int> _xpToLevelMapping;
    private readonly ITestOutputHelper _output;

    static _100LevelPredictorUnitTest()
    {
        var xpNeededForLevel = new HashSet<int>()
        {
            0, 750, 1500, 2500, 3500, 5000, 6500, 8000, 9500, 11000,
            13000, 15000, 17000, 19000, 21000, 23500, 26000, 28500, 31000, 33500,
            36500, 39500, 42500, 45500, 48500, 52000, 55500, 59500, 64000, 68500,
            73000, 78500, 84000, 89500, 95000, 101500, 108000, 114500, 121000, 127500,
            134500, 142500, 150500, 158500, 166500, 175500, 184500, 193500, 202500, 212000,
            221500, 231000, 240500, 250000, 260000, 270000, 280000, 290000, 300000, 310500,
            321000, 331500, 342000, 352500, 363500, 374500, 385500, 396500, 407500, 418500,
            429500, 441000, 452500, 464000, 475500, 487000, 498500, 510000, 522000, 534000,
            546000, 558000, 570000, 582000, 594000, 606500, 619000, 631500, 644000, 657000,
            670000, 683500, 697000, 711000, 725000, 740000, 755000, 771000, 787000, 807000,
        };

        _xpToLevelMapping = xpNeededForLevel
            .Select((xp, index) => new { Level = index + 1, XP = xp })
            .ToDictionary(x => x.Level, x => x.XP);
    }

    public _100LevelPredictorUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    // better use Excel forecast sheet with linear, polynomial, moving average, exponential smoothing etc.
    public void Calculate_WorkCorrectly(int polynomialRegression)
    {
        var dateToLevelMapping = new Dictionary<DateOnly, int>
        {
            { new DateOnly(2024,09,20), 30 },
            { new DateOnly(2025,01,19), 55 },
            { new DateOnly(2025,05,16), 69 },
            { new DateOnly(2025,09,04), 79 },
            { new DateOnly(2025,09,08), 80 },
            { new DateOnly(2025,09,20), 81 },
        };

        var predictedDate = PredictLevelDate(dateToLevelMapping, 100, degree: polynomialRegression);
        _output.WriteLine($"Predicted date for level 100: {predictedDate:yyyy-MM-dd}, polynom: {polynomialRegression}");
    }

    private static DateOnly PredictLevelDate(Dictionary<DateOnly, int> dateToLevel, int targetLevel, int degree = 2)
    {
        var targetXp = _xpToLevelMapping[targetLevel];

        // Convert to numeric (X = days, Y = xp)
        var points = dateToLevel
            .Select(p => new
            {
                X = p.Key.ToDateTime(TimeOnly.MinValue).ToOADate(),
                Y = (double)_xpToLevelMapping[p.Value]
            })
            .ToList();

        var n = points.Count;

        // Build Vandermonde matrix for least squares
        var X = new double[n, degree + 1];
        var Y = new double[n];

        for (var i = 0; i < n; i++)
        {
            var xi = points[i].X;
            var pow = 1.0;
            for (var j = 0; j <= degree; j++)
            {
                X[i, j] = pow;
                pow *= xi;
            }
            Y[i] = points[i].Y;
        }

        // Solve normal equations: (X^T X) c = X^T Y
        var xtX = new double[degree + 1, degree + 1];
        var xtY = new double[degree + 1];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j <= degree; j++)
            {
                xtY[j] += X[i, j] * Y[i];
                for (var k = 0; k <= degree; k++)
                {
                    xtX[j, k] += X[i, j] * X[i, k];
                }
            }
        }

        // Gaussian elimination to solve for coefficients
        var coeffs = SolveLinearSystem(xtX, xtY);

        // Now solve f(x) = targetXp
        // Only works robustly for degree <= 3 (quadratic/cubic). Otherwise need root-finding.
        Func<double, double> f = x =>
        {
            double sum = 0;
            var pow = 1.0;
            for (var j = 0; j < coeffs.Length; j++)
            {
                sum += coeffs[j] * pow;
                pow *= x;
            }
            return sum;
        };

        // Use binary search to approximate the root near last known point
        var left = points.Min(p => p.X);
        var right = points.Max(p => p.X) + 365; // look ahead 1 year
        for (var iter = 0; iter < 100; iter++)
        {
            var mid = (left + right) / 2;
            if (f(mid) < targetXp)
            {
                left = mid;
            }
            else
            {
                right = mid;
            }
        }

        var targetDate = DateTime.FromOADate((left + right) / 2);
        return DateOnly.FromDateTime(targetDate);
    }

    // Basic Gaussian elimination
    private static double[] SolveLinearSystem(double[,] A, double[] b)
    {
        var n = b.Length;
        var a = (double[,])A.Clone();
        var x = (double[])b.Clone();

        for (var i = 0; i < n; i++)
        {
            // pivot
            var max = i;
            for (var k = i + 1; k < n; k++)
            {
                if (Math.Abs(a[k, i]) > Math.Abs(a[max, i]))
                {
                    max = k;
                }
            }

            for (var k = i; k < n; k++)
            {
                (a[i, k], a[max, k]) = (a[max, k], a[i, k]);
            }

            (x[i], x[max]) = (x[max], x[i]);

            // normalize
            var div = a[i, i];
            for (var k = i; k < n; k++)
            {
                a[i, k] /= div;
            }

            x[i] /= div;

            // eliminate
            for (var j = 0; j < n; j++)
            {
                if (j == i)
                {
                    continue;
                }

                var factor = a[j, i];
                for (var k = i; k < n; k++)
                {
                    a[j, k] -= factor * a[i, k];
                }

                x[j] -= factor * x[i];
            }
        }

        return x;
    }
}
