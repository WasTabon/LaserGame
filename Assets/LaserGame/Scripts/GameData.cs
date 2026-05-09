using System;
using UnityEngine;

[Serializable]
public class GameData
{
    public int unlockedLevel = 1;
    public int coins = 0;
    public int[] starsPerLevel = new int[30];
    public int hintCount = 0;
    public int undoCount = 0;
    public int skipCount = 0;
    public bool soundEnabled = true;
    public bool musicEnabled = true;
    public bool hapticsEnabled = true;

    public int GetStarsForLevel(int levelIndex)
    {
        if (starsPerLevel == null || levelIndex < 1 || levelIndex > starsPerLevel.Length) return 0;
        return starsPerLevel[levelIndex - 1];
    }

    public void SetStarsForLevel(int levelIndex, int stars)
    {
        if (starsPerLevel == null || levelIndex < 1 || levelIndex > starsPerLevel.Length) return;
        if (stars > starsPerLevel[levelIndex - 1]) starsPerLevel[levelIndex - 1] = stars;
    }
}
