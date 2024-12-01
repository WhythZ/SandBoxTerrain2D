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
    [SerializeField] private bool isGenerateCaves = false;    //�Ƿ����ɶ�Ѩ
    [SerializeField] private Texture2D caveSpreadTex;         //�洢���ɵĵ�ͼ��Ѩ������ͼ
    [SerializeField] private float caveFreq = 0.08f;          //��ն����ֵ�Ƶ������صİ�������Ƶ��
    [SerializeField] private float caveSize = 0.2f;           //��ֵԽ��Խ������caveFreq����Ѩ�ࣩ

    [Header("Biome Settings")]
    [SerializeField] private float biomeFreq = 0.05f;         //���Ƹ�Ⱥϵ��ռ��
    [SerializeField] private Gradient biomeSpread;            //���Ƹ�Ⱥϵ��ռ��
    [SerializeField] private Texture2D biomeMapTex;           //����Ⱥϵ�ķֲ�����ͼ
    [SerializeField] private BiomeSettings[] biomes;          //���ø�Ⱥϵ������
    private Color[] biomeColors;                              //�洢��Gradient�����õĸ�Ⱥϵ��ɫ

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
                //��ȡ��ǰ����Ⱥϵ����
                Color _biomeColor = biomeMapTex.GetPixel(_x, _y);
                int _bTypeIdx = System.Array.IndexOf(biomeColors, _biomeColor);

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
            if (biomes[_bTypeIdx].diamondSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Diamond;
            else if (biomes[_bTypeIdx].goldSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Gold;
            else if (biomes[_bTypeIdx].ironSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Iron;
            else if (biomes[_bTypeIdx].coalSpreadTex.GetPixel(_x, _y) == Color.white)
                return TileType.Coal;
            else
            {
                return TileType.Stone;
            }
        }
        //����������
        else if (_y < _height - 1)
        {
            return TileType.Dirt;
        }
        //���òݵز�
        else if (_y < _height)
        {
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
        
        //Ⱥϵ�ֲ��������������
        GenerateBiomes(seed);
    }

    private void GenerateBiomes(int _seed)
    {
        #region BiomeMapTex
        if (biomeMapTex == null)
            biomeMapTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        for (int x = 0; x < biomeMapTex.width; x++)
        {
            for (int y = 0; y < biomeMapTex.height; y++)
            {
                //�˴���δ��_y����������������ɣ��������������ǳ������������ɵ�
                float _v = Mathf.PerlinNoise((x + _seed) * biomeFreq, (x + _seed) * biomeFreq);
                //float _v = Mathf.PerlinNoise((x + seed) * _freq, (y + seed) * _freq);

                //����Gradientɫ��λ�õ���ɫ����������Ⱥϵ������
                Color _col = biomeSpread.Evaluate(_v);
                biomeMapTex.SetPixel(x, y, _col);

                //�洢��ɫ����˳��Ӧ������ȷ��
                if (System.Array.IndexOf(biomeColors, _col) == -1)
                    biomeColors[biomeColors.Length] = _col;
            }
        }
        //���²�������ʹ������Ч
        biomeMapTex.Apply();
        #endregion

        #region SpecificBiomes
        //����ͨ��seed�����Ĳ�ͬ���ӣ����Ⱥϵ��Ĳ����
        DrawBiomeSettingsTextures(2 * _seed, BiomeType.Grass);
        DrawBiomeSettingsTextures(3 * _seed, BiomeType.Desert);
        DrawBiomeSettingsTextures(4 * _seed, BiomeType.Snow);
        #endregion
    }

    private void DrawBiomeSettingsTextures(int _seed, BiomeType _biomeType)
    {
        int _bIdx = _biomeType.GetHashCode();
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].coalSpreadTex, biomes[_bIdx].coalRarity, biomes[_bIdx].coalSize);
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].ironSpreadTex, biomes[_bIdx].ironRarity, biomes[_bIdx].ironSize);
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].goldSpreadTex, biomes[_bIdx].goldRarity, biomes[_bIdx].goldSize);
        DrawPerlinNoiseTexture(_seed, ref biomes[_bIdx].diamondSpreadTex, biomes[_bIdx].diamondRarity, biomes[_bIdx].diamondSize);
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
                float _v = Mathf.PerlinNoise((x + _seed) * _freq, (y + _seed) * _freq);
                //��ĳ��[0,1]��Χ�ڵ���ֵ������ֵ_v�Ի��ֽ��ޣ���ɫ��Ϊ��Ѩ����ʯ��С���ȱ
                if (_v <= _size)
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
    //[Header("Biome Type")]
    //public BiomeType type;

    [Header("Ore Spread")]
    public Texture2D coalSpreadTex;         //ú�����ɵķֲ�����ͼ
    public Texture2D ironSpreadTex;         //�������ɵķֲ�����ͼ
    public Texture2D goldSpreadTex;         //������ɵķֲ�����ͼ
    public Texture2D diamondSpreadTex;      //������ɵķֲ�����ͼ

    [Header("Coal Settings")]
    public float coalRarity = 0.2f;         //ú��ϡȱ��
    public float coalSize = 0.18f;          //ú����С
    
    [Header("Iron Settings")]
    public float ironRarity = 0.18f;        //����ϡȱ��
    public float ironSize = 0.16f;          //������С
    
    [Header("Gold Settings")]
    public float goldRarity = 0.13f;        //���ϡȱ��
    public float goldSize = 0.11f;          //�����С
    
    [Header("Diamond Settings")]
    public float diamondRarity = 0.12f;     //���ϡȱ��
    public float diamondSize = 0.02f;       //�����С
}