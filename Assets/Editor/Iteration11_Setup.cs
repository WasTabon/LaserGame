using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration11_Setup
{
    private const string MainMenuPath = "Assets/LaserGame/Scenes/MainMenu.unity";
    private const string DatabasePath = "Assets/LaserGame/Levels/LevelsDatabase.asset";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color BgAccent = new Color(0.078f, 0.078f, 0.156f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color GreenSoft = new Color(0.5f, 1f, 0.5f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);

    [MenuItem("LaserGame/Iteration 11/Update Levels To Harder")]
    public static void UpdateLevelsToHarder()
    {
        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null || db.uniqueConfigs == null || db.uniqueConfigs.Length < 5)
        {
            Debug.LogWarning("[Iteration 11] LevelsDatabase not found or incomplete. Run Iteration 08 setup first.");
            return;
        }

        if (db.uniqueConfigs[1] != null) db.uniqueConfigs[1].definition = BuildLevel2Hard();
        if (db.uniqueConfigs[2] != null) db.uniqueConfigs[2].definition = BuildLevel3Hard();
        if (db.uniqueConfigs[3] != null) db.uniqueConfigs[3].definition = BuildLevel4Hard();
        if (db.uniqueConfigs[4] != null) db.uniqueConfigs[4].definition = BuildLevel5Hard();

        for (int i = 0; i < db.uniqueConfigs.Length; i++)
        {
            if (db.uniqueConfigs[i] != null) EditorUtility.SetDirty(db.uniqueConfigs[i]);
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Iteration 11] Levels 2-5 updated to harder versions.");
    }

    [MenuItem("LaserGame/Iteration 11/Update MainMenu Scene (Shop)")]
    public static void UpdateMainMenuScene()
    {
        if (!File.Exists(MainMenuPath)) { Debug.LogWarning("MainMenu scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogWarning("Canvas not found."); return; }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var shopButton = BuildShopButton(canvasRT);
        var shopPopup = BuildShopPopup(canvasRT);

        var mmCtrl = Object.FindObjectOfType<MainMenuController>();
        if (mmCtrl != null)
        {
            var ext = mmCtrl.GetComponent<MainMenuShopExtension>();
            if (ext == null) ext = mmCtrl.gameObject.AddComponent<MainMenuShopExtension>();
            ext.shopButton = shopButton.GetComponent<Button>();
            ext.shopPopup = shopPopup.GetComponent<ShopPopup>();
            EditorUtility.SetDirty(mmCtrl);
            EditorUtility.SetDirty(ext);
        }
        else
        {
            Debug.LogWarning("MainMenuController not found in MainMenu scene; created shop UI but not wired.");
        }

        shopPopup.transform.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 11] Shop button + ShopPopup added to MainMenu scene.");
    }

    [MenuItem("LaserGame/Iteration 11/Run Both")]
    public static void RunBoth()
    {
        UpdateLevelsToHarder();
        UpdateMainMenuScene();
    }

    [MenuItem("LaserGame/Iteration 11/Grant 500 Coins (Test)")]
    public static void GrantCoins()
    {
        SaveSystem.Load();
        SaveSystem.Data.coins += 500;
        SaveSystem.Save();
        Debug.Log("[Iteration 11] +500 coins. Total: " + SaveSystem.Data.coins);
    }

    private static LevelDefinition BuildLevel2Hard()
    {
        return new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(1, 2), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(1, 4), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(4, 4) },
            walls = new List<Vector2Int> { new Vector2Int(2, 2) },
            energyStars = new List<Vector2Int> { new Vector2Int(3, 4) },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 2
        };
    }

    private static LevelDefinition BuildLevel3Hard()
    {
        return new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(3, 4) },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 2),
                new Vector2Int(3, 3)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 1
        };
    }

    private static LevelDefinition BuildLevel4Hard()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(4, 3), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(2, 3),
                new Vector2Int(4, 1)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 1
        };
    }

    private static LevelDefinition BuildLevel5Hard()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 3), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(3, 6),
                new Vector2Int(5, 5)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 3),
                new Vector2Int(3, 4)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(3, 5), rotationStep = 0 }
            },
            maxMovesForThreeStars = 1
        };
    }

    private static GameObject BuildShopButton(RectTransform parent)
    {
        var btn = FindOrCreateChild(parent, "ShopButton");
        var img = EnsureComponent<Image>(btn);
        img.color = MagentaNeon;
        EnsureComponent<CanvasGroup>(btn);
        var b = EnsureComponent<Button>(btn);
        b.targetGraphic = img;
        b.transition = Selectable.Transition.ColorTint;
        EnsureComponent<ButtonAnimator>(btn);

        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(420, 130);
        rt.anchoredPosition = new Vector2(0, 320);

        var lbl = FindOrCreateChild(rt, "Label");
        var tmp = EnsureComponent<TextMeshProUGUI>(lbl);
        tmp.text = "SHOP";
        tmp.fontSize = 56;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = BgColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 8;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
    }

    private static GameObject BuildShopPopup(RectTransform parent)
    {
        var popup = FindOrCreateChild(parent, "ShopPopup");
        var popupRT = popup.GetComponent<RectTransform>();
        StretchFull(popupRT);
        var popupGroup = EnsureComponent<CanvasGroup>(popup);
        popupGroup.alpha = 0f;
        popupGroup.blocksRaycasts = false;

        var backdrop = FindOrCreateChild(popupRT, "Backdrop");
        var backdropImg = EnsureComponent<Image>(backdrop);
        backdropImg.color = new Color(0, 0, 0, 0.7f);
        StretchFull(backdrop.GetComponent<RectTransform>());
        var backdropBtn = EnsureComponent<Button>(backdrop);
        backdropBtn.transition = Selectable.Transition.None;

        var content = FindOrCreateChild(popupRT, "Content");
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(820, 1280);
        contentRT.anchoredPosition = Vector2.zero;
        var contentImg = EnsureComponent<Image>(content);
        contentImg.color = PanelColor;

        var title = FindOrCreateChild(contentRT, "Title");
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 100);
        titleRT.anchoredPosition = new Vector2(0, -25);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(title);
        titleTMP.text = "SHOP";
        titleTMP.fontSize = 64;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = CyanNeon;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.characterSpacing = 8;

        var coinsRow = FindOrCreateChild(contentRT, "CoinsBlock");
        var coinsRowRT = coinsRow.GetComponent<RectTransform>();
        coinsRowRT.anchorMin = new Vector2(0.5f, 1);
        coinsRowRT.anchorMax = new Vector2(0.5f, 1);
        coinsRowRT.pivot = new Vector2(0.5f, 1);
        coinsRowRT.sizeDelta = new Vector2(360, 80);
        coinsRowRT.anchoredPosition = new Vector2(0, -150);
        var coinsRowImg = EnsureComponent<Image>(coinsRow);
        coinsRowImg.color = BgAccent;

        var coinsIcon = FindOrCreateChild(coinsRowRT, "Icon");
        var coinsIconImg = EnsureComponent<Image>(coinsIcon);
        coinsIconImg.color = YellowNeon;
        var coinsIconRT = coinsIcon.GetComponent<RectTransform>();
        coinsIconRT.anchorMin = new Vector2(0, 0.5f);
        coinsIconRT.anchorMax = new Vector2(0, 0.5f);
        coinsIconRT.pivot = new Vector2(0.5f, 0.5f);
        coinsIconRT.sizeDelta = new Vector2(40, 40);
        coinsIconRT.anchoredPosition = new Vector2(45, 0);

        var coinsTxt = FindOrCreateChild(coinsRowRT, "CoinsText");
        var coinsTxtRT = coinsTxt.GetComponent<RectTransform>();
        coinsTxtRT.anchorMin = new Vector2(0, 0);
        coinsTxtRT.anchorMax = new Vector2(1, 1);
        coinsTxtRT.offsetMin = new Vector2(80, 0);
        coinsTxtRT.offsetMax = new Vector2(-15, 0);
        var coinsTMP = EnsureComponent<TextMeshProUGUI>(coinsTxt);
        coinsTMP.text = "0";
        coinsTMP.fontSize = 40;
        coinsTMP.fontStyle = FontStyles.Bold;
        coinsTMP.alignment = TextAlignmentOptions.MidlineLeft;
        coinsTMP.color = WhiteSoft;

        var hintRow = BuildShopRow(contentRT, "HintRow", "HINT", "Highlight first mirror", "?", CyanNeon, new Vector2(0, -290));
        var undoRow = BuildShopRow(contentRT, "UndoRow", "UNDO", "Revert last move", "U", MagentaNeon, new Vector2(0, -550));
        var skipRow = BuildShopRow(contentRT, "SkipRow", "SKIP", "Auto-win level", ">", GreenSoft, new Vector2(0, -810));

        var closeBtn = CreatePillButton(contentRT, "CloseButton", "CLOSE", MagentaNeon, BgColor, 56);
        var closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.5f, 0);
        closeRT.anchorMax = new Vector2(0.5f, 0);
        closeRT.pivot = new Vector2(0.5f, 0);
        closeRT.sizeDelta = new Vector2(360, 110);
        closeRT.anchoredPosition = new Vector2(0, 50);

        var sp = EnsureComponent<ShopPopup>(popup);
        sp.canvasGroup = popupGroup;
        sp.contentRect = contentRT;
        sp.backdrop = backdropImg;
        sp.coinsText = coinsTMP;
        sp.closeButton = closeBtn.GetComponent<Button>();
        sp.backdropButton = backdropBtn;

        sp.hintCountText = hintRow.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        sp.hintCostText = hintRow.transform.Find("BuyButton/CostText").GetComponent<TextMeshProUGUI>();
        sp.hintBuyButton = hintRow.transform.Find("BuyButton").GetComponent<Button>();

        sp.undoCountText = undoRow.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        sp.undoCostText = undoRow.transform.Find("BuyButton/CostText").GetComponent<TextMeshProUGUI>();
        sp.undoBuyButton = undoRow.transform.Find("BuyButton").GetComponent<Button>();

        sp.skipCountText = skipRow.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        sp.skipCostText = skipRow.transform.Find("BuyButton/CostText").GetComponent<TextMeshProUGUI>();
        sp.skipBuyButton = skipRow.transform.Find("BuyButton").GetComponent<Button>();

        sp.hintCost = 50;
        sp.undoCost = 75;
        sp.skipCost = 200;

        popup.SetActive(false);
        return popup;
    }

    private static GameObject BuildShopRow(RectTransform parent, string name, string title, string desc, string iconText, Color iconColor, Vector2 anchoredPos)
    {
        var row = FindOrCreateChild(parent, name);
        var rowRT = row.GetComponent<RectTransform>();
        rowRT.anchorMin = new Vector2(0, 1);
        rowRT.anchorMax = new Vector2(1, 1);
        rowRT.pivot = new Vector2(0.5f, 1);
        rowRT.sizeDelta = new Vector2(-60, 230);
        rowRT.anchoredPosition = anchoredPos;
        var rowImg = EnsureComponent<Image>(row);
        rowImg.color = BgAccent;

        var icon = FindOrCreateChild(rowRT, "Icon");
        var iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.5f);
        iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.sizeDelta = new Vector2(140, 140);
        iconRT.anchoredPosition = new Vector2(25, 0);
        var iconImg = EnsureComponent<Image>(icon);
        iconImg.color = new Color(iconColor.r, iconColor.g, iconColor.b, 0.18f);

        var iconLetter = FindOrCreateChild(iconRT, "Letter");
        var iconLetterRT = iconLetter.GetComponent<RectTransform>();
        StretchFull(iconLetterRT);
        var iconTMP = EnsureComponent<TextMeshProUGUI>(iconLetter);
        iconTMP.text = iconText;
        iconTMP.fontSize = 90;
        iconTMP.fontStyle = FontStyles.Bold;
        iconTMP.color = iconColor;
        iconTMP.alignment = TextAlignmentOptions.Center;
        iconTMP.raycastTarget = false;

        var titleGo = FindOrCreateChild(rowRT, "Title");
        var titleRT = titleGo.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.5f);
        titleRT.anchorMax = new Vector2(0, 0.5f);
        titleRT.pivot = new Vector2(0, 0.5f);
        titleRT.sizeDelta = new Vector2(380, 60);
        titleRT.anchoredPosition = new Vector2(195, 30);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(titleGo);
        titleTMP.text = title;
        titleTMP.fontSize = 44;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = WhiteSoft;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
        titleTMP.characterSpacing = 4;

        var descGo = FindOrCreateChild(rowRT, "Desc");
        var descRT = descGo.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0.5f);
        descRT.anchorMax = new Vector2(0, 0.5f);
        descRT.pivot = new Vector2(0, 0.5f);
        descRT.sizeDelta = new Vector2(380, 50);
        descRT.anchoredPosition = new Vector2(195, -20);
        var descTMP = EnsureComponent<TextMeshProUGUI>(descGo);
        descTMP.text = desc;
        descTMP.fontSize = 28;
        descTMP.color = new Color(WhiteSoft.r, WhiteSoft.g, WhiteSoft.b, 0.65f);
        descTMP.alignment = TextAlignmentOptions.MidlineLeft;

        var countGo = FindOrCreateChild(rowRT, "CountText");
        var countRT = countGo.GetComponent<RectTransform>();
        countRT.anchorMin = new Vector2(0, 0.5f);
        countRT.anchorMax = new Vector2(0, 0.5f);
        countRT.pivot = new Vector2(0, 0.5f);
        countRT.sizeDelta = new Vector2(380, 50);
        countRT.anchoredPosition = new Vector2(195, -75);
        var countTMP = EnsureComponent<TextMeshProUGUI>(countGo);
        countTMP.text = "x0";
        countTMP.fontSize = 32;
        countTMP.fontStyle = FontStyles.Bold;
        countTMP.color = YellowNeon;
        countTMP.alignment = TextAlignmentOptions.MidlineLeft;

        var buyBtn = FindOrCreateChild(rowRT, "BuyButton");
        var buyImg = EnsureComponent<Image>(buyBtn);
        buyImg.color = CyanNeon;
        EnsureComponent<CanvasGroup>(buyBtn);
        var b = EnsureComponent<Button>(buyBtn);
        b.targetGraphic = buyImg;
        b.transition = Selectable.Transition.ColorTint;
        EnsureComponent<ButtonAnimator>(buyBtn);
        var buyRT = buyBtn.GetComponent<RectTransform>();
        buyRT.anchorMin = new Vector2(1, 0.5f);
        buyRT.anchorMax = new Vector2(1, 0.5f);
        buyRT.pivot = new Vector2(1, 0.5f);
        buyRT.sizeDelta = new Vector2(220, 130);
        buyRT.anchoredPosition = new Vector2(-25, 0);

        var costTxt = FindOrCreateChild(buyRT, "CostText");
        var costRT = costTxt.GetComponent<RectTransform>();
        StretchFull(costRT);
        var costTMP = EnsureComponent<TextMeshProUGUI>(costTxt);
        costTMP.text = "0";
        costTMP.fontSize = 44;
        costTMP.fontStyle = FontStyles.Bold;
        costTMP.color = BgColor;
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.raycastTarget = false;

        return row;
    }

    private static GameObject CreatePillButton(RectTransform parent, string name, string label, Color fill, Color textColor, int fontSize)
    {
        var btn = FindOrCreateChild(parent, name);
        var img = EnsureComponent<Image>(btn);
        img.color = fill;
        EnsureComponent<CanvasGroup>(btn);
        var b = EnsureComponent<Button>(btn);
        b.targetGraphic = img;
        b.transition = Selectable.Transition.ColorTint;
        EnsureComponent<ButtonAnimator>(btn);

        var lbl = FindOrCreateChild(btn.GetComponent<RectTransform>(), "Label");
        var tmp = EnsureComponent<TextMeshProUGUI>(lbl);
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 6;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var c = parent.GetChild(i);
            if (c.name == name) return c.gameObject;
        }
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject FindOrCreateChild(RectTransform parent, string name)
    {
        return FindOrCreateChild((Transform)parent, name);
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }
}
