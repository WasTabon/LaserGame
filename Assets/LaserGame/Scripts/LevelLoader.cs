using UnityEngine;

public static class LevelLoader
{
    public static LevelDefinition LoadForLevel(int levelIndex, LevelsDatabaseSO database, LevelDefinition fallback)
    {
        if (database != null)
        {
            var cfg = database.GetConfigForLevel(levelIndex);
            if (cfg != null && cfg.definition != null)
            {
                return cfg.definition;
            }
        }
        return fallback;
    }
}
