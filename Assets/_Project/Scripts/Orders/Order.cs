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

        public Order(IReadOnlyList<IngredientSO> requirements, float startTime, float graceSeconds = 0f)
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
            GraceSeconds = Mathf.Max(0f, graceSeconds);
            BaseScore = total;
        }

        /// <summary>Scaled game time at which this order appeared.</summary>
        public float StartTime { get; }

        /// <summary>
        /// Seconds the customer waits patiently before points start dropping. Delivering
        /// inside this window is what keeps a combo streak alive.
        /// </summary>
        public float GraceSeconds { get; }

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

        /// <summary>Seconds of grace left before the penalty starts. Zero once it has begun.</summary>
        public float GetGraceRemaining(float now) => Mathf.Max(0f, GraceSeconds - GetElapsedSeconds(now));

        /// <summary>True while the order is still worth its full base score.</summary>
        public bool IsWithinGrace(float now) => GetElapsedSeconds(now) < GraceSeconds;

        /// <summary>
        /// Ingredient values minus one point per whole second the order has been open beyond
        /// its grace period. Time is floored, so 14.99s of penalty only costs 14 points.
        /// May go negative.
        /// </summary>
        public int CalculateScore(float now)
        {
            float overdue = Mathf.Max(0f, GetElapsedSeconds(now) - GraceSeconds);
            return BaseScore - Mathf.FloorToInt(overdue);
        }

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
