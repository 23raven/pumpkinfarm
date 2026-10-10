using System.Collections.Generic;
using UnityEngine;

public class HUDPresenter : MonoBehaviour
{
    private readonly Dictionary<HUDTextKey, HUDTextBinding> bindings =
        new Dictionary<HUDTextKey, HUDTextBinding>();

    private void Awake()
    {
        CacheBindings();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<HUDTextUpdateEvent>(OnTextUpdated);

        // Ask the bridge to resend the latest display values.
        GameEvents.Publish(new HUDRefreshRequestedEvent());
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<HUDTextUpdateEvent>(OnTextUpdated);
    }

    private void CacheBindings()
    {
        bindings.Clear();

        HUDTextBinding[] found =
            GetComponentsInChildren<HUDTextBinding>(true);

        foreach (HUDTextBinding binding in found)
        {
            if (binding.Key == HUDTextKey.None)
                continue;

            if (bindings.ContainsKey(binding.Key))
            {
                Debug.LogWarning(
                    $"Duplicate HUD key: {binding.Key}",
                    binding
                );

                continue;
            }

            bindings.Add(binding.Key, binding);
        }
    }

    private void OnTextUpdated(HUDTextUpdateEvent message)
    {
        if (bindings.TryGetValue(message.Key, out HUDTextBinding binding))
            binding.SetText(message.Text);
    }
}