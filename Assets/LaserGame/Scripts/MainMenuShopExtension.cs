using UnityEngine;
using UnityEngine.UI;

public class MainMenuShopExtension : MonoBehaviour
{
    public Button shopButton;
    public ShopPopup shopPopup;

    private void OnEnable()
    {
        if (shopButton != null)
        {
            shopButton.onClick.RemoveListener(OnShopClicked);
            shopButton.onClick.AddListener(OnShopClicked);
        }
    }

    private void OnDisable()
    {
        if (shopButton != null)
        {
            shopButton.onClick.RemoveListener(OnShopClicked);
        }
    }

    private void OnShopClicked()
    {
        if (shopPopup == null) return;
        shopPopup.transform.SetAsLastSibling();
        shopPopup.Open();
    }
}
