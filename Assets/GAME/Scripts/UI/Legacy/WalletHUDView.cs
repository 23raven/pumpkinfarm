using TMPro;
using UnityEngine;

public class WalletHUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI balanceText;

    public void SetBalance(int balance)
    {
        if (balanceText == null)
            return;

        balanceText.text = $"Money: {balance}";
    }
}