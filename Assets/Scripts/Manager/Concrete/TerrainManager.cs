using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : Manager<TerrainManager>
{
    [Header("Perlin Noise Seed")]
    [SerializeField] private int seed;                        //随机生成的随机种子

    [Header("Terrain Shape")]
    [SerializeField] private float terrainRelief = 0.05f;     //与地形地起伏相关的柏林噪声频率
    [SerializeField] private int heightMultiplier = 35;       //为地形增加[0,~]内的随机厚度增量
    [SerializeField] private int heightAddition = 50;         //地形的基础厚度
    [SerializeField] private int dirtLayerHeight = 5;         //泥土层的厚度

    [Header("Cave Settings")]
    [SerializeField] private Texture2D caveSpreadTex;         //存储生成的地图洞穴的噪声图
    [SerializeField] private bool isGenerateCaves = false;    //是否生成洞穴
    [SerializeField] private float caveFreq = 0.08f;          //与空洞出现的频率正相关的柏林噪声频率
    [SerializeField] private float caveSize = 0.2f;           //该值越大，越能体现caveFreq（洞穴多）

    [Header("Biome Settings")]
    [SerializeField] private Texture2D biomeMapTex;           //生物群系的分布噪声图
    public float biomeFreq = 0.5f;                            //群系的频率
    [SerializeField] private BiomeSettings[] biomes;          //设置各群系的属性

    public void GenerateTerrain(int _seed)
    {
        #region NoisesGeneration
        //先设置种子，后生成地图中各种元素的噪声纹理
        seed = _seed;
        GenerateAllTextures();
        #endregion

        //在实际生成地形前先初始化区块
        TilemapManager.instance.InitTilemap();

        #region TilesPreSetting
        //取用noiseTexture坐标系的函数y=PerlinNoise(f(x))曲线的下方部分作为地形
        for (int _y = 0; _y < TilemapManager.instance.WorldLength; _y++)
        {
            for (int _x = 0; _x < TilemapManager.instance.WorldLength; _x++)
            {
                //用_x对用于截取整个地图的曲线引入一个[0,1]范围的的柏林噪声值，在此基础上增加一些计算参数，以生成需要的凹凸不平的地形
                float _height = Mathf.PerlinNoise((_x + seed) * terrainRelief, seed * terrainRelief) * heightMultiplier + heightAddition;
                //获取当前生物群系种类，对于无群系，的使用0作为默认群系种类
                int _bTypeIdx = 0;
                Color _col = biomeMapTex.GetPixel(_x, _y);
                for (int i = 0; i < biomes.Length; i++)
                {
                    if (biomes[i].color == _col)
                    {
                        _bTypeIdx = i;
                        break;
                    }
                }

                //依据高度和群系种类，设置层级的瓦片种类
                TileType _tileType = GetTileTypeByBiomeAt(_bTypeIdx, _x, _y, _height);
                //控制是否生成洞穴
                if (isGenerateCaves && caveSpreadTex.GetPixel(_x, _y) == Color.white)
                    _tileType = TileType.Air;

                //最终设置该点处的瓦片
                TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
            }
        }
        #endregion

        //实际根据上述预设置的瓦片类型生成瓦片地图
        TilemapManager.instance.GenerateTilemap();
    }

    private TileType GetTileTypeByBiomeAt(int _bTypeIdx, int _x, int _y, float _height)
    {
        //设置岩石层（含矿石）
        if (_y < _height - dirtLayerHeight)
        {
            //注意此处的先后优先顺序，最稀缺的最优先被生成
            if (biomes[_bTypeIdx].ores.diamondSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Diamond;
            else if (biomes[_bTypeIdx].ores.goldSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Gold;
            else if (biomes[_bTypeIdx].ores.ironSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Iron;
            else if (biomes[_bTypeIdx].ores.coalSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Coal;
            else
            {
                if (_bTypeIdx == BiomeType.Desert.GetHashCode())
                    return TileType.Sand;
                else if (_bTypeIdx == BiomeType.Snow.GetHashCode())
                    return TileType.Snow;
                else
                    return TileType.Stone;
            }
        }
        //设置泥土层
        else if (_y < _height - 1)
        {
            if (_bTypeIdx == BiomeType.Desert.GetHashCode())
                return TileType.Sand;
            else if (_bTypeIdx == BiomeType.Snow.GetHashCode())
                return TileType.Snow;
            else
                return TileType.Dirt;
        }
        //设置草地层
        else if (_y < _height)
        {
            if (_bTypeIdx == BiomeType.Desert.GetHashCode())
                return TileType.Sand;
            else if (_bTypeIdx == BiomeType.Snow.GetHashCode())
                return TileType.Snow;
            else
                return TileType.DirtGrass;

        }
        //设置空气层
        else
            return TileType.Air;
    }

    private void GenerateAllTextures()
    {
        //洞穴分布噪声纹理的生成
        DrawPerlinNoiseTexture(seed, ref caveSpreadTex, caveFreq, caveSize);

        //采用通过seed衍生的不同种子（因为使用的是相同的频率），生成不同群系的分布噪声纹理
        biomeMapTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        DrawBiomeTextures(2 * seed, BiomeType.Grass);
        DrawBiomeTextures(3 * seed, BiomeType.Desert);
        DrawBiomeTextures(4 * seed, BiomeType.Snow);
    }

    private void DrawBiomeTextures(int _seed, BiomeType _biomeType)
    {
        #region BiomeSettings
        int _bIdx = _biomeType.GetHashCode();
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].ores.coalSpreadTex, biomes[_bIdx].ores.coalRarity, biomes[_bIdx].ores.coalSize);
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].ores.ironSpreadTex, biomes[_bIdx].ores.ironRarity, biomes[_bIdx].ores.ironSize);
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].ores.goldSpreadTex, biomes[_bIdx].ores.goldRarity, biomes[_bIdx].ores.goldSize);
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].ores.diamondSpreadTex, biomes[_bIdx].ores.diamondRarity, biomes[_bIdx].ores.diamondSize);
        #endregion

        #region BiomeSpread
        for (int x = 0; x < biomeMapTex.width; x++)
        {
            for (int y = 0; y < biomeMapTex.height; y++)
            {
                float _p = Mathf.PerlinNoise((x + _seed) * biomeFreq, (y + _seed) * biomeFreq);
                if (_p <= biomes[_biomeType.GetHashCode()].biomeSize)
                    biomeMapTex.SetPixel(x, y, biomes[_biomeType.GetHashCode()].color);
            }
        }
        biomeMapTex.Apply();
        #endregion
    }

    private void DrawPerlinNoiseTexture(int _seed, ref Texture2D _noiseTex, float _freq, float _size)
    {
        //按照世界地形的边长初始化噪声图形
        _noiseTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        //逐像素计算噪声值
        for (int x = 0; x < _noiseTex.width; x++)
        {
            for(int y = 0; y < _noiseTex.height; y++)
            {
                //依据位置(x,y)、随机种子、频率，使用柏林噪声生成一个在[0,1]间的噪声值（作为rgb的话越大越接近白色）
                float _p = Mathf.PerlinNoise((x + _seed) * _freq, (y + _seed) * _freq);
                //以某个[0,1]范围内的阈值对噪声值_p以划分界限，白色作为洞穴、矿石等小块空缺
                if (_p <= _size)
                    _noiseTex.SetPixel(x, y, Color.white);
                else
                    _noiseTex.SetPixel(x, y, Color.black);
            }
        }
        //更新材质纹理，使更改生效
        _noiseTex.Apply();
    }
}

enum BiomeType
{
    Grass = 0,
    Desert = 1,
    Snow = 2
}

[System.Serializable]
class BiomeSettings
{
    [Header("Biome Type")]
    //public BiomeType type;                  //群系的种类
    public Color color;                     //该群系显示在噪声图中的颜色

    [Header("Biome Spread")]
    public float biomeSize = 0.3f;          //群系的大小

    [Header("Ore Settings")]
    public OreSettings ores;                //群系的矿物分布
}

[System.Serializable]
class OreSettings
{
    [Header("Ore Spread")]
    public Texture2D coalSpreadTex;         //煤矿生成的分布噪声图
    public Texture2D ironSpreadTex;         //铁矿生成的分布噪声图
    public Texture2D goldSpreadTex;         //金矿生成的分布噪声图
    public Texture2D diamondSpreadTex;      //钻矿生成的分布噪声图

    [Header("Coal")]
    public float coalRarity = 0.2f;         //煤矿稀缺度
    public float coalSize = 0.18f;          //煤矿块大小

    [Header("Iron")]
    public float ironRarity = 0.18f;        //铁矿稀缺度
    public float ironSize = 0.16f;          //铁矿块大小

    [Header("Gold")]
    public float goldRarity = 0.13f;        //金矿稀缺度
    public float goldSize = 0.11f;          //金矿块大小

    [Header("Diamond")]
    public float diamondRarity = 0.12f;     //钻矿稀缺度
    public float diamondSize = 0.02f;       //钻矿块大小
}
