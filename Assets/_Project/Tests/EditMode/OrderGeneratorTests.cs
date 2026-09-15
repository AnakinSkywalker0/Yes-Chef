using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using YesChef.Ingredients;
using YesChef.Orders;

namespace YesChef.Tests
{
    /// <summary>
    /// Checks the order-composition rules: only two or three ingredients, drawn from the
    /// pool, with duplicates permitted.
    /// </summary>
    public class OrderGeneratorTests
    {
        private const int SampleCount = 400;

        private List<IngredientSO> _pool;

        [SetUp]
        public void SetUp()
        {
            _pool = new List<IngredientSO>
            {
                CreateIngredient("Cheese", 10),
                CreateIngredient("Vegetable", 20),
                CreateIngredient("Meat", 30)
            };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (IngredientSO ingredient in _pool)
            {
                Object.DestroyImmediate(ingredient);
            }
        }

        [Test]
        public void Create_AlwaysProducesTwoOrThreeIngredients()
        {
            var generator = new OrderGenerator(_pool);

            for (int i = 0; i < SampleCount; i++)
            {
                int count = generator.Create(startTime: 0f).Requirements.Count;
                Assert.That(count, Is.InRange(2, 3));
            }
        }

        [Test]
        public void Create_OnlyUsesIngredientsFromThePool()
        {
            var generator = new OrderGenerator(_pool);

            for (int i = 0; i < SampleCount; i++)
            {
                foreach (IngredientSO requirement in generator.Create(startTime: 0f).Requirements)
                {
                    Assert.Contains(requirement, _pool);
                }
            }
        }

        [Test]
        public void Create_StampsOrdersWithTheConfiguredGrace()
        {
            var generator = new OrderGenerator(_pool, graceSeconds: 25f);

            Order order = generator.Create(startTime: 0f);

            Assert.AreEqual(25f, order.GraceSeconds);
            Assert.IsTrue(order.IsWithinGrace(now: 24f));
        }

        [Test]
        public void Create_ProducesBothOrderSizes()
        {
            var generator = new OrderGenerator(_pool);
            bool sawTwo = false;
            bool sawThree = false;

            for (int i = 0; i < SampleCount; i++)
            {
                int count = generator.Create(startTime: 0f).Requirements.Count;
                sawTwo |= count == 2;
                sawThree |= count == 3;
            }

            Assert.IsTrue(sawTwo, "Expected some two-ingredient orders.");
            Assert.IsTrue(sawThree, "Expected some three-ingredient orders.");
        }

        [Test]
        public void Constructor_RejectsAnEmptyPool()
        {
            Assert.Throws<System.ArgumentException>(() => new OrderGenerator(new List<IngredientSO>()));
        }

        private static IngredientSO CreateIngredient(string displayName, int scoreValue)
        {
            var ingredient = ScriptableObject.CreateInstance<IngredientSO>();
            var serialized = new SerializedObject(ingredient);

            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_scoreValue").intValue = scoreValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return ingredient;
        }
    }
}
