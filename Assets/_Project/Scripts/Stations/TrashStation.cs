using YesChef.Characters;
using YesChef.Ingredients;

namespace YesChef.Stations
{
    /// <summary>Discards whatever the player is carrying, raw or prepared.</summary>
    public class TrashStation : BaseStation
    {
        public override void Interact(PlayerController player)
        {
            if (!player.HasIngredient)
            {
                RaiseRejected();
                return;
            }

            Ingredient discarded = player.HeldIngredient;
            discarded.DestroySelf();
            RaiseInteracted();
        }
    }
}
