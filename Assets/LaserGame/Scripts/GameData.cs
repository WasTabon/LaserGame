using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int coins = 0;
    public int unlockedLevel = 1;
    public List<LevelProgress> levelProgress = new List<LevelProgress>();
    public bool soundEnabled = true;
    public bool musicEnabled = true;
    public bool hapticsEnabled = true;

    public int GetStarsForLevel(int levelIndex)
    {
        foreach (var p in levelProgress)
        {
            if (p.levelIndex == levelIndex) return p.stars;
        }
        return 0;
    }

    public void SetStarsForLevel(int levelIndex, int stars)
    {
        foreach (var p in levelProgress)
        {
            if (p.levelIndex == levelIndex)
            {
                if (stars > p.stars) p.stars = stars;
                return;
            }
        }
        levelProgress.Add(new LevelProgress { levelIndex = levelIndex, stars = stars });
    }
}

[Serializable]
public class LevelProgress
{
    public int levelIndex;
    public int stars;
}
