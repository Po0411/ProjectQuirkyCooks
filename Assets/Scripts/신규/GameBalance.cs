using System.Collections.Generic;

/// <summary>
/// Contains centralised balancing values for the game. Storing these values
/// here allows designers to tweak numbers without digging through code. Other
/// scripts should reference this static class instead of hard‑coding numbers.
/// The values provided here are initial examples and can be adjusted to
/// achieve the desired gameplay feel.
/// </summary>
public static class GameBalance
{
    // Player stats
    public const int StartingHealth = 3;
    public const int StartingStamina = 10;
    public const int StartingAttack = 1;

    // Penalties and rewards
    public const int DeathPenaltyGold = 200;
    public const int WrongDeliveryPenalty = 100;

    // Food prices for delivered dishes. Keys correspond to the names of
    // ItemData assets (e.g. "CookedPotato"). DeliveryManager can reference this
    // directly or copy into its own menuPrices array.
    public static readonly Dictionary<string, int> FoodPrices = new Dictionary<string, int>
    {
        { "CookedPotato", 150 },
        { "CookedTomato", 160 },
        { "CookedApple", 180 },
        { "CookedWheat", 200 },
        { "CookedCabbage", 220 },
        { "CookedCorn", 240 }
    };
}