using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    [SerializeField] private List<TKey> keys;
    [SerializeField] private List<TValue> values;
    private int index;

    public JsonDictionary()
    {
        keys = new();
        values = new();
    }

    public TValue this[TKey key]
    {
        get
        {
            if (keys.Contains(key))
            {
                index = keys.IndexOf(key);
                return values[index];
            }
            throw new NullReferenceException(key.ToString());
        }
        set
        {
            if (keys.Contains(key))
            {
                index = keys.IndexOf(key);
                values[index] = value;
            }
            throw new NullReferenceException(key.ToString());
        }
    }

    public int Count => keys.Count;

    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => throw new NotImplementedException();

    public ICollection<TValue> Values => throw new NotImplementedException();

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (keys.Contains(item.Key))
            Debug.LogError($"Dictionary has item with key {item.Key}");
        else
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (keys.Contains(key))
        {
            Debug.LogError($"Dictionary has item with key {key}");
        }
        else
        {
            keys.Add(key);
            values.Add(value);
        }
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!keys.Contains(item.Key))
            return false;
        return values[keys.IndexOf(item.Key)].Equals(item.Value);
    }

    public bool ContainsKey(TKey key)
    {
        return keys.Contains(key);
    }

    [Obsolete("Not support CopyTo")]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new System.NotSupportedException();
    }

    [Obsolete("Not support GetEnumerator")]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new System.NotSupportedException();
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!Contains(item))
        {
            Debug.LogError($"Can't find {item.Key}");
            return false;
        }
        index = keys.IndexOf(item.Key);
        values.RemoveAt(index);
        return true;
    }

    public bool Remove(TKey key)
    {
        if (!ContainsKey(key))
        {
            throw new NullReferenceException(key.ToString());
        }
        index = keys.IndexOf(key);
        values.RemoveAt(index);
        return true;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }
        value = default(TValue);
        return false;
    }

    [Obsolete("Not support GetEnumerator")]
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotSupportedException();
    }
}
