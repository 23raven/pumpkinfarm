using UnityEngine;

public class WalletService : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int startingBalance = 100;

    private int balance;

    public int Balance => balance;

    private void Awake()
    {
        balance = startingBalance;
    }

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;

        if (services == null)
        {
            Debug.LogError(
                "GameServices is missing from the scene.",
                this
            );

            return;
        }

        if (services.Register(this))
            PublishBalance();
    }

    private void OnDisable()
    {
        GameServices services = GameServices.Instance;

        if (services != null)
            services.Unregister(this);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        balance += amount;
        PublishBalance();
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || balance < amount)
            return false;

        balance -= amount;
        PublishBalance();

        return true;
    }

    private void PublishBalance()
    {
        GameEvents.Publish(new WalletBalanceEvent(balance));
    }
}