using NUnit.Framework;
using YesChef.Scoring;

namespace YesChef.Tests
{
    /// <summary>
    /// Locks down the combo rules: the multiplier a delivery earns is the one in effect
    /// before it lands, it climbs one step per on-time order up to the cap, and a single
    /// late order breaks the streak without ever multiplying a penalty.
    /// </summary>
    public class ComboTrackerTests
    {
        [Test]
        public void StartsAtMultiplierOneWithNoStreak()
        {
            var combo = new ComboTracker(maxMultiplier: 3);

            Assert.AreEqual(0, combo.Streak);
            Assert.AreEqual(1, combo.Multiplier);
            Assert.IsFalse(combo.IsMaxed);
        }

        [Test]
        public void FirstOnTimeDelivery_PaysBaseValueAndStartsTheStreak()
        {
            var combo = new ComboTracker(maxMultiplier: 3);

            ScoreAward award = combo.Award(rawPoints: 40, onTime: true);

            Assert.AreEqual(40, award.RawPoints);
            Assert.AreEqual(1, award.Multiplier);
            Assert.AreEqual(40, award.Points);
            Assert.IsTrue(award.OnTime);
            Assert.AreEqual(1, combo.Streak);
            Assert.AreEqual(2, combo.Multiplier);
        }

        [Test]
        public void ConsecutiveOnTimeDeliveries_StepTheMultiplierUp()
        {
            var combo = new ComboTracker(maxMultiplier: 3);

            combo.Award(40, onTime: true);
            ScoreAward second = combo.Award(40, onTime: true);
            ScoreAward third = combo.Award(40, onTime: true);

            Assert.AreEqual(2, second.Multiplier);
            Assert.AreEqual(80, second.Points);
            Assert.AreEqual(3, third.Multiplier);
            Assert.AreEqual(120, third.Points);
        }

        [Test]
        public void Multiplier_NeverExceedsTheCap()
        {
            var combo = new ComboTracker(maxMultiplier: 3);

            for (int i = 0; i < 10; i++)
            {
                combo.Award(10, onTime: true);
            }

            Assert.AreEqual(3, combo.Multiplier);
            Assert.IsTrue(combo.IsMaxed);
            Assert.AreEqual(10, combo.Streak, "The streak keeps counting even once the multiplier is capped.");
            Assert.AreEqual(30, combo.Award(10, onTime: true).Points);
        }

        [Test]
        public void LateDelivery_ResetsTheStreakAndPaysUnmultiplied()
        {
            var combo = new ComboTracker(maxMultiplier: 3);
            combo.Award(10, onTime: true);
            combo.Award(10, onTime: true);
            combo.Award(10, onTime: true);

            ScoreAward late = combo.Award(rawPoints: 25, onTime: false);

            Assert.AreEqual(1, late.Multiplier);
            Assert.AreEqual(25, late.Points);
            Assert.IsFalse(late.OnTime);
            Assert.AreEqual(0, combo.Streak);
            Assert.AreEqual(1, combo.Multiplier);
        }

        [Test]
        public void LateDelivery_NeverMultipliesANegativeScore()
        {
            var combo = new ComboTracker(maxMultiplier: 3);
            combo.Award(10, onTime: true);
            combo.Award(10, onTime: true);

            ScoreAward late = combo.Award(rawPoints: -6, onTime: false);

            Assert.AreEqual(-6, late.Points);
        }

        [Test]
        public void StreakRebuildsFromScratchAfterABreak()
        {
            var combo = new ComboTracker(maxMultiplier: 3);
            combo.Award(10, onTime: true);
            combo.Award(10, onTime: true);
            combo.Award(10, onTime: false);

            ScoreAward next = combo.Award(10, onTime: true);

            Assert.AreEqual(1, next.Multiplier);
            Assert.AreEqual(2, combo.Multiplier);
        }

        [Test]
        public void Reset_ClearsTheStreak()
        {
            var combo = new ComboTracker(maxMultiplier: 3);
            combo.Award(10, onTime: true);
            combo.Award(10, onTime: true);

            combo.Reset();

            Assert.AreEqual(0, combo.Streak);
            Assert.AreEqual(1, combo.Multiplier);
        }

        [Test]
        public void Constructor_ClampsCapToAtLeastOne()
        {
            var combo = new ComboTracker(maxMultiplier: 0);

            combo.Award(10, onTime: true);

            Assert.AreEqual(1, combo.MaxMultiplier);
            Assert.AreEqual(1, combo.Multiplier);
        }
    }
}
