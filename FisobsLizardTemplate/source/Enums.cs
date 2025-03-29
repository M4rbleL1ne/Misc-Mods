namespace FisobsLizardTemplate;

public static class CreatureTemplateType
{
    public static CreatureTemplate.Type MyLittleLizard = new(nameof(MyLittleLizard), true);

    public static void UnregisterValues()
    {
        if (MyLittleLizard is not null)
        {
            MyLittleLizard.Unregister();
            MyLittleLizard = null!;
        }
    }
}

public static class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID MyLittleLizard = new(nameof(MyLittleLizard), true);

    public static void UnregisterValues()
    {
        if (MyLittleLizard is not null)
        {
            MyLittleLizard.Unregister();
            MyLittleLizard = null!;
        }
    }
}