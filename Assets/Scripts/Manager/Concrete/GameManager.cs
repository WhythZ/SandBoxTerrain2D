using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager<GameManager>
{
    void Start()
    {
        #region TerrainGeneration
        //�������һ�����ӣ�ʹ��ÿ�����ɵ���������ͼ��ͬ
        int _seed = UnityEngine.Random.Range(-10000, 10000);
        //�����������ɵ���
        TerrainManager.instance.GenerateTerrain(_seed);
        #endregion
    }
}
