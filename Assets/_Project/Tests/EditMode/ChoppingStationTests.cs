using NUnit.Framework;
using YesChef.Ingredients;
using YesChef.Stations;

namespace YesChef.Tests
{
    public class ChoppingStationTests : KitchenTestBase
    {
        private const float ChopDuration = 2f;

        private ChoppingStation _table;
        private int _rejections;
        private int _interactions;

        [SetUp]
        public void SetUp()
        {
            _table = CreateComponent<ChoppingStation>("Table");
            _rejections = 0;
            _interactions = 0;
            _table.OnInteractionRejected += () => _rejections++;
            _table.OnInteracted += () => _interactions++;
        }

        [Test]
        public void PlacingRawVegetable_StartsChopping()
        {
            CreateIngredient(Vegetable, Player);

            _table.Interact(Player);

            Assert.IsTrue(_table.IsChopping);
            Assert.IsFalse(Player.HasIngredient);
            Assert.AreEqual(1, _interactions);
        }

        [Test]
        public void PlacingCheese_IsRefused()
        {
            Ingredient cheese = CreateIngredient(Cheese, Player);

            _table.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.AreSame(cheese, Player.HeldIngredient);
            Assert.IsFalse(_table.HasIngredient);
        }

        [Test]
        public void PlacingAlreadyChoppedVegetable_IsRefused()
        {
            CreateIngredient(Vegetable, Player, prepared: true);

            _table.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.IsTrue(Player.HasIngredient);
        }

        [Test]
        public void SecondVegetableWhileChopping_IsRefused()
        {
            CreateIngredient(Vegetable, Player);
            _table.Interact(Player);
            CreateIngredient(Vegetable, Player);

            _table.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.IsTrue(Player.HasIngredient);
        }

        [Test]
        public void CollectingWhileChopping_IsRefused()
        {
            CreateIngredient(Vegetable, Player);
            _table.Interact(Player);

            _table.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.IsTrue(_table.HasIngredient);
            Assert.IsFalse(Player.HasIngredient);
        }

        [Test]
        public void ChopCompletes_AfterDuration()
        {
            CreateIngredient(Vegetable, Player);
            _table.Interact(Player);

            _table.Advance(ChopDuration * 0.5f);
            Assert.IsTrue(_table.IsChopping);
            Assert.AreEqual(IngredientState.Raw, _table.HeldIngredient.State);

            _table.Advance(ChopDuration * 0.5f);
            Assert.IsFalse(_table.IsChopping);
            Assert.AreEqual(IngredientState.Prepared, _table.HeldIngredient.State);
        }

        [Test]
        public void CollectingChoppedVegetable_HandsItToThePlayer()
        {
            CreateIngredient(Vegetable, Player);
            _table.Interact(Player);
            _table.Advance(ChopDuration);

            _table.Interact(Player);

            Assert.IsFalse(_table.HasIngredient);
            Assert.IsTrue(Player.HasIngredient);
            Assert.IsTrue(Player.HeldIngredient.IsReadyForDelivery);
        }

        [Test]
        public void EmptyHandsOnEmptyTable_IsRefused()
        {
            _table.Interact(Player);

            Assert.AreEqual(1, _rejections);
        }

        [Test]
        public void ProgressBarContract_TracksTheChop()
        {
            int notifications = 0;
            _table.OnProgressChanged += () => notifications++;

            CreateIngredient(Vegetable, Player);
            _table.Interact(Player);
            Assert.IsTrue(_table.IsInProgress);

            _table.Advance(1f);
            Assert.AreEqual(0.5f, _table.Progress, 0.0001f);

            _table.Advance(1f);
            Assert.IsFalse(_table.IsInProgress);
            Assert.AreEqual(0f, _table.Progress);
            Assert.GreaterOrEqual(notifications, 3);
        }
    }
}
