using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : PopupBase
{
    public Button closeButton;
    public Button backdropButton;
    public TextMeshProUGUI coinsText;

    public Button hintBuyButton;
    public TextMeshProUGUI hintCountText;
    public TextMeshProUGUI hintCostText;

    public Button undoBuyButton;
    public TextMeshProUGUI undoCountText;
    public TextMeshProUGUI undoCostText;

    public Button skipBuyButton;
    public TextMeshProUGUI skipCountText;
    public TextMeshProUGUI skipCostText;

    public int hintCost = 50;
    public int undoCost = 75;
    public int skipCost = 200;

    private void OnEnable()
    {
        Wire(closeButton, HandleClose);
        Wire(backdropButton, HandleClose);
        Wire(hintBuyButton, HandleHintBuy);
        Wire(undoBuyButton, HandleUndoBuy);
        Wire(skipBuyButton, HandleSkipBuy);
        Refresh();
    }

    private void OnDisable()
    {
        Unwire(closeButton, HandleClose);
        Unwire(backdropButton, HandleClose);
        Unwire(hintBuyButton, HandleHintBuy);
        Unwire(undoBuyButton, HandleUndoBuy);
        Unwire(skipBuyButton, HandleSkipBuy);
    }

    private void Wire(Button b, UnityEngine.Events.UnityAction action)
    {
        if (b == null) return;
        b.onClick.RemoveListener(action);
        b.onClick.AddListener(action);
    }

    private void Unwire(Button b, UnityEngine.Events.UnityAction action)
    {
        if (b == null) return;
        b.onClick.RemoveListener(action);
    }

    public void Refresh()
    {
        if (coinsText != null) coinsText.text = SaveSystem.Data.coins.ToString();
        SetRowState(hintCountText, hintCostText, hintBuyButton, SaveSystem.Data.hintCount, hintCost);
        SetRowState(undoCountText, undoCostText, undoBuyButton, SaveSystem.Data.undoCount, undoCost);
        SetRowState(skipCountText, skipCostText, skipBuyButton, SaveSystem.Data.skipCount, skipCost);
    }

    private void SetRowState(TextMeshProUGUI count, TextMeshProUGUI cost, Button buy, int owned, int costValue)
    {
        if (count != null) count.text = "x" + owned;
        if (cost != null) cost.text = costValue.ToString();
        if (buy != null)
        {
            bool affordable = SaveSystem.Data.coins >= costValue;
            buy.interactable = affordable;
            var img = buy.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = affordable ? 1f : 0.4f;
                img.color = c;
            }
        }
    }

    private void HandleHintBuy()
    {
        TryBuy(hintCost, BoostKind.Hint);
    }

    private void HandleUndoBuy()
    {
        TryBuy(undoCost, BoostKind.Undo);
    }

    private void HandleSkipBuy()
    {
        TryBuy(skipCost, BoostKind.Skip);
    }

    private enum BoostKind { Hint, Undo, Skip }

    private void TryBuy(int cost, BoostKind kind)
    {
        if (SaveSystem.Data.coins < cost)
        {
            ShakeContent();
            HapticManager.Trigger(HapticManager.HapticType.Heavy);
            return;
        }
        SaveSystem.Data.coins -= cost;
        switch (kind)
        {
            case BoostKind.Hint: SaveSystem.Data.hintCount++; break;
            case BoostKind.Undo: SaveSystem.Data.undoCount++; break;
            case BoostKind.Skip: SaveSystem.Data.skipCount++; break;
        }
        SaveSystem.Save();
        Refresh();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        HapticManager.Trigger(HapticManager.HapticType.Light);

        if (coinsText != null)
        {
            coinsText.rectTransform.DOKill();
            coinsText.rectTransform.localScale = Vector3.one;
            coinsText.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 0.5f);
        }
    }

    private void HandleClose()
    {
        Close();
    }

    private void ShakeContent()
    {
        if (contentRect != null)
        {
            contentRect.DOKill(false);
            contentRect.DOShakeAnchorPos(0.3f, 15f, 12, 90f, false, true);
        }
    }
}
