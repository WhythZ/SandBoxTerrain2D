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
    [SerializeField] private int seed;                        //������ɵ��������

    [Header("Terrain Shape")]
    [SerializeField] private float terrainRelief = 0.05f;     //����ε������صİ�������Ƶ��
    [SerializeField] private int heightMultiplier = 35;       //Ϊ��������[0,~]�ڵ�����������
    [SerializeField] private int heightAddition = 50;         //���εĻ������
    [SerializeField] private int dirtLayerHeight = 5;         //������ĺ��

    [Header("Cave Settings")]
    [SerializeField] private Texture2D caveSpreadTex;         //�洢���ɵĵ�ͼ��Ѩ������ͼ
    [SerializeField] private bool isGenerateCaves = false;    //�Ƿ����ɶ�Ѩ
    [SerializeField] private float caveFreq = 0.08f;          //��ն����ֵ�Ƶ������صİ�������Ƶ��
    [SerializeField] private float caveSize = 0.2f;           //��ֵԽ��Խ������caveFreq����Ѩ�ࣩ

    [Header("Biome Settings")]
    [SerializeField] private Texture2D biomeMapTex;           //����Ⱥϵ�ķֲ�����ͼ
    public float biomeFreq = 0.5f;                            //Ⱥϵ��Ƶ��
    [SerializeField] private BiomeSettings[] biomes;          //���ø�Ⱥϵ������

    public void GenerateTerrain(int _seed)
    {
        #region NoisesGeneration
        //���������ӣ������ɵ�ͼ�и���Ԫ�ص���������
        seed = _seed;
        GenerateAllTextures();
        #endregion

        //��ʵ�����ɵ���ǰ�ȳ�ʼ������
        TilemapManager.instance.InitTilemap();

        #region TilesPreSetting
        //ȡ��noiseTexture����ϵ�ĺ���y=PerlinNoise(f(x))���ߵ��·�������Ϊ����
        for (int _y = 0; _y < TilemapManager.instance.WorldLength; _y++)
        {
            for (int _x = 0; _x < TilemapManager.instance.WorldLength; _x++)
            {
                //��_x�����ڽ�ȡ������ͼ����������һ��[0,1]��Χ�ĵİ�������ֵ���ڴ˻���������һЩ�����������������Ҫ�İ�͹��ƽ�ĵ���
                float _height = Mathf.PerlinNoise((_x + seed) * terrainRelief, seed * terrainRelief) * heightMultiplier + heightAddition;
                //��ȡ��ǰ����Ⱥϵ���࣬������Ⱥϵ����ʹ��0��ΪĬ��Ⱥϵ����
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

                //���ݸ߶Ⱥ�Ⱥϵ���࣬���ò㼶����Ƭ����
                TileType _tileType = GetTileTypeByBiomeAt(_bTypeIdx, _x, _y, _height);
                //�����Ƿ����ɶ�Ѩ
                if (isGenerateCaves && caveSpreadTex.GetPixel(_x, _y) == Color.white)
                    _tileType = TileType.Air;

                //�������øõ㴦����Ƭ
                TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
            }
        }
        #endregion

        //ʵ�ʸ�������Ԥ���õ���Ƭ����������Ƭ��ͼ
        TilemapManager.instance.GenerateTilemap();
    }

    private TileType GetTileTypeByBiomeAt(int _bTypeIdx, int _x, int _y, float _height)
    {
        //������ʯ�㣨����ʯ��
        if (_y < _height - dirtLayerHeight)
        {
            //ע��˴����Ⱥ�����˳����ϡȱ�������ȱ�����
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
        //����������
        else if (_y < _height - 1)
        {
            if (_bTypeIdx == BiomeType.Desert.GetHashCode())
                return TileType.Sand;
            else if (_bTypeIdx == BiomeType.Snow.GetHashCode())
                return TileType.Snow;
            else
                return TileType.Dirt;
        }
        //���òݵز�
        else if (_y < _height)
        {
            if (_bTypeIdx == BiomeType.Desert.GetHashCode())
                return TileType.Sand;
            else if (_bTypeIdx == BiomeType.Snow.GetHashCode())
                return TileType.Snow;
            else
                return TileType.DirtGrass;

        }
        //���ÿ�����
        else
            return TileType.Air;
    }

    private void GenerateAllTextures()
    {
        //��Ѩ�ֲ��������������
        DrawPerlinNoiseTexture(seed, ref caveSpreadTex, caveFreq, caveSize);

        //����ͨ��seed�����Ĳ�ͬ���ӣ���Ϊʹ�õ�����ͬ��Ƶ�ʣ������ɲ�ͬȺϵ�ķֲ���������
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
        //����������εı߳���ʼ������ͼ��
        _noiseTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        //�����ؼ�������ֵ
        for (int x = 0; x < _noiseTex.width; x++)
        {
            for(int y = 0; y < _noiseTex.height; y++)
            {
                //����λ��(x,y)��������ӡ�Ƶ�ʣ�ʹ�ð�����������һ����[0,1]�������ֵ����Ϊrgb�Ļ�Խ��Խ�ӽ���ɫ��
                float _p = Mathf.PerlinNoise((x + _seed) * _freq, (y + _seed) * _freq);
                //��ĳ��[0,1]��Χ�ڵ���ֵ������ֵ_v�Ի��ֽ��ޣ���ɫ��Ϊ��Ѩ����ʯ��С���ȱ
                if (_p <= _size)
                    _noiseTex.SetPixel(x, y, Color.white);
                else
                    _noiseTex.SetPixel(x, y, Color.black);
            }
        }
        //���²�������ʹ������Ч
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
    //public BiomeType type;                  //Ⱥϵ������
    public Color color;                     //��Ⱥϵ��ʾ������ͼ�е���ɫ

    [Header("Biome Spread")]
    public float biomeSize = 0.3f;          //Ⱥϵ�Ĵ�С

    [Header("Ore Settings")]
    public OreSettings ores;                //Ⱥϵ�Ŀ���ֲ�
}

[System.Serializable]
class OreSettings
{
    [Header("Ore Spread")]
    public Texture2D coalSpreadTex;         //ú�����ɵķֲ�����ͼ
    public Texture2D ironSpreadTex;         //�������ɵķֲ�����ͼ
    public Texture2D goldSpreadTex;         //������ɵķֲ�����ͼ
    public Texture2D diamondSpreadTex;      //������ɵķֲ�����ͼ

    [Header("Coal")]
    public float coalRarity = 0.2f;         //ú��ϡȱ��
    public float coalSize = 0.18f;          //ú����С

    [Header("Iron")]
    public float ironRarity = 0.18f;        //����ϡȱ��
    public float ironSize = 0.16f;          //������С

    [Header("Gold")]
    public float goldRarity = 0.13f;        //���ϡȱ��
    public float goldSize = 0.11f;          //�����С

    [Header("Diamond")]
    public float diamondRarity = 0.12f;     //���ϡȱ��
    public float diamondSize = 0.02f;       //�����С
}