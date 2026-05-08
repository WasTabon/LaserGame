using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LaserGame/Level Config", order = 0)]
public class LevelConfigSO : ScriptableObject
{
    public LevelDefinition definition = new LevelDefinition();
}
