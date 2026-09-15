using UnityEngine;

namespace YesChef.Scoring
{
    /// <summary>
    /// Tracks the run of consecutive on-time deliveries and turns it into a multiplier.
    /// <para>
    /// Plain C# for the same reason as <see cref="Orders.Order"/>: the exact moment the
    /// multiplier steps up or resets is easy to get subtly wrong, so it is unit-tested.
    /// </para>
    /// <para>
    /// The multiplier a delivery earns is the one in effect <em>before</em> it lands: the
    /// first on-time order in a streak pays x1, the second x2, the third x3 and so on up to
    /// <see cref="MaxMultiplier"/>. One late order drops the streak straight back to zero,
    /// and that late order is paid at x1 so a negative total is never multiplied.
    /// </para>
    /// </summary>
    public class ComboTracker
    {
        public const int MinMultiplier = 1;

        public ComboTracker(int maxMultiplier)
        {
            MaxMultiplier = Mathf.Max(MinMultiplier, maxMultiplier);
        }

        public int MaxMultiplier { get; }

        /// <summary>Consecutive on-time deliveries since the last reset.</summary>
        public int Streak { get; private set; }

        /// <summary>Multiplier the next on-time delivery will receive.</summary>
        public int Multiplier => Mathf.Min(MinMultiplier + Streak, MaxMultiplier);

        /// <summary>True once the streak is long enough to be paying out the cap.</summary>
        public bool IsMaxed => Multiplier >= MaxMultiplier;

        /// <summary>Settles one order and advances or breaks the streak accordingly.</summary>
        public ScoreAward Award(int rawPoints, bool onTime)
        {
            if (!onTime)
            {
                Streak = 0;
                return new ScoreAward(rawPoints, MinMultiplier, onTime: false);
            }

            var award = new ScoreAward(rawPoints, Multiplier, onTime: true);
            Streak++;
            return award;
        }

        public void Reset() => Streak = 0;
    }
}
