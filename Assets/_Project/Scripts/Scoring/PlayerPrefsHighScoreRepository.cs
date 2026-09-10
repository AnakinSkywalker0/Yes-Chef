using UnityEngine;

namespace YesChef.Scoring
{
    /// <summary>PlayerPrefs-backed high score storage. Persists between game sessions.</summary>
    public class PlayerPrefsHighScoreRepository : IHighScoreRepository
    {
        private const string HighScoreKey = "YesChef.HighScore";

        public int Load() => PlayerPrefs.GetInt(HighScoreKey, 0);

        public void Save(int highScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }
}
