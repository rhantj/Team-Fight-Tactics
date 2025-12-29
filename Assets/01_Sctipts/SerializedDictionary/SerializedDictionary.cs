using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedDictionary<TKey, TValue> :
    IDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dict;

    private Dictionary<TKey, TValue> Dictionary
    {
        get
        {
            if (dict == null)
            {
                dict = new Dictionary<TKey, TValue>();
                for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
                {
                    if (!dict.ContainsKey(keys[i]))
                        dict.Add(keys[i], values[i]);
                }
            }

            return dict;
        }
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        if (dict == null)
            return;

        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dict = new Dictionary<TKey, TValue>();

        int count = Mathf.Min(keys.Count, values.Count);
        for (int i = 0; i < count; i++)
        {
            if (!dict.ContainsKey(keys[i]))
            {
                dict.Add(keys[i], values[i]);
            }
            else
            {
                Debug.LogWarning($"[SerializedDictionary] 중복 키 감지: {keys[i]} → 무시");
            }
        }
    }

    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set => Dictionary[key] = value;
    }

    public ICollection<TKey> Keys => Dictionary.Keys;
    public ICollection<TValue> Values => Dictionary.Values;
    public int Count => Dictionary.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value) => Dictionary.Add(key, value);

    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    public bool Remove(TKey key) => Dictionary.Remove(key);

    public bool TryGetValue(TKey key, out TValue value) =>
        Dictionary.TryGetValue(key, out value);

    public void Add(KeyValuePair<TKey, TValue> item) =>
        Dictionary.Add(item.Key, item.Value);

    public void Clear()
    {
        Dictionary.Clear();
        keys.Clear();
        values.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        Dictionary.ContainsKey(item.Key) &&
        EqualityComparer<TValue>.Default.Equals(Dictionary[item.Key], item.Value);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int i = arrayIndex;
        foreach (var kvp in Dictionary)
        {
            array[i++] = kvp;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (Contains(item))
            return Dictionary.Remove(item.Key);
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        Dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
