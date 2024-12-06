using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�ɼ̳й����������ĳ�����࣬����Լ��Ϊ��������ࣨMonoBehavior���������GameObject�ϣ����޷���newʵ���������Բ�Ӧʹ��newԼ����
public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
{
    //�ⲿͨ�������Է��ʸù���������
    public static T instance;

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject); //ȷ��ֻ��һ������������
        else
            instance = (T)this; //���޷�new����ǿתΪ�������������T
    }
}