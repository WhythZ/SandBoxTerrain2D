using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : Manager<TerrainManager>
{
    [Header("Berlin Noise")]
    [SerializeField] private int seed;                      //随机生成的随机种子
    [SerializeField] private Texture2D caveSpreadTex;       //存储生成的地图洞穴的噪声图
    [SerializeField] private Texture2D coalSpreadTex;       //煤矿生成的分布噪声图
    [SerializeField] private Texture2D ironSpreadTex;       //铁矿生成的分布噪声图
    [SerializeField] private Texture2D goldSpreadTex;       //金矿生成的分布噪声图
    [SerializeField] private Texture2D diamondSpreadTex;    //钻矿生成的分布噪声图

    [Header("Terrain Shape")]
    [SerializeField] private float terrainRelief = 0.05f;   //与地形地起伏相关的柏林噪声频率
    [SerializeField] private int heightMultiplier = 35;     //乘上柏林噪声后为地形厚度增加[0,~]内的随机增量
    [SerializeField] private int heightAddition = 50;       //地形的基础厚度

    [Header("Ore Settings")]
    [SerializeField] private float coalRarity = 0.2f;       //煤矿稀缺度
    [SerializeField] private float coalSize = 0.18f;        //煤矿块大小
    [SerializeField] private float ironRarity = 0.18f;      //铁矿稀缺度
    [SerializeField] private float ironSize = 0.16f;        //铁矿块大小
    [SerializeField] private float goldRarity = 0.13f;      //金矿稀缺度
    [SerializeField] private float goldSize = 0.11f;        //金矿块大小
    [SerializeField] private float diamondRarity = 0.12f;   //钻矿稀缺度
    [SerializeField] private float diamondSize = 0.02f;     //钻矿块大小

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //泥土层的厚度

    [Header("Cave Settings")]
    [SerializeField] private bool isGenerateCaves = false;  //是否生成洞穴
    [SerializeField] private float caveFreq = 0.08f;        //与空洞出现的频率正相关的柏林噪声频率
    [SerializeField] private float caveSize = 0.2f;         //该值越大，越能体现caveFreq（洞穴多）

    [Header("Tree Settings")]
    [SerializeField] private float treeChance = 0.07f;      //树木在地表草地上生成的概率
    public float TreeChance { get => treeChance; }
    [SerializeField] private int maxTreeHeight = 7;         //树干的最大高度
    public int MaxTreeHeight { get => maxTreeHeight; }
    [SerializeField] private int minTreeHeight = 4;         //树干的最小高度
    public int MinTreeHeight { get => minTreeHeight; }

    public void GenerateTerrain(int _seed)
    {
        #region NoisesGeneration
        //先设置种子，后生成噪声
        seed = _seed;
        //生成地图中各种元素的噪声
        GenerateNoiseTexture(ref caveSpreadTex, caveFreq, caveSize);
        GenerateNoiseTexture(ref coalSpreadTex, coalRarity, coalSize);
        GenerateNoiseTexture(ref ironSpreadTex, ironRarity, ironSize);
        GenerateNoiseTexture(ref goldSpreadTex, goldRarity, goldSize);
        GenerateNoiseTexture(ref diamondSpreadTex, diamondRarity, diamondSize);
        #endregion

        //在实际生成地形前先初始化区块
        TilemapManager.instance.InitTilemap();

        //取用noiseTexture坐标系的函数y=PerlinNoise(f(x))曲线的下方部分作为地形
        for (int _y = 0; _y < TilemapManager.instance.WorldLength; _y++)
        {
            for (int _x = 0; _x < TilemapManager.instance.WorldLength; _x++)
            {
                //用_x对用于截取整个地图的曲线引入一个[0,1]范围的的柏林噪声值，在此基础上增加一些计算参数，以生成需要的凹凸不平的地形
                float _height = Mathf.PerlinNoise((_x + seed) * terrainRelief, seed * terrainRelief) * heightMultiplier + heightAddition;
                //依据高度设置不同的地形瓦片层
                TileType _tileType;

                //设置岩石层（含矿石）
                if (_y < _height - dirtLayerHeight)
                {
                    //注意此处的先后优先顺序，最稀缺的最优先被生成
                    if (diamondSpreadTex.GetPixel(_x, _y) == Color.white)
                        _tileType = TileType.Diamond;
                    else if (goldSpreadTex.GetPixel(_x, _y) == Color.white)
                        _tileType = TileType.Gold;
                    else if (ironSpreadTex.GetPixel(_x, _y) == Color.white)
                        _tileType = TileType.Iron;
                    else if (coalSpreadTex.GetPixel(_x, _y) == Color.white)
                        _tileType = TileType.Coal;
                    else
                        _tileType = TileType.Stone;
                }
                //设置泥土层
                else if (_y < _height - 1)
                    _tileType = TileType.Dirt;
                //设置草地层
                else if (_y < _height)
                    _tileType = TileType.DirtGrass;
                //设置空气层
                else
                    _tileType = TileType.Air;

                //控制是否生成洞穴
                if (isGenerateCaves && caveSpreadTex.GetPixel(_x, _y) == Color.white)
                        _tileType = TileType.Air;

                //最终设置该点处的瓦片
                TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
            }
        }

        //实际根据上述预设置的瓦片类型生成瓦片地图
        TilemapManager.instance.GenerateTilemap();
    }

    private void GenerateNoiseTexture(ref Texture2D _noiseTex, float _freq, float _threhold)
    {
        //按照世界地形的边长初始化噪声图形
        _noiseTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        //逐像素计算噪声值
        for (int x = 0; x < _noiseTex.width; x++)
        {
            for(int y = 0; y < _noiseTex.height; y++)
            {
                //依据位置(x,y)、随机种子、频率，使用柏林噪声生成一个在[0,1]间的噪声值（作为rgb的话越大越接近白色）
                float _v = Mathf.PerlinNoise((x + seed) * _freq, (y + seed) * _freq);
                //以某个[0,1]范围内的阈值对噪声值_v以划分界限，白色作为洞穴、矿石等小块空缺
                if (_v <= _threhold)
                    _noiseTex.SetPixel(x, y, Color.white);
                else
                    _noiseTex.SetPixel(x, y, Color.black);
            }
        }
        //更新材质纹理，使更改生效
        _noiseTex.Apply();
    }
}
