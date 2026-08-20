namespace CSharpFeatures;

/// <summary>Base type for the shape hierarchy used by the pattern-matching demos.</summary>
public abstract record Shape;

/// <summary>A rectangle. One whose <see cref="Width"/> equals its <see cref="Height"/> is a square.</summary>
public sealed record Rectangle(double Width, double Height) : Shape;

/// <summary>A circle.</summary>
public sealed record Circle(double Diameter) : Shape;

/// <summary>A triangle -- included purely to give the switch demos a case with no special handling.</summary>
public sealed record Triangle(double Base, double Height) : Shape;
