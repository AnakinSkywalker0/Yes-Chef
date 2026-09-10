using System;

namespace YesChef.Stations
{
    /// <summary>
    /// Implemented by anything that runs a timed job the player should be able to see -
    /// chopping a vegetable, cooking a piece of meat.
    /// <para>
    /// Keeping this an interface means <see cref="YesChef.UI.WorldProgressBarUI"/> knows
    /// nothing about stoves or tables; any future station gets a progress bar for free.
    /// </para>
    /// </summary>
    public interface IHasProgress
    {
        /// <summary>Raised whenever <see cref="IsInProgress"/> or <see cref="Progress"/> changes.</summary>
        event Action OnProgressChanged;

        /// <summary>True while a timed job is running.</summary>
        bool IsInProgress { get; }

        /// <summary>Normalised job completion in the range [0, 1].</summary>
        float Progress { get; }
    }
}
