using NUnit.Framework;
using UnityEngine;
using YesChef.Ingredients;
using YesChef.Stations;

namespace YesChef.Tests
{
    public class StoveStationTests : KitchenTestBase
    {
        private const float CookDuration = 6f;

        private StoveStation _stove;
        private StoveSlot _burnerA;
        private StoveSlot _burnerB;
        private int _rejections;

        [SetUp]
        public void SetUp()
        {
            _stove = CreateComponent<StoveStation>("Stove");
            _burnerA = CreateComponent<StoveSlot>("Burner_A");
            _burnerB = CreateComponent<StoveSlot>("Burner_B");
            _burnerA.transform.SetParent(_stove.transform);
            _burnerB.transform.SetParent(_stove.transform);
            SetReferences(_stove, "_slots", _burnerA, _burnerB);

            _rejections = 0;
            _stove.OnInteractionRejected += () => _rejections++;
        }

        [Test]
        public void RawMeat_FillsBurnersInOrder()
        {
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            Assert.IsTrue(_burnerA.IsCooking);
            Assert.IsTrue(_burnerB.IsEmpty);

            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            Assert.IsTrue(_burnerB.IsCooking);
            Assert.IsFalse(Player.HasIngredient);
        }

        [Test]
        public void ThirdMeat_IsRefusedWhenBothBurnersBusy()
        {
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            Ingredient third = CreateIngredient(Meat, Player);

            _stove.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.AreSame(third, Player.HeldIngredient);
        }

        [Test]
        public void RawVegetable_IsRefused()
        {
            CreateIngredient(Vegetable, Player);

            _stove.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.IsTrue(Player.HasIngredient);
            Assert.IsTrue(_burnerA.IsEmpty);
        }

        [Test]
        public void CollectingWhileStillCooking_IsRefused()
        {
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            _burnerA.Advance(CookDuration * 0.5f);

            _stove.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.IsFalse(Player.HasIngredient);
        }

        [Test]
        public void CookedMeat_CanBeCollected()
        {
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            _burnerA.Advance(CookDuration);
            Assert.IsTrue(_burnerA.HoldsCookedIngredient);

            _stove.Interact(Player);

            Assert.IsTrue(Player.HasIngredient);
            Assert.AreEqual(IngredientState.Prepared, Player.HeldIngredient.State);
            Assert.IsTrue(_burnerA.IsEmpty);
        }

        [Test]
        public void BurnersCookIndependently()
        {
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            _burnerA.Advance(CookDuration * 0.75f);

            CreateIngredient(Meat, Player);
            _stove.Interact(Player);

            _burnerA.Advance(CookDuration * 0.25f);
            _burnerB.Advance(CookDuration * 0.25f);

            Assert.IsTrue(_burnerA.HoldsCookedIngredient, "First burner started earlier and should finish first.");
            Assert.IsTrue(_burnerB.IsCooking);
        }

        [Test]
        public void CollectingTakesTheFinishedBurner_NotTheBusyOne()
        {
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            CreateIngredient(Meat, Player);
            _stove.Interact(Player);
            _burnerB.Advance(CookDuration);

            _stove.Interact(Player);

            Assert.IsTrue(_burnerB.IsEmpty, "The cooked burner must be the one emptied.");
            Assert.IsTrue(_burnerA.IsCooking);
        }
    }
}
