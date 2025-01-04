using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class EnableHotFix : MonoBehaviour
{
    const string HOTFIX_SYMBOL = "HOTFIX_ENABLE";
    [MenuItem("XLua/Enable Hotfix")]
    public static void ActiveHotFix()
    {
        var target = NamedBuildTarget.Standalone;
#if UNITY_ANDROID
        target = NamedBuildTarget.Android;
#endif
        var defines = PlayerSettings.GetScriptingDefineSymbols(target);
        var i = defines.Contains(HOTFIX_SYMBOL);
        if (i)
        {
            Debug.Log("HOTFIX was enabled!");
            return;
        }
        defines += ";" + HOTFIX_SYMBOL;
        PlayerSettings.SetScriptingDefineSymbols(target, defines);
        Debug.Log("HOTFIX was active success!");
    }

    [MenuItem("XLua/Disable Hotfix")]
    public static void DisableHotFix()
    {
        var target = NamedBuildTarget.Standalone;
#if UNITY_ANDROID
        target = NamedBuildTarget.Android;
#endif
        var defines = PlayerSettings.GetScriptingDefineSymbols(target);
        defines = defines.Replace(";" + HOTFIX_SYMBOL, "");
        PlayerSettings.SetScriptingDefineSymbols(target, defines);
        Debug.Log("Disable Hotfix!");
    }

}
