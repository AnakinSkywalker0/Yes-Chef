using NUnit.Framework;
using YesChef.Ingredients;
using YesChef.Stations;

namespace YesChef.Tests
{
    public class TrashStationTests : KitchenTestBase
    {
        [Test]
        public void EmptyHands_IsRefused()
        {
            var trash = CreateComponent<TrashStation>("Trash");
            int rejections = 0;
            trash.OnInteractionRejected += () => rejections++;

            trash.Interact(Player);

            Assert.AreEqual(1, rejections);
        }

        [Test]
        public void DiscardsRawAndPreparedAlike()
        {
            var trash = CreateComponent<TrashStation>("Trash");

            CreateIngredient(Meat, Player);
            trash.Interact(Player);
            Assert.IsFalse(Player.HasIngredient);

            CreateIngredient(Meat, Player, prepared: true);
            trash.Interact(Player);
            Assert.IsFalse(Player.HasIngredient);
        }

        [Test]
        public void DiscardedIngredient_IsDestroyed()
        {
            var trash = CreateComponent<TrashStation>("Trash");
            Ingredient cheese = CreateIngredient(Cheese, Player);

            trash.Interact(Player);

            Assert.IsTrue(cheese == null, "Unity's null check reports a destroyed object.");
        }
    }
}
