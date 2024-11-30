using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : Manager<TerrainManager>
{
    [Header("Berlin Noise")]
    private float seed;                                     //������ɵ��������
    [SerializeField] private Texture2D caveNoiseTex;        //�洢���ɵĵ�ͼ��Ѩ����������ͼ��
    
    [Header("Terrain Shape")]
    [SerializeField] private float terrainFreq = 0.05f;     //����β�����صİ�������Ƶ��
    [SerializeField] private int heightMultiplier = 25;     //���ϰ���������Ϊ���κ������[0,~]�ڵ��������
    [SerializeField] private int heightAddition = 25;       //���εĻ������

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //������ĺ��

    [Header("Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //�Ƿ����ɶ�Ѩ
    [SerializeField] private float caveFreq = 0.05f;        //��ն����ֵ�Ƶ������صİ�������Ƶ��
    [SerializeField] private float caveThrehold = 0.2f;     //��ֵԽ��Խ������caveFreq������ϡ���Ҷ�Ѩ�ࣩ

    [Header("Trees")]
    [SerializeField] private float treeChance = 0.07f;      //��ľ�ڵر�ݵ������ɵĸ���
    public float TreeChance { get => treeChance; }
    [SerializeField] private int maxTreeHeight = 7;         //���ɵ����߶�
    public int MaxTreeHeight { get => maxTreeHeight; }
    [SerializeField] private int minTreeHeight = 4;         //���ɵ���С�߶�
    public int MinTreeHeight { get => minTreeHeight; }

    private void Start()
    {
        //�������һ�����ӣ�ʹ��ÿ�����ɵ���������ͼ��ͬ
        seed = UnityEngine.Random.Range(-10000, 10000);
        //���ɵ�ͼ�и���Ԫ�ص�����
        GenerateNoiseTexture(ref caveNoiseTex, caveFreq);

        //�������Ӻ��������ɵ���
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        //��ȫ��������ͼ������worldSize��worldSize����Ƭ
        //for (int x = 0; x < worldSize; x++)
        //{
        //    for (int y = 0; y < worldSize; y++)
        //    {
        //        if (noiseTexture.GetPixel(x, y).r < 0.5f)
        //        {
        //            //��(x,y)������һ����Ƭ
        //        }
        //    }
        //}

        //��ʵ�����ɵ���ǰ�ȳ�ʼ������
        TilemapManager.instance.InitTilemap();

        //ȡ��noiseTexture����ϵ�ĺ���y=PerlinNoise(f(x))���ߵ��·�������Ϊ����
        for (int _y = 0; _y < TilemapManager.instance.WorldLength; _y++)
        {
            for (int _x = 0; _x < TilemapManager.instance.WorldLength; _x++)
            {
                //��_x�����ڽ�ȡ������ͼ����������һ��[0,1]��Χ�ĵİ�������ֵ���ڴ˻���������һЩ�����������������Ҫ�İ�͹��ƽ�ĵ���
                float _height = Mathf.PerlinNoise((_x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

                //���ݸ߶����ò�ͬ�ĵ�����Ƭ��
                TileType _tileType;
                //��ʯ��
                if(_y < _height - dirtLayerHeight)
                    _tileType = TileType.Stone;
                //������
                else if (_y < _height - 1)
                    _tileType = TileType.Dirt;
                //�ݵز�
                else if (_y < _height)
                    _tileType = TileType.DirtGrass;
                //������
                else
                    _tileType = TileType.Air;

                //�����Ƿ����ɶ�Ѩ
                if (isGenerateCaves)
                {
                    //���ڵ�ĻҶȴ���ĳ��[0,1]��Χ�ڵ���ֵʱ�����ɸõ���Ƭ������noiseTexture�ǻҶ�ͼ������rgb���߾��ɣ�Խ��Խ�ӽ���ɫ��
                    if (caveNoiseTex.GetPixel(_x, _y).r > caveThrehold)
                        TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
                }
                else
                    TilemapManager.instance.PreSetTileAt(_tileType, _x, _y);
            }
        }

        //ʵ�ʸ����������õ���Ƭ����������Ƭ��ͼ
        TilemapManager.instance.GenerateTilemap();
    }

    private void GenerateNoiseTexture(ref Texture2D _noiseTex, float _freq)
    {
        //����������εı߳���ʼ������ͼ��
        _noiseTex = new Texture2D(TilemapManager.instance.WorldLength, TilemapManager.instance.WorldLength);
        //�����ؼ�������ֵ
        for (int x = 0; x < _noiseTex.width; x++)
        {
            for(int y = 0; y < _noiseTex.height; y++)
            {
                //����λ��(x,y)��������ӡ�Ƶ�ʣ�ʹ�ð�����������һ����[0,1]�������ֵ
                float _v = Mathf.PerlinNoise((x + seed) * _freq, (y + seed) * _freq);
                //�����ɵ�����ֵ��Ϊ�Ҷ�ֵ������ÿ������
                _noiseTex.SetPixel(x, y, new Color(_v, _v, _v));
            }
        }
        //���²�������ʹ������Ч
        _noiseTex.Apply();
    }
}
