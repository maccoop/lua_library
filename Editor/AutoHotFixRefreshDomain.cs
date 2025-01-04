using UnityEditor;
using UnityEngine;

public static class AutoHotFixRefreshDomain
{
    [InitializeOnLoadMethod]
    private static void OnProjectLoadedInEditor()
    {
        UnityEditor.AssemblyReloadEvents.afterAssemblyReload += OnDomainReload;
    }

    private static void OnDomainReload()
    {
#if HOTFIX_ENABLE
#endif
    }
}
