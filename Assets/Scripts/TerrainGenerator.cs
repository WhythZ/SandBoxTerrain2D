using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

enum TileType
{
    TreeLeaf,
    TreeLog,
    Grass,
    Dirt,
    Stone
}

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile Sprites")]
    [SerializeField] private Sprite treeLeaf;               //树叶瓦片
    [SerializeField] private Sprite treeLog;                //树干瓦片
    [SerializeField] private Sprite grass;                  //石头瓦片
    [SerializeField] private Sprite dirt;                   //泥土瓦片
    [SerializeField] private Sprite stone;                  //石头瓦片

    [Header("Terrain Size")]
    [SerializeField] private int worldSize = 100;           //正方形噪声材质图像的边长
    [SerializeField] private int heightMultiplier = 25;     //为地形厚度增加[0,5]的随机增量
    [SerializeField] private int heightAddition = 25;       //地形的基础厚度

    //[Header("Terrain Tiles")]
    //[SerializeField] private List<>

    [Header("Terrain Shape")]
    [SerializeField] private float surfaceThrehold = 0.2f;  //该值越大，地形越稀疏，越能体现caveFreq，洞穴越多
    [SerializeField] private float terrainFreq = 0.05f;     //与地形波动相关的柏林噪声频率

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //泥土层的厚度

    [Header("Terrain Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //是否生成洞穴
    [SerializeField] private float caveFreq = 0.05f;        //与空洞出现的频率正相关的柏林噪声频率

    [Header("Surface Trees")]
    [SerializeField] private float treeChance = 0.07f;      //树木在地表草地上生成的概率
    [SerializeField] private int maxTreeHeight = 7;         //树干的最大高度
    [SerializeField] private int minTreeHeight = 4;         //树干的最小高度

    [Header("Berlin Noise")]
    [SerializeField] private Texture2D noiseTexture;        //存储生成的噪声材质图像
    private float seed;                                     //随机生成的随机种子

    private void Start()
    {
        //随机生成一个种子，使得每次生成的噪声材质图像不同
        seed = UnityEngine.Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        //完全按照noiseTexture生成100x100的一万个Tile
        //for (int x = 0; x < worldSize; x++)
        //{
        //    for (int y = 0; y < worldSize; y++)
        //    {
        //        if (noiseTexture.GetPixel(x, y).r < 0.5f)
        //        {
        //            GameObject _newTile = new GameObject();
        //            _newTile.name = "Tile";
        //            _newTile.transform.parent = this.transform;
        //            _newTile.AddComponent<SpriteRenderer>();
        //            _newTile.GetComponent<SpriteRenderer>().sprite = tile;
        //            _newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
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
                Sprite _sprite;
                //草地层
                if (_y > _height - 1)
                    _sprite = grass;
                //泥土层
                else if (_y > _height - dirtLayerHeight)
                    _sprite = dirt;
                //石头层
                else
                    _sprite = stone;

                //控制是否生成洞穴
                if (isGenerateCaves)
                {
                    //仅在点的灰度大于某个[0,1]范围内的阈值时才生成该点瓦片；由于noiseTexture是灰度图，所以rgb三者均可（越大越接近白色）
                    if (noiseTexture.GetPixel(_x, _y).r > surfaceThrehold)
                        GenerateTileAt(_sprite, _x, _y);
                }
                else
                    GenerateTileAt(_sprite, _x, _y);
            }
        }
    }

    private void GenerateTileAt(Sprite _sprite, int _x, int _y)
    {
        //先在对应位置放置砖块，然后检测该砖块可能附带生成的其他东西
        PlaceTileAt(_sprite,_x, _y);

        //在草地上按概率生成树，注意传入的坐标是y+1（y就会直接覆盖掉草皮）
        if(_sprite == grass)
        {
            int _chance = Mathf.RoundToInt(treeChance * 100);
            int _random = UnityEngine.Random.Range(0, 100);
            if (_random < _chance)
                PlaceTreeAt(_x, _y + 1);
        }
    }

    private void PlaceTreeAt(int _x, int _y)
    {
        //生成树干
        int _height = UnityEngine.Random.Range(minTreeHeight, maxTreeHeight);
        for (int i = 0; i < _height; i++)
            PlaceTileAt(treeLog, _x, _y + i);

        //生成树叶
        PlaceTileAt(treeLeaf, _x, _y + _height);
        PlaceTileAt(treeLeaf, _x - 1, _y + _height);
        PlaceTileAt(treeLeaf, _x + 1, _y + _height);
        PlaceTileAt(treeLeaf, _x, _y + _height + 1);
    }

    private void PlaceTileAt(Sprite _sprite, int _x, int _y)
    {
        GameObject _newTile = new GameObject();
        _newTile.name = _sprite.name;
        _newTile.transform.parent = this.transform;
        //偏移量0.5f是为了确保和Unity的网格重合，好看一点
        _newTile.transform.position = new Vector2(_x + 0.5f, _y + 0.5f);
        _newTile.AddComponent<SpriteRenderer>();
        _newTile.GetComponent<SpriteRenderer>().sprite = _sprite;
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
