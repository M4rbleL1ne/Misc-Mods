namespace LBHardhatCat;

sealed class HardhatTutorial : UpdatableAndDeletable
{
    internal HardhatTutorial(Room room) => this.room = room;

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is Room rm && rm.BeingViewed && rm.game is RainWorldGame game && game.cameras is RoomCamera[] ar && ar.Length > 0 && ar[0]?.hud?.textPrompt is HUD.TextPrompt txt && game.rainWorld?.inGameTranslator is InGameTranslator tr && game.Players.Count > 0 && game.Players[0]?.realizedCreature is Player pl)
        {
            pl.AddFood(1);
            txt.AddMessage(tr.Translate("You can regurgitate tiles when your stomach is empty and there are no swallowable objects in your hands for the cost of half a food pip."), 20, 600, true, true);
            txt.AddMessage(tr.Translate("Throw them to place them."), 20, 600, true, true);
            Destroy();
        }
    }
}