using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using YesChef.Ingredients;
using YesChef.Orders;

namespace YesChef.Tests
{
    /// <summary>
    /// Locks down the scoring rules from the design document. These are the fiddliest rules
    /// in the game (floored time penalty, duplicate ingredients, negative totals), which is
    /// exactly why <see cref="Order"/> was kept free of Unity lifecycle dependencies.
    /// </summary>
    public class OrderTests
    {
        private const int CheeseValue = 10;
        private const int VegetableValue = 20;
        private const int MeatValue = 30;

        private IngredientSO _cheese;
        private IngredientSO _vegetable;
        private IngredientSO _meat;

        [SetUp]
        public void SetUp()
        {
            _cheese = CreateIngredient("Cheese", CheeseValue, IngredientProcess.None);
            _vegetable = CreateIngredient("Vegetable", VegetableValue, IngredientProcess.Chop);
            _meat = CreateIngredient("Meat", MeatValue, IngredientProcess.Cook);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cheese);
            Object.DestroyImmediate(_vegetable);
            Object.DestroyImmediate(_meat);
        }

        [Test]
        public void BaseScore_IsSumOfIngredientValues()
        {
            var order = new Order(new[] { _cheese, _meat }, startTime: 0f);

            Assert.AreEqual(CheeseValue + MeatValue, order.BaseScore);
        }

        /// <summary>The worked example from the design document.</summary>
        [Test]
        public void CalculateScore_SubtractsOnePointPerElapsedSecond()
        {
            var order = new Order(new[] { _cheese, _meat }, startTime: 0f);

            Assert.AreEqual(26, order.CalculateScore(now: 14f));
        }

        [Test]
        public void CalculateScore_FloorsElapsedTime()
        {
            var order = new Order(new[] { _cheese, _meat }, startTime: 0f);

            // 14.99 seconds must only dock 14 points, not 15.
            Assert.AreEqual(26, order.CalculateScore(now: 14.99f));
        }

        [Test]
        public void CalculateScore_CanGoNegative()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f);

            Assert.AreEqual(-6, order.CalculateScore(now: 16f));
        }

        [Test]
        public void CalculateScore_HoldsFullValueDuringGrace()
        {
            var order = new Order(new[] { _cheese, _meat }, startTime: 0f, graceSeconds: 20f);

            Assert.AreEqual(40, order.CalculateScore(now: 0f));
            Assert.AreEqual(40, order.CalculateScore(now: 19.99f));
        }

        [Test]
        public void CalculateScore_OnlyPenalisesTimeBeyondGrace()
        {
            var order = new Order(new[] { _cheese, _meat }, startTime: 0f, graceSeconds: 20f);

            // 34s open, 20s of which were free: 14 points docked, as in the worked example.
            Assert.AreEqual(26, order.CalculateScore(now: 34f));
            Assert.AreEqual(26, order.CalculateScore(now: 34.99f));
        }

        [Test]
        public void CalculateScore_GraceIsMeasuredFromStartTime()
        {
            var order = new Order(new[] { _cheese }, startTime: 100f, graceSeconds: 5f);

            Assert.AreEqual(10, order.CalculateScore(now: 104f));
            Assert.AreEqual(9, order.CalculateScore(now: 106f));
        }

        [Test]
        public void IsWithinGrace_IsTrueUntilTheGraceExpires()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f, graceSeconds: 10f);

            Assert.IsTrue(order.IsWithinGrace(now: 0f));
            Assert.IsTrue(order.IsWithinGrace(now: 9.99f));
            Assert.IsFalse(order.IsWithinGrace(now: 10f));
        }

        [Test]
        public void IsWithinGrace_IsNeverTrueWithoutAGracePeriod()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f);

            Assert.IsFalse(order.IsWithinGrace(now: 0f));
        }

        [Test]
        public void GetGraceRemaining_CountsDownToZero()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f, graceSeconds: 10f);

            Assert.AreEqual(10f, order.GetGraceRemaining(now: 0f));
            Assert.AreEqual(4f, order.GetGraceRemaining(now: 6f));
            Assert.AreEqual(0f, order.GetGraceRemaining(now: 30f));
        }

        [Test]
        public void Constructor_ClampsNegativeGraceToZero()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f, graceSeconds: -5f);

            Assert.AreEqual(0f, order.GraceSeconds);
            Assert.AreEqual(9, order.CalculateScore(now: 1f));
        }

        [Test]
        public void TryDeliver_RejectsIngredientTheOrderDoesNotWant()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f);

            Assert.IsFalse(order.TryDeliver(_meat));
            Assert.IsFalse(order.IsComplete);
        }

        [Test]
        public void TryDeliver_RequiresOneDeliveryPerDuplicate()
        {
            var order = new Order(new[] { _meat, _meat, _meat }, startTime: 0f);

            Assert.IsTrue(order.TryDeliver(_meat));
            Assert.IsFalse(order.IsComplete, "One of three meats should not complete the order.");

            Assert.IsTrue(order.TryDeliver(_meat));
            Assert.IsFalse(order.IsComplete);

            Assert.IsTrue(order.TryDeliver(_meat));
            Assert.IsTrue(order.IsComplete);
        }

        [Test]
        public void TryDeliver_RejectsExtraIngredientsOnceComplete()
        {
            var order = new Order(new[] { _cheese }, startTime: 0f);

            Assert.IsTrue(order.TryDeliver(_cheese));
            Assert.IsTrue(order.IsComplete);
            Assert.IsFalse(order.TryDeliver(_cheese));
        }

        [Test]
        public void IsDelivered_TracksSlotsIndependently()
        {
            var order = new Order(new[] { _cheese, _vegetable }, startTime: 0f);

            order.TryDeliver(_vegetable);

            Assert.IsFalse(order.IsDelivered(0));
            Assert.IsTrue(order.IsDelivered(1));
        }

        [Test]
        public void GetElapsedSeconds_NeverGoesNegative()
        {
            var order = new Order(new[] { _cheese }, startTime: 10f);

            Assert.AreEqual(0f, order.GetElapsedSeconds(now: 5f));
        }

        /// <summary>
        /// Builds a throwaway ingredient asset. The fields are private and serialized by
        /// design, so the test writes them the same way the inspector would.
        /// </summary>
        private static IngredientSO CreateIngredient(string displayName, int scoreValue, IngredientProcess process)
        {
            var ingredient = ScriptableObject.CreateInstance<IngredientSO>();
            var serialized = new SerializedObject(ingredient);

            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_scoreValue").intValue = scoreValue;
            serialized.FindProperty("_requiredProcess").enumValueIndex = (int)process;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return ingredient;
        }
    }
}
