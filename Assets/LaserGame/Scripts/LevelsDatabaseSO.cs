using UnityEngine;

[CreateAssetMenu(fileName = "LevelsDatabase", menuName = "LaserGame/Levels Database", order = 1)]
public class LevelsDatabaseSO : ScriptableObject
{
    public LevelConfigSO[] uniqueConfigs = new LevelConfigSO[5];
    public int[] levelToConfigMapping = new int[30];
    public int totalLevels = 30;

    public LevelConfigSO GetConfigForLevel(int levelIndex)
    {
        if (uniqueConfigs == null || uniqueConfigs.Length == 0) return null;
        if (levelIndex < 1 || levelIndex > totalLevels) return null;
        if (levelToConfigMapping == null || levelToConfigMapping.Length < levelIndex)
        {
            int fallback = (levelIndex - 1) % uniqueConfigs.Length;
            return uniqueConfigs[fallback];
        }
        int cfgIdx = levelToConfigMapping[levelIndex - 1];
        if (cfgIdx < 0 || cfgIdx >= uniqueConfigs.Length) return null;
        return uniqueConfigs[cfgIdx];
    }
}
