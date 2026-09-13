using System;
using NUnit.Framework;
using YesChef.Stations;

namespace YesChef.Tests
{
    /// <summary>The countdown shared by chopping tables and stove burners.</summary>
    public class PreparationTimerTests
    {
        [Test]
        public void Constructor_RejectsNonPositiveDuration()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PreparationTimer(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PreparationTimer(-1f));
        }

        [Test]
        public void Idle_ReportsNoProgress()
        {
            var timer = new PreparationTimer(2f);

            Assert.IsFalse(timer.IsRunning);
            Assert.AreEqual(0f, timer.Progress);
            Assert.IsFalse(timer.Tick(1f), "Ticking an idle timer must not complete it.");
        }

        [Test]
        public void Tick_AdvancesProgressProportionally()
        {
            var timer = new PreparationTimer(2f);
            timer.Start();

            Assert.IsFalse(timer.Tick(0.5f));
            Assert.AreEqual(0.25f, timer.Progress, 0.0001f);

            Assert.IsFalse(timer.Tick(0.5f));
            Assert.AreEqual(0.5f, timer.Progress, 0.0001f);
        }

        [Test]
        public void Tick_CompletesExactlyOnce()
        {
            var timer = new PreparationTimer(2f);
            timer.Start();

            Assert.IsFalse(timer.Tick(1.5f));
            Assert.IsTrue(timer.Tick(1f), "Crossing the duration must report completion.");
            Assert.IsFalse(timer.IsRunning);
            Assert.AreEqual(0f, timer.Progress, "Progress resets so the bar hides.");
            Assert.IsFalse(timer.Tick(1f), "A finished timer must not complete again.");
        }

        [Test]
        public void Cancel_StopsAndClearsProgress()
        {
            var timer = new PreparationTimer(2f);
            timer.Start();
            timer.Tick(1f);

            timer.Cancel();

            Assert.IsFalse(timer.IsRunning);
            Assert.AreEqual(0f, timer.Progress);
        }

        [Test]
        public void Cancel_OnIdleTimer_DoesNotNotify()
        {
            var timer = new PreparationTimer(2f);
            int notifications = 0;
            timer.OnChanged += () => notifications++;

            timer.Cancel();

            Assert.AreEqual(0, notifications);
        }

        [Test]
        public void OnChanged_FiresForStartTickAndCompletion()
        {
            var timer = new PreparationTimer(1f);
            int notifications = 0;
            timer.OnChanged += () => notifications++;

            timer.Start();
            timer.Tick(0.5f);
            timer.Tick(0.5f);

            Assert.AreEqual(3, notifications);
        }

        [Test]
        public void Tick_IgnoresNegativeDeltaTime()
        {
            var timer = new PreparationTimer(2f);
            timer.Start();
            timer.Tick(1f);

            timer.Tick(-5f);

            Assert.AreEqual(0.5f, timer.Progress, 0.0001f);
        }

        [Test]
        public void Start_RestartsAJobInProgress()
        {
            var timer = new PreparationTimer(2f);
            timer.Start();
            timer.Tick(1.5f);

            timer.Start();

            Assert.IsTrue(timer.IsRunning);
            Assert.AreEqual(0f, timer.Progress);
        }
    }
}
