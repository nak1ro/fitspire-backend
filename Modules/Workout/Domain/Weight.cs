using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain;

/// <summary>
/// Value object representing weight in Kilograms.
/// Handles conversion and prevents negative values.
/// </summary>
public sealed class Weight : ValueObject
{
    public double Kilograms { get; }

    private Weight(double kg)
    {
        if (kg < 0)
            throw new DomainException("Weight cannot be negative.");
        Kilograms = kg;
    }

    public static Weight FromKilograms(double kg) => new(kg);
    
    // 1 lb = 0.45359237 kg
    public static Weight FromPounds(double lbs) => new(lbs * 0.45359237);

    public double ToPounds() => Kilograms / 0.45359237;

    public override string ToString() => $"{Kilograms:F2} kg";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Kilograms;
    }
    
    // Operator overloads for convenient math
    public static bool operator >(Weight left, Weight right) => left.Kilograms > right.Kilograms;
    public static bool operator <(Weight left, Weight right) => left.Kilograms < right.Kilograms;
    public static bool operator >=(Weight left, Weight right) => left.Kilograms >= right.Kilograms;
    public static bool operator <=(Weight left, Weight right) => left.Kilograms <= right.Kilograms;
}
