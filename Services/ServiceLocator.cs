using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void InitServiceLocator()
    {
        Instance = new GameObject("Service Locator").AddComponent<ServiceLocator>();
        Instance.Init();
    }


    Dictionary<string, IEService> _services;

    private void Init()
    {
        _services = new Dictionary<string, IEService>();
    }

    public IEService GetService(string serviceName)
    {
        try
        {
            return _services[serviceName];
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public T GetService<T>() where T : IEService
    {
        try
        {
            return (T)_services[typeof(T).Name];
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void Register(string serviceName, IEService service)
    {
        if (_services.ContainsKey(serviceName))
        {
            throw new Exception($"Service {serviceName} was registed!");
        }
        Debug.Log($"{serviceName} was Registed success!");
        _services.Add(serviceName, service);
    }
    public void Register(IEService service)
    {
        string serviceName = service.GetType().FullName;
        Register(serviceName, service);
    }
}
