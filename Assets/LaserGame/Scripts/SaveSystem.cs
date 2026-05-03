using UnityEngine;

public static class SaveSystem
{
    private const string SaveKey = "LaserGame_Save";
    private static GameData _cached;

    public static GameData Data
    {
        get
        {
            if (_cached == null) Load();
            return _cached;
        }
    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            _cached = JsonUtility.FromJson<GameData>(json);
            if (_cached == null) _cached = new GameData();
        }
        else
        {
            _cached = new GameData();
        }
    }

    public static void Save()
    {
        if (_cached == null) _cached = new GameData();
        string json = JsonUtility.ToJson(_cached);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        _cached = new GameData();
        Save();
    }
}
