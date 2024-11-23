using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//����Dictionary�ṹ����Unity�����л�ϵͳ��֧�ֵ����ݽṹ���޷���Inspector�б���ʾ�ͱ༭
//�������Լ�ʵ��һ���ֵ�ṹ������ISerializationCallbackReceiver�ӿڲ�ʵ�����躯������
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    //����Inspector��ʾ�ļ���ֵ�б�List<>�ṹ��Unity��֧�ֵĿ����л��ṹ
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        //���л�ǰ����б��������
        keys.Clear();
        values.Clear();

        //thisָ�����ں˵�Dictionaryʵ������
        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        //�����л����ؽ��ֵ�
        this.Clear();
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            this[keys[i]] = values[i];
        }
    }
}