namespace YesChef.Scoring
{
    /// <summary>
    /// The settled result of one order: what it was worth on its own, what multiplier the
    /// combo applied, and the points that actually landed on the scoreboard.
    /// </summary>
    public readonly struct ScoreAward
    {
        public ScoreAward(int rawPoints, int multiplier, bool onTime)
        {
            RawPoints = rawPoints;
            Multiplier = multiplier;
            OnTime = onTime;
        }

        /// <summary>Order value after the time penalty, before the combo multiplier.</summary>
        public int RawPoints { get; }

        /// <summary>Combo multiplier that was applied. Always 1 for a late delivery.</summary>
        public int Multiplier { get; }

        /// <summary>True when the order was handed over inside its grace period.</summary>
        public bool OnTime { get; }

        /// <summary>Points added to the score.</summary>
        public int Points => RawPoints * Multiplier;
    }
}
