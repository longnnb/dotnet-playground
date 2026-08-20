namespace CSharpFeatures;

/// <summary>
/// Demonstrates four ways to branch on a shape's runtime type -- from the pre-C#7 <c>is</c> +
/// cast idiom through to a C# 8 <c>switch</c> expression with positional patterns -- run side
/// by side over the same shapes so the progression, and what each style can and can't express,
/// is directly comparable.
/// </summary>
public static class ShapePatternsDemo
{
    private static readonly Shape[] Shapes =
    [
        new Rectangle(Width: 3, Height: 4),
        new Rectangle(Width: 5, Height: 5),
        new Circle(Diameter: 2),
        new Triangle(Base: 6, Height: 2),
    ];

    /// <summary>Runs all four description styles over the same shapes.</summary>
    public static Task RunAsync()
    {
        foreach (var shape in Shapes)
        {
            Console.WriteLine($"{shape}");
            Console.WriteLine($"  legacy:            {DescribeLegacy(shape)}");
            Console.WriteLine($"  type pattern:      {DescribeWithTypePattern(shape)}");
            Console.WriteLine($"  switch statement:  {DescribeWithSwitchStatement(shape)}");
            Console.WriteLine($"  switch expression: {DescribeWithSwitchExpression(shape)}");
        }

        return Task.CompletedTask;
    }

    /// <summary>Pre-C#7: <c>is</c> as a plain boolean test followed by a separate cast, and <c>as</c> with a null check.</summary>
    private static string DescribeLegacy(Shape shape)
    {
        if (shape is Rectangle)
        {
            var rectangle = (Rectangle)shape;
            return rectangle.Width == rectangle.Height ? "square (legacy cast)" : "rectangle (legacy cast)";
        }

        var circle = shape as Circle;
        if (circle != null)
        {
            return $"circle, diameter {circle.Diameter} (legacy `as`)";
        }

        return "unrecognized shape (legacy)";
    }

    /// <summary>C# 7 type patterns (<c>is Rectangle r</c>), including the negated <c>is not Circle</c> form added in C# 9.</summary>
    private static string DescribeWithTypePattern(Shape shape)
    {
        if (shape is Rectangle { Width: var w, Height: var h } && w == h)
        {
            return "square (type pattern)";
        }

        if (shape is Rectangle r)
        {
            return $"rectangle {r.Width}x{r.Height} (type pattern)";
        }

        if (shape is not Circle)
        {
            return "not a circle (type pattern, negated)";
        }

        return "circle (type pattern)";
    }

    /// <summary>Classic <c>switch</c> statement with a <c>when</c> guard for the square special case.</summary>
    private static string DescribeWithSwitchStatement(Shape shape)
    {
        switch (shape)
        {
            case Rectangle sq when sq.Width == sq.Height:
                return "square (switch statement)";
            case Rectangle rr:
                return $"rectangle {rr.Width}x{rr.Height} (switch statement)";
            case Circle c:
                return $"circle, diameter {c.Diameter} (switch statement)";
            default:
                return "unrecognized shape (switch statement)";
        }
    }

    /// <summary>C# 8 <c>switch</c> expression with positional record patterns.</summary>
    private static string DescribeWithSwitchExpression(Shape shape) => shape switch
    {
        Rectangle(var w, var h) when w == h => "square (switch expression)",
        Rectangle(var w, var h) => $"rectangle {w}x{h} (switch expression)",
        Circle(var d) => $"circle, diameter {d} (switch expression)",
        _ => "unrecognized shape (switch expression)",
    };
}

/* Expected output
Rectangle { Width = 3, Height = 4 }
  legacy:            rectangle (legacy cast)
  type pattern:      rectangle 3x4 (type pattern)
  switch statement:  rectangle 3x4 (switch statement)
  switch expression: rectangle 3x4 (switch expression)
Rectangle { Width = 5, Height = 5 }
  legacy:            square (legacy cast)
  type pattern:      square (type pattern)
  switch statement:  square (switch statement)
  switch expression: square (switch expression)
Circle { Diameter = 2 }
  legacy:            circle, diameter 2 (legacy `as`)
  type pattern:      circle (type pattern)
  switch statement:  circle, diameter 2 (switch statement)
  switch expression: circle, diameter 2 (switch expression)
Triangle { Base = 6, Height = 2 }
  legacy:            unrecognized shape (legacy)
  type pattern:      not a circle (type pattern, negated)
  switch statement:  unrecognized shape (switch statement)
  switch expression: unrecognized shape (switch expression)
*/
