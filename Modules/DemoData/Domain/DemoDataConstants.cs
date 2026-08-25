namespace backend.Modules.DemoData.Domain;

public static class DemoDataConstants
{
    // Fixed known credentials so whoever runs the seed can log into the demo accounts afterward.
    public const string DemoPassword = "Demo1234";

    public const string HeroEmail = "alex.morgan@fitspire.demo";
    public const string HeroUserName = "alexmorgan";
    public const string HeroDisplayName = "Alex Morgan";
    public const string HeroBio = "Marathon training, one step at a time. Chasing a sub-4 finish this year.";

    public static readonly (string DisplayName, string UserName, string Bio)[] FillerAccounts =
    [
        ("Jordan Lee", "jordanlee", "Lifting heavy, eating heavier."),
        ("Sam Rivera", "samrivera", "Weekend trail runner."),
        ("Taylor Chen", "taylorchen", "Yoga in the morning, gym at night."),
        ("Casey Kim", "caseykim", "Cyclist. Coffee enthusiast."),
        ("Morgan Reyes", "morganreyes", "Swimming my way to a triathlon."),
        ("Riley Patel", "rileypatel", "Consistency over intensity."),
        ("Jamie Novak", "jamienovak", "New to lifting, loving it."),
        ("Drew Santos", "drewsantos", "Marathon #3 in the books."),
        ("Avery Cole", "averycole", "Home gym gains."),
        ("Quinn Baxter", "quinnbaxter", "Chasing PRs one week at a time."),
    ];
}
