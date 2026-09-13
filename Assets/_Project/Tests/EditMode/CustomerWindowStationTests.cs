using NUnit.Framework;
using UnityEngine;
using YesChef.Ingredients;
using YesChef.Orders;
using YesChef.Stations;

namespace YesChef.Tests
{
    public class CustomerWindowStationTests : KitchenTestBase
    {
        private CustomerWindowStation _window;
        private int _rejections;
        private int _completions;
        private int _awarded;

        [SetUp]
        public void SetUp()
        {
            _window = CreateComponent<CustomerWindowStation>("Window");
            _rejections = 0;
            _completions = 0;
            _window.OnInteractionRejected += () => _rejections++;
            _window.OnOrderCompleted += (window, order, points) =>
            {
                _completions++;
                _awarded = points;
            };
        }

        [Test]
        public void NoOrder_RefusesDelivery()
        {
            Ingredient cheese = CreateIngredient(Cheese, Player);

            _window.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.AreSame(cheese, Player.HeldIngredient, "Ingredient must remain in hand.");
        }

        [Test]
        public void EmptyHands_IsRefused()
        {
            _window.AssignOrder(new Order(new[] { Cheese }, Time.time));

            _window.Interact(Player);

            Assert.AreEqual(1, _rejections);
        }

        [Test]
        public void UnpreparedIngredient_StaysInHand()
        {
            _window.AssignOrder(new Order(new[] { Meat }, Time.time));
            Ingredient rawMeat = CreateIngredient(Meat, Player);

            _window.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.AreSame(rawMeat, Player.HeldIngredient);
            Assert.IsFalse(_window.CurrentOrder.IsDelivered(0));
        }

        [Test]
        public void UnwantedIngredient_StaysInHand()
        {
            _window.AssignOrder(new Order(new[] { Meat }, Time.time));
            Ingredient cheese = CreateIngredient(Cheese, Player);

            _window.Interact(Player);

            Assert.AreEqual(1, _rejections);
            Assert.AreSame(cheese, Player.HeldIngredient);
        }

        [Test]
        public void PartialDelivery_TicksRequirementWithoutCompleting()
        {
            _window.AssignOrder(new Order(new[] { Cheese, Meat }, Time.time));
            CreateIngredient(Cheese, Player);

            _window.Interact(Player);

            Assert.IsFalse(Player.HasIngredient, "Delivered ingredient leaves the hand.");
            Assert.IsTrue(_window.CurrentOrder.IsDelivered(0));
            Assert.IsFalse(_window.CurrentOrder.IsDelivered(1));
            Assert.AreEqual(0, _completions);
        }

        [Test]
        public void FullDelivery_CompletesAndAwardsPoints()
        {
            var order = new Order(new[] { Cheese, Meat }, Time.time);
            _window.AssignOrder(order);

            CreateIngredient(Cheese, Player);
            _window.Interact(Player);
            CreateIngredient(Meat, Player, prepared: true);
            _window.Interact(Player);

            Assert.AreEqual(1, _completions);
            Assert.IsFalse(_window.HasOrder, "Window empties once the order is settled.");
            Assert.AreEqual(order.BaseScore, _awarded, "Delivered instantly, so no time penalty.");
        }

        [Test]
        public void DuplicateRequirements_NeedOneDeliveryEach()
        {
            _window.AssignOrder(new Order(new[] { Cheese, Cheese }, Time.time));

            CreateIngredient(Cheese, Player);
            _window.Interact(Player);
            Assert.AreEqual(0, _completions);

            CreateIngredient(Cheese, Player);
            _window.Interact(Player);
            Assert.AreEqual(1, _completions);
        }

        [Test]
        public void OnOrderChanged_FiresForAssignDeliverAndClear()
        {
            int changes = 0;
            _window.OnOrderChanged += _ => changes++;

            _window.AssignOrder(new Order(new[] { Cheese }, Time.time));
            CreateIngredient(Cheese, Player);
            _window.Interact(Player);

            // Assign, then the completing delivery clears the window.
            Assert.AreEqual(2, changes);
        }
    }
}
