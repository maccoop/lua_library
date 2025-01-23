using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManagerService : MonoBehaviour, IEService
{
    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private AssetBundle assetBundle;
    private Sprite _cache;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    public Sprite GetSprite(string spritePath)
    {
        if (!sprites.ContainsKey(spritePath))
        {
            _cache = Resources.Load<Sprite>(spritePath);
            sprites.Add(spritePath, _cache);
        }
        return sprites[spritePath];
    }
}