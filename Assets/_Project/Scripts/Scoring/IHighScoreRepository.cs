namespace YesChef.Scoring
{
    /// <summary>
    /// Persistence seam for the high score. <see cref="ScoreManager"/> depends on this
    /// rather than on PlayerPrefs directly, so swapping in a save file or a cloud backend
    /// later is a one-line change.
    /// </summary>
    public interface IHighScoreRepository
    {
        int Load();

        void Save(int highScore);
    }
}
