using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager<GameManager>
{
    void Start()
    {
        #region TerrainGeneration
        //随机生成一个种子，使得每次生成的噪声材质图像不同
        int _seed = UnityEngine.Random.Range(-10000, 10000);
        //根据种子生成地形
        TerrainManager.instance.GenerateTerrain(_seed);
        #endregion
    }
}
