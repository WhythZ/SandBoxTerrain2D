using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//由于Dictionary结构不是Unity的序列化系统所支持的数据结构，无法在Inspector中被显示和编辑
//故我们自己实现一个字典结构，调用ISerializationCallbackReceiver接口并实现所需函数即可
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    //用于Inspector显示的键和值列表，List<>结构是Unity所支持的可序列化结构
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        //序列化前清空列表并填充数据
        keys.Clear();
        values.Clear();

        //this指的是内核的Dictionary实例对象
        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        //反序列化后重建字典
        this.Clear();
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            this[keys[i]] = values[i];
        }
    }
}