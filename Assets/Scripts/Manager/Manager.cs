using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//可继承管理器单例的抽象基类，泛型约束为该类的子类（MonoBehavior必须挂载在GameObject上，而无法被new实例化，所以不应使用new约束）
public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
{
    //外部通过此属性访问该管理器单例
    public static T instance;

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject); //确保只有一个管理器单例
        else
            instance = (T)this; //因无法new，故强转为子类管理器类型T
    }
}