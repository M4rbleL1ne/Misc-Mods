using System.Diagnostics.CodeAnalysis;

namespace LBHardhatCat;

public static class SandboxUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LBHardhatCatRuntile = new(nameof(LBHardhatCatRuntile), true),
        LBHardhatCatRocktile = new(nameof(LBHardhatCatRocktile), true);

    public static void UnregisterValues()
    {
        if (LBHardhatCatRuntile is not null)
        {
            LBHardhatCatRuntile.Unregister();
            LBHardhatCatRuntile = null;
        }
        if (LBHardhatCatRocktile is not null)
        {
            LBHardhatCatRocktile.Unregister();
            LBHardhatCatRocktile = null;
        }
    }
}

public static class MiscItemType
{
    [AllowNull] public static SLOracleBehaviorHasMark.MiscItemType LBHardhatCatRuntile = new(nameof(LBHardhatCatRuntile), true),
        LBHardhatCatRocktile = new(nameof(LBHardhatCatRocktile), true);

    public static void UnregisterValues()
    {
        if (LBHardhatCatRuntile is not null)
        {
            LBHardhatCatRuntile.Unregister();
            LBHardhatCatRuntile = null;
        }
        if (LBHardhatCatRocktile is not null)
        {
            LBHardhatCatRocktile.Unregister();
            LBHardhatCatRocktile = null;
        }
    }
}

public static class AbstractPhysicalObjectType
{
    [AllowNull]
    public static AbstractPhysicalObject.AbstractObjectType LBHardhatCatRuntile = new(nameof(LBHardhatCatRuntile), true),
        LBHardhatCatRocktile = new(nameof(LBHardhatCatRocktile), true);

    public static void UnregisterValues()
    {
        if (LBHardhatCatRuntile is not null)
        {
            LBHardhatCatRuntile.Unregister();
            LBHardhatCatRuntile = null;
        }
        if (LBHardhatCatRocktile is not null)
        {
            LBHardhatCatRocktile.Unregister();
            LBHardhatCatRocktile = null;
        }
    }
}

public static class SlugcatUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SlugcatUnlockID LBHardhatCat = new("Hardhat Cat", true);

    public static void UnregisterValues()
    {
        if (LBHardhatCat is not null)
        {
            LBHardhatCat.Unregister();
            LBHardhatCat = null;
        }
    }
}