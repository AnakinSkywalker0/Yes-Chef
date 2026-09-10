using System;
using System.Collections.Generic;
using UnityEngine;
using YesChef.Ingredients;

namespace YesChef.Orders
{
    /// <summary>
    /// A single customer order and its scoring rules.
    /// <para>
    /// Deliberately a plain C# class with no Unity dependency beyond <see cref="Mathf"/>:
    /// the scoring rule is the most fiddly part of the design, and keeping it out of a
    /// MonoBehaviour makes it directly unit-testable.
    /// </para>
    /// </summary>
    public class Order
    {
        private readonly IngredientSO[] _requirements;
        private readonly bool[] _delivered;

        public Order(IReadOnlyList<IngredientSO> requirements, float startTime)
        {
            if (requirements == null || requirements.Count == 0)
            {
                throw new ArgumentException("An order needs at least one ingredient.", nameof(requirements));
            }

            _requirements = new IngredientSO[requirements.Count];
            int total = 0;

            for (int i = 0; i < requirements.Count; i++)
            {
                _requirements[i] = requirements[i];
                total += requirements[i].ScoreValue;
            }

            _delivered = new bool[_requirements.Length];
            StartTime = startTime;
            BaseScore = total;
        }

        /// <summary>Scaled game time at which this order appeared.</summary>
        public float StartTime { get; }

        /// <summary>Sum of the ingredient values, before the time penalty.</summary>
        public int BaseScore { get; }

        public IReadOnlyList<IngredientSO> Requirements => _requirements;

        public bool IsComplete { get; private set; }

        public bool IsDelivered(int index) => _delivered[index];

        /// <summary>
        /// Consumes one outstanding requirement matching <paramref name="ingredient"/>.
        /// Duplicate ingredients are handled naturally: each slot is ticked off separately.
        /// </summary>
        public bool TryDeliver(IngredientSO ingredient)
        {
            if (ingredient == null || IsComplete)
            {
                return false;
            }

            for (int i = 0; i < _requirements.Length; i++)
            {
                if (_delivered[i] || _requirements[i] != ingredient)
                {
                    continue;
                }

                _delivered[i] = true;
                IsComplete = AllDelivered();
                return true;
            }

            return false;
        }

        public float GetElapsedSeconds(float now) => Mathf.Max(0f, now - StartTime);

        /// <summary>
        /// Ingredient values minus one point per whole second the order has been open.
        /// Time is floored, so 14.99s of service only costs 14 points. May go negative.
        /// </summary>
        public int CalculateScore(float now) => BaseScore - Mathf.FloorToInt(GetElapsedSeconds(now));

        private bool AllDelivered()
        {
            for (int i = 0; i < _delivered.Length; i++)
            {
                if (!_delivered[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
