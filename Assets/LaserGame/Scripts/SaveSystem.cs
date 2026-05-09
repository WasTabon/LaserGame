using UnityEngine;

public static class SaveSystem
{
    private const string Key = "lasergame_save_v1";
    private static GameData _data;

    public static GameData Data
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    public static void Load()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json))
        {
            _data = new GameData();
            return;
        }
        try
        {
            _data = JsonUtility.FromJson<GameData>(json);
            if (_data == null) _data = new GameData();
            if (_data.starsPerLevel == null || _data.starsPerLevel.Length != 30)
            {
                var newArr = new int[30];
                if (_data.starsPerLevel != null)
                {
                    for (int i = 0; i < Mathf.Min(_data.starsPerLevel.Length, 30); i++) newArr[i] = _data.starsPerLevel[i];
                }
                _data.starsPerLevel = newArr;
            }
        }
        catch
        {
            _data = new GameData();
        }
    }

    public static void Save()
    {
        if (_data == null) return;
        string json = JsonUtility.ToJson(_data);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }
}
