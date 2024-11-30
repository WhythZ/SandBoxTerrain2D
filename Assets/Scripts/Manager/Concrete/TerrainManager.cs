using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : Manager<TerrainManager>
{
    [Header("Berlin Noise")]
    [SerializeField] private int seed;                      //������ɵ��������
    [SerializeField] private Texture2D caveSpreadTex;       //�洢���ɵĵ�ͼ��Ѩ������ͼ
    [SerializeField] private Texture2D coalSpreadTex;       //ú�����ɵķֲ�����ͼ
    [SerializeField] private Texture2D ironSpreadTex;       //�������ɵķֲ�����ͼ
    [SerializeField] private Texture2D goldSpreadTex;       //������ɵķֲ�����ͼ
    [SerializeField] private Texture2D diamondSpreadTex;    //������ɵķֲ�����ͼ

    [Header("Terrain Shape")]
    [SerializeField] private float terrainRelief = 0.05f;   //����ε������صİ�������Ƶ��
    [SerializeField] private int heightMultiplier = 35;     //���ϰ���������Ϊ���κ������[0,~]�ڵ��������
    [SerializeField] private int heightAddition = 50;       //���εĻ������

    [Header("Ore Settings")]
    [SerializeField] private float coalRarity = 0.2f;       //ú��ϡȱ��
    [SerializeField] private float coalSize = 0.18f;        //ú����С
    [SerializeField] private float ironRarity = 0.18f;      //����ϡȱ��
    [SerializeField] private float ironSize = 0.16f;        //������С
    [SerializeField] private float goldRarity = 0.13f;      //���ϡȱ��
    [SerializeField] private float goldSize = 0.11f;        //�����С
    [SerializeField] private float diamondRarity = 0.12f;   //���ϡȱ��
    [SerializeField] private float diamondSize = 0.02f;     //�����С

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //������ĺ��

    [Header("Cave Settings")]
    [SerializeField] private bool isGenerateCaves = false;  //�Ƿ����ɶ�Ѩ
    [SerializeField] private float caveFreq = 0.08f;        //��ն����ֵ�Ƶ������صİ�������Ƶ��
    [SerializeField] private float caveSize = 0.2f;         //��ֵԽ��Խ������caveFreq����Ѩ�ࣩ

    [Header("Tree Settings")]
    [SerializeField] private float treeChance = 0.07f;      //��ľ�ڵر�ݵ������ɵĸ���
    public float TreeChance { get => treeChance; }
    [SerializeField] private int maxTreeHeight = 7;         //���ɵ����߶�
    public int MaxTreeHeight { get => maxTreeHeight; }
    [SerializeField] private int minTreeHeight = 4;         //���ɵ���С�߶�
    public int MinTreeHeight { get => minTreeHeight; }

    public void GenerateTerrain(int _seed)
    {
        #region NoisesGeneration
        //���������ӣ�����������
        seed = _seed;
        //���ɵ�ͼ�и���Ԫ�ص�����
        GenerateNoiseTexture(ref caveSpreadTex, caveFreq, caveSize);
        GenerateNoiseTexture(ref coalSpreadTex, coalRarity, coalSize);
        GenerateNoiseTexture(ref ironSpreadTex, ironRarity, ironSize);
        GenerateNoiseTexture(ref goldSpreadTex, goldRarity, goldSize);
        GenerateNoiseTexture(ref diamondSpreadTex, diamondRarity, diamondSize);
        #endregion

        //��ʵ�����ɵ���ǰ�ȳ�ʼ������
        TilemapManager.instance.InitTilemap();

        //ȡ��noiseTexture����ϵ�ĺ���y=PerlinNoise(f(x))���ߵ��·�������Ϊ����
        for (int _y = 0; _y < TilemapManager.instance.WorldLength; _y++)
        {
            for (int _x = 0; _x < TilemapManager.instance.WorldLength; _x++)
            {
                //��_x�����ڽ�ȡ������ͼ����������һ��[0,1]��Χ�ĵİ�������ֵ���ڴ˻���������һЩ�����������������Ҫ�İ�͹��ƽ�ĵ���
                float _height = Mathf.PerlinNoise((_x + seed) * terrainRelief, seed * terrainRelief) * heightMultiplier + heightAddition;
                //���ݸ߶����ò�ͬ�ĵ�����Ƭ��
                TileType _tileType;

                //������ʯ�㣨����ʯ��
                if (_y < _height - dirtLayerHeight)
                {
                    //ע��˴����Ⱥ�����˳����ϡȱ�������ȱ�����
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
                //����������
                else if (_y < _height - 1)
                    _tileType = TileType.Dirt;
                //���òݵز�
                else if (_y < _height)
                    _tileType = TileType.DirtGrass;
                //���ÿ�����
                else
                    _tileType = TileType.Air;

                //�����Ƿ����ɶ�Ѩ
                if (isGenerateCaves && caveSpreadTex.GetPixel(_x, _y) == Color.white)
                        _tileType = TileType.Air;

                //�������øõ㴦����Ƭ
                TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
            }
        }

        //ʵ�ʸ�������Ԥ���õ���Ƭ����������Ƭ��ͼ
        TilemapManager.instance.GenerateTilemap();
    }

    private void GenerateNoiseTexture(ref Texture2D _noiseTex, float _freq, float _threhold)
    {
        //����������εı߳���ʼ������ͼ��
        _noiseTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        //�����ؼ�������ֵ
        for (int x = 0; x < _noiseTex.width; x++)
        {
            for(int y = 0; y < _noiseTex.height; y++)
            {
                //����λ��(x,y)��������ӡ�Ƶ�ʣ�ʹ�ð�����������һ����[0,1]�������ֵ����Ϊrgb�Ļ�Խ��Խ�ӽ���ɫ��
                float _v = Mathf.PerlinNoise((x + seed) * _freq, (y + seed) * _freq);
                //��ĳ��[0,1]��Χ�ڵ���ֵ������ֵ_v�Ի��ֽ��ޣ���ɫ��Ϊ��Ѩ����ʯ��С���ȱ
                if (_v <= _threhold)
                    _noiseTex.SetPixel(x, y, Color.white);
                else
                    _noiseTex.SetPixel(x, y, Color.black);
            }
        }
        //���²�������ʹ������Ч
        _noiseTex.Apply();
    }
}
