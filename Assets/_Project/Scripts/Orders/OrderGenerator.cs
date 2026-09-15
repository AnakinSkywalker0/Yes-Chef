using System;
using System.Collections.Generic;
using YesChef.Ingredients;
using Random = UnityEngine.Random;

namespace YesChef.Orders
{
    /// <summary>
    /// Builds random orders: an even split between two and three ingredients, each drawn
    /// uniformly from the pool so duplicates (meat, meat, meat) are possible.
    /// </summary>
    public class OrderGenerator
    {
        private const float SmallOrderChance = 0.5f;
        private const int SmallOrderSize = 2;
        private const int LargeOrderSize = 3;

        private readonly IReadOnlyList<IngredientSO> _pool;
        private readonly float _graceSeconds;

        public OrderGenerator(IReadOnlyList<IngredientSO> pool, float graceSeconds = 0f)
        {
            if (pool == null || pool.Count == 0)
            {
                throw new ArgumentException("The order generator needs a non-empty ingredient pool.", nameof(pool));
            }

            _pool = pool;
            _graceSeconds = graceSeconds;
        }

        public Order Create(float startTime)
        {
            int size = Random.value < SmallOrderChance ? SmallOrderSize : LargeOrderSize;
            var requirements = new IngredientSO[size];

            for (int i = 0; i < size; i++)
            {
                requirements[i] = _pool[Random.Range(0, _pool.Count)];
            }

            return new Order(requirements, startTime, _graceSeconds);
        }
    }
}
