using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class XMLReader
{
    static XmlDocument xmlDoc;

    public static async Task Init()
    {
        string uri = "C:/Users/Admin/Project/LuaScript/flappy/flappy_bird.xml";
        var request = UnityWebRequest.Get(uri);
        await request.SendWebRequest();
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(request.downloadHandler.text);
        PlayerPrefs.GetInt("day", 0);
    }

    public static string[] GetDependencies()
    {
        return GetAllChildElementByParentTagName("dependencies");
    }
    public static string[] GetMain()
    {
        return GetAllChildTagByParentTagName("main");
    }
    public static string[] GetServices()
    {
        return GetAllChildTagByParentTagName("service");
    }

    internal static string[] GetHotFix()
    {
        return GetAllChildTagByParentTagName("hotfix");
    }
    
    internal static string[] GetRequire()
    {
        return GetAllChildTagByParentTagName("require");
    }

    public static string[] GetAllChildTagByParentTagName(string tagName)
    {
        List<string> result = new();
        var items = xmlDoc.GetElementsByTagName(tagName);
        XmlNode item;
        XmlNode cache = null;
        for (int i = 0; i < items.Count; i++)
        {
            item = items[i];
            for (int j = 0; j < item.ChildNodes.Count; j++)
            {
                cache = item.ChildNodes[j];
                result.Add(cache.Name + "@" + cache.Attributes["path"].Value);
            }
        }
        return result.ToArray();
    }

    public static string[] GetAllChildElementByParentTagName(string tagName)
    {
        List<string> result = new();
        var items = xmlDoc.GetElementsByTagName(tagName);
        XmlNode item;
        for (int i = 0; i < items.Count; i++)
        {
            item = items[i];
            for (int j = 0; j < item.ChildNodes.Count; j++)
            {
                result.Add(item.ChildNodes[j].Attributes["path"].Value);
            }
        }
        return result.ToArray();
    }

}
