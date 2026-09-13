using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YesChef.Ingredients;
using YesChef.Stations;

namespace YesChef.Tests
{
    /// <summary>The single place an ingredient moves: <see cref="Ingredient.SetHolder"/>.</summary>
    public class IngredientTests : KitchenTestBase
    {
        [Test]
        public void SetHolder_MovesBetweenHolders()
        {
            var table = CreateComponent<ChoppingStation>("Table");
            Ingredient vegetable = CreateIngredient(Vegetable, Player);

            vegetable.SetHolder(table);

            Assert.IsFalse(Player.HasIngredient, "Previous holder must be released.");
            Assert.AreSame(vegetable, table.HeldIngredient);
            Assert.AreSame(table, vegetable.Holder);
        }

        [Test]
        public void SetHolder_RefusesAnOccupiedHolder()
        {
            var table = CreateComponent<ChoppingStation>("Table");
            Ingredient first = CreateIngredient(Vegetable, table);
            Ingredient second = CreateIngredient(Cheese, Player);

            LogAssert.Expect(LogType.Error, new Regex("already occupied"));
            second.SetHolder(table);

            Assert.AreSame(first, table.HeldIngredient, "Occupant must be untouched.");
            Assert.AreSame(second, Player.HeldIngredient, "Mover must stay where it was.");
        }

        [Test]
        public void DestroySelf_ReleasesTheHolder()
        {
            Ingredient cheese = CreateIngredient(Cheese, Player);

            cheese.DestroySelf();

            Assert.IsFalse(Player.HasIngredient);
        }

        [Test]
        public void IsReadyForDelivery_FollowsPreparationRules()
        {
            Assert.IsTrue(CreateIngredient(Cheese, null).IsReadyForDelivery, "Cheese needs no preparation.");
            Assert.IsFalse(CreateIngredient(Vegetable, null).IsReadyForDelivery, "Raw vegetables are not servable.");
            Assert.IsTrue(CreateIngredient(Vegetable, null, prepared: true).IsReadyForDelivery);
            Assert.IsFalse(CreateIngredient(Meat, null).IsReadyForDelivery, "Raw meat is not servable.");
        }

        [Test]
        public void MarkPrepared_RaisesOnChangedOnce()
        {
            Ingredient meat = CreateIngredient(Meat, null);
            int changes = 0;
            meat.OnChanged += () => changes++;

            meat.MarkPrepared();
            meat.MarkPrepared();

            Assert.AreEqual(1, changes);
            Assert.AreEqual(IngredientState.Prepared, meat.State);
        }
    }
}
