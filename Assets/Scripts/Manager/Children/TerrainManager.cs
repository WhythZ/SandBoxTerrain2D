using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : Manager<TerrainManager>
{
    #region ClassFields
    [Header("Terrain Size")]
    [SerializeField] private int worldSize = 100;           //正方形噪声材质图像的边长
    [SerializeField] private int heightMultiplier = 25;     //为地形厚度增加[0,5]的随机增量
    [SerializeField] private int heightAddition = 25;       //地形的基础厚度

    [Header("Terrain Shape")]
    [SerializeField] private float surfaceThrehold = 0.2f;  //该值越大，越体现caveFreq，地形越稀疏且洞穴越多
    [SerializeField] private float terrainFreq = 0.05f;     //与地形波动相关的柏林噪声频率

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //泥土层的厚度

    [Header("Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //是否生成洞穴
    [SerializeField] private float caveFreq = 0.05f;        //与空洞出现的频率正相关的柏林噪声频率

    [Header("Trees")]
    [SerializeField] private float treeChance = 0.07f;      //树木在地表草地上生成的概率
    [SerializeField] private int maxTreeHeight = 7;         //树干的最大高度
    [SerializeField] private int minTreeHeight = 4;         //树干的最小高度

    [Header("Berlin Noise")]
    [SerializeField] private Texture2D noiseTexture;        //存储生成的噪声材质图像
    private float seed;                                     //随机生成的随机种子
    #endregion

    #region ClassProperties
    public float TreeChance { get => treeChance; }
    public int MaxTreeHeight { get => maxTreeHeight; }
    public int MinTreeHeight { get => minTreeHeight; }
    #endregion

    private void Start()
    {
        //随机生成一个种子，使得每次生成的噪声材质图像不同
        seed = UnityEngine.Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        //完全按照噪声图像生成100x100个瓦片
        //for (int x = 0; x < worldSize; x++)
        //{
        //    for (int y = 0; y < worldSize; y++)
        //    {
        //        if (noiseTexture.GetPixel(x, y).r < 0.5f)
        //        {
        //            //在(x,y)处生成一个瓦片
        //        }
        //    }
        //}

        //取用noiseTexture坐标系的函数y=PerlinNoise(f(x))曲线的下方部分作为地形
        for (int _x = 0; _x < worldSize; _x++)
        {
            //用x对截取的高度函数曲线引入一个[0,1]范围的的柏林噪声值，在此基础上增加一些计算参数，以生成需要的凹凸不平的地形
            float _height = Mathf.PerlinNoise((_x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;
            for (int _y = 0; _y < _height; _y++)
            {
                //依据高度设置不同的地形瓦片层
                TileType _tileType;
                //草地层
                if (_y > _height - 1)
                    _tileType = TileType.DirtGrass;
                //泥土层
                else if (_y > _height - dirtLayerHeight)
                    _tileType = TileType.Dirt;
                //石头层
                else
                    _tileType = TileType.Stone;

                //控制是否生成洞穴
                if (isGenerateCaves)
                {
                    //仅在点的灰度大于某个[0,1]范围内的阈值时才生成该点瓦片；由于noiseTexture是灰度图，所以rgb三者均可（越大越接近白色）
                    if (noiseTexture.GetPixel(_x, _y).r > surfaceThrehold)
                        TileManager.instance.GenerateTileAt(_tileType, _x, _y);
                }
                else
                    TileManager.instance.GenerateTileAt(_tileType, _x, _y);
            }
        }
    }

    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        //逐像素计算噪声值
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for(int y = 0; y < noiseTexture.height; y++)
            {
                //依据位置(x,y)、随机种子、频率，使用柏林噪声生成一个在[0,1]间的噪声值
                float _v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                //将生成的噪声值作为灰度值，赋给每个像素
                noiseTexture.SetPixel(x, y, new Color(_v, _v, _v));
            }
        }

        //更新材质纹理，使更改生效
        noiseTexture.Apply();
    }
}
