using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : Manager<TerrainManager>
{
    [Header("Berlin Noise")]
    private float seed;                                     //随机生成的随机种子
    [SerializeField] private Texture2D caveNoiseTex;        //存储生成的地图洞穴的噪声材质图像
    
    [Header("Terrain Shape")]
    [SerializeField] private float terrainFreq = 0.05f;     //与地形波动相关的柏林噪声频率
    [SerializeField] private int heightMultiplier = 25;     //乘上柏林噪声后为地形厚度增加[0,~]内的随机增量
    [SerializeField] private int heightAddition = 25;       //地形的基础厚度

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //泥土层的厚度

    [Header("Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //是否生成洞穴
    [SerializeField] private float caveFreq = 0.05f;        //与空洞出现的频率正相关的柏林噪声频率
    [SerializeField] private float caveThrehold = 0.2f;     //该值越大，越能体现caveFreq（地形稀疏且洞穴多）

    [Header("Trees")]
    [SerializeField] private float treeChance = 0.07f;      //树木在地表草地上生成的概率
    public float TreeChance { get => treeChance; }
    [SerializeField] private int maxTreeHeight = 7;         //树干的最大高度
    public int MaxTreeHeight { get => maxTreeHeight; }
    [SerializeField] private int minTreeHeight = 4;         //树干的最小高度
    public int MinTreeHeight { get => minTreeHeight; }

    private void Start()
    {
        //随机生成一个种子，使得每次生成的噪声材质图像不同
        seed = UnityEngine.Random.Range(-10000, 10000);
        //生成地图中各种元素的噪声
        GenerateNoiseTexture(ref caveNoiseTex, caveFreq);

        //根据种子和噪声生成地形
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        //完全按照噪声图像生成worldSize×worldSize个瓦片
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

        //在实际生成地形前先初始化区块
        TilemapManager.instance.InitTilemap();

        //取用noiseTexture坐标系的函数y=PerlinNoise(f(x))曲线的下方部分作为地形
        for (int _y = 0; _y < TilemapManager.instance.WorldLength; _y++)
        {
            for (int _x = 0; _x < TilemapManager.instance.WorldLength; _x++)
            {
                //用_x对用于截取整个地图的曲线引入一个[0,1]范围的的柏林噪声值，在此基础上增加一些计算参数，以生成需要的凹凸不平的地形
                float _height = Mathf.PerlinNoise((_x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

                //依据高度设置不同的地形瓦片层
                TileType _tileType;
                //岩石层
                if(_y < _height - dirtLayerHeight)
                    _tileType = TileType.Stone;
                //泥土层
                else if (_y < _height - 1)
                    _tileType = TileType.Dirt;
                //草地层
                else if (_y < _height)
                    _tileType = TileType.DirtGrass;
                //空气层
                else
                    _tileType = TileType.Air;

                //控制是否生成洞穴
                if (isGenerateCaves)
                {
                    //仅在点的灰度大于某个[0,1]范围内的阈值时才生成该点瓦片；由于noiseTexture是灰度图，所以rgb三者均可（越大越接近白色）
                    if (caveNoiseTex.GetPixel(_x, _y).r > caveThrehold)
                        TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
                }
                else
                    TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
            }
        }

        //实际根据上述设置的瓦片类型生成瓦片地图
        TilemapManager.instance.GenerateTilemap();
    }

    private void GenerateNoiseTexture(ref Texture2D _noiseTex, float _freq)
    {
        //按照世界地形的边长初始化噪声图形
        _noiseTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        //逐像素计算噪声值
        for (int x = 0; x < _noiseTex.width; x++)
        {
            for(int y = 0; y < _noiseTex.height; y++)
            {
                //依据位置(x,y)、随机种子、频率，使用柏林噪声生成一个在[0,1]间的噪声值
                float _v = Mathf.PerlinNoise((x + seed) * _freq, (y + seed) * _freq);
                //将生成的噪声值作为灰度值，赋给每个像素
                _noiseTex.SetPixel(x, y, new Color(_v, _v, _v));
            }
        }
        //更新材质纹理，使更改生效
        _noiseTex.Apply();
    }
}
