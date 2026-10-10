using UnityEngine;

public class WalletHUDAdapter : MonoBehaviour
{
    private WalletHUDView view;

    private void Awake()
    {
        view = GetComponent<WalletHUDView>();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<WalletBalanceEvent>(OnBalanceChanged);
        Refresh();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<WalletBalanceEvent>(OnBalanceChanged);
    }

    private void OnBalanceChanged(WalletBalanceEvent message)
    {
        if (view != null)
            view.SetBalance(message.Balance);
    }

    private void Refresh()
    {
        if (view == null)
            return;

        if (GameEvents.TryGetLatest<WalletBalanceEvent>(out var message))
            view.SetBalance(message.Balance);
    }
}