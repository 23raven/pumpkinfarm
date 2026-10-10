using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class GameServices : MonoBehaviour
{
    private static GameServices instance;

    private readonly Dictionary<Type, object> services =
        new Dictionary<Type, object>();

    public static GameServices Instance => instance;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError(
                "Only one GameServices instance is allowed.",
                this
            );

            enabled = false;
            return;
        }

        instance = this;
    }

    public bool Register<T>(T service) where T : class
    {
        if (service == null)
        {
            Debug.LogError(
                $"Cannot register null service: {typeof(T).Name}",
                this
            );

            return false;
        }

        Type type = typeof(T);

        if (services.TryGetValue(type, out object existing))
        {
            if (ReferenceEquals(existing, service))
                return true;

            Debug.LogError(
                $"Service already registered: {type.Name}",
                this
            );

            return false;
        }

        services.Add(type, service);

        Debug.Log($"Service registered: {type.Name}", this);

        return true;
    }

    public void Unregister<T>(T service) where T : class
    {
        if (service == null)
            return;

        Type type = typeof(T);

        if (services.TryGetValue(type, out object existing) &&
            ReferenceEquals(existing, service))
        {
            services.Remove(type);
        }
    }

    public bool TryGet<T>(out T service) where T : class
    {
        if (services.TryGetValue(typeof(T), out object registered) &&
            registered is T typed)
        {
            service = typed;
            return true;
        }

        service = null;
        return false;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}