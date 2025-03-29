using System.Diagnostics.CodeAnalysis;

namespace GildedSanctuarySpecific;

public static class RoomEffectType
{
    [AllowNull] public static RoomSettings.RoomEffect.Type GRJGoldenFlakes = new(nameof(GRJGoldenFlakes), true);

    public static void UnregisterValues()
    {
        if (GRJGoldenFlakes is not null)
        {
            GRJGoldenFlakes.Unregister();
            GRJGoldenFlakes = null;
        }
    }
}

public static class PlacedObjectType
{
    [AllowNull] public static PlacedObject.Type GRJLeviathanPushBack = new(nameof(GRJLeviathanPushBack), true);

    public static void UnregisterValues()
    {
        if (GRJLeviathanPushBack is not null)
        {
            GRJLeviathanPushBack.Unregister();
            GRJLeviathanPushBack = null;
        }
    }
}