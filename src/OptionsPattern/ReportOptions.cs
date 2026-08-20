using System.ComponentModel.DataAnnotations;

namespace OptionsPattern;

/// <summary>
/// The options class every demo in this project binds or configures a different way, so the
/// same shape can be compared across strategies. <see cref="ConnectionString"/> and
/// <see cref="Timeout"/> are the two properties every demo actually sets;
/// <see cref="IntValue"/>/<see cref="StringValue"/>/<see cref="BoolValue"/> exist to show what
/// survives when a demo binds only part of this shape.
/// </summary>
public class ReportOptions
{
    /// <summary>The configuration section name every JSON-based demo binds from.</summary>
    public const string SectionName = "ReportOptions";

    /// <summary>Required by <see cref="Demos.ValidationDemo"/>: binding a source that omits this must fail validation.</summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Required to be positive by <see cref="Demos.ValidationDemo"/>.</summary>
    [Range(1, int.MaxValue)]
    public int Timeout { get; set; } = 30;

    /// <summary>Never set by most demos -- its untouched default (50) shows what "unbound" looks like.</summary>
    public int IntValue { get; set; } = 50;

    /// <summary>Never set by most demos.</summary>
    public string StringValue { get; set; } = string.Empty;

    /// <summary>Never set by most demos.</summary>
    public bool BoolValue { get; set; }

    /// <summary>Prints every property, so a demo's binding result is fully visible rather than just the two properties most demos touch.</summary>
    public override string ToString() =>
        $"ConnectionString=\"{ConnectionString}\", Timeout={Timeout}, IntValue={IntValue}, StringValue=\"{StringValue}\", BoolValue={BoolValue}";
}
