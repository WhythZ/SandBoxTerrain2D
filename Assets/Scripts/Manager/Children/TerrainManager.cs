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
    [SerializeField] private int worldSize = 100;           //��������������ͼ��ı߳�
    [SerializeField] private int heightMultiplier = 25;     //Ϊ���κ������[0,5]���������
    [SerializeField] private int heightAddition = 25;       //���εĻ������

    [Header("Terrain Shape")]
    [SerializeField] private float surfaceThrehold = 0.2f;  //��ֵԽ��Խ����caveFreq������Խϡ���Ҷ�ѨԽ��
    [SerializeField] private float terrainFreq = 0.05f;     //����β�����صİ�������Ƶ��

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //������ĺ��

    [Header("Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //�Ƿ����ɶ�Ѩ
    [SerializeField] private float caveFreq = 0.05f;        //��ն����ֵ�Ƶ������صİ�������Ƶ��

    [Header("Trees")]
    [SerializeField] private float treeChance = 0.07f;      //��ľ�ڵر�ݵ������ɵĸ���
    [SerializeField] private int maxTreeHeight = 7;         //���ɵ����߶�
    [SerializeField] private int minTreeHeight = 4;         //���ɵ���С�߶�

    [Header("Berlin Noise")]
    [SerializeField] private Texture2D noiseTexture;        //�洢���ɵ���������ͼ��
    private float seed;                                     //������ɵ��������
    #endregion

    #region ClassProperties
    public float TreeChance { get => treeChance; }
    public int MaxTreeHeight { get => maxTreeHeight; }
    public int MinTreeHeight { get => minTreeHeight; }
    #endregion

    private void Start()
    {
        //�������һ�����ӣ�ʹ��ÿ�����ɵ���������ͼ��ͬ
        seed = UnityEngine.Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        //��ȫ��������ͼ������100x100����Ƭ
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

        //ȡ��noiseTexture����ϵ�ĺ���y=PerlinNoise(f(x))���ߵ��·�������Ϊ����
        for (int _x = 0; _x < worldSize; _x++)
        {
            //��x�Խ�ȡ�ĸ߶Ⱥ�����������һ��[0,1]��Χ�ĵİ�������ֵ���ڴ˻���������һЩ�����������������Ҫ�İ�͹��ƽ�ĵ���
            float _height = Mathf.PerlinNoise((_x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;
            for (int _y = 0; _y < _height; _y++)
            {
                //���ݸ߶����ò�ͬ�ĵ�����Ƭ��
                TileType _tileType;
                //�ݵز�
                if (_y > _height - 1)
                    _tileType = TileType.DirtGrass;
                //������
                else if (_y > _height - dirtLayerHeight)
                    _tileType = TileType.Dirt;
                //ʯͷ��
                else
                    _tileType = TileType.Stone;

                //�����Ƿ����ɶ�Ѩ
                if (isGenerateCaves)
                {
                    //���ڵ�ĻҶȴ���ĳ��[0,1]��Χ�ڵ���ֵʱ�����ɸõ���Ƭ������noiseTexture�ǻҶ�ͼ������rgb���߾��ɣ�Խ��Խ�ӽ���ɫ��
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

        //�����ؼ�������ֵ
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for(int y = 0; y < noiseTexture.height; y++)
            {
                //����λ��(x,y)��������ӡ�Ƶ�ʣ�ʹ�ð�����������һ����[0,1]�������ֵ
                float _v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                //�����ɵ�����ֵ��Ϊ�Ҷ�ֵ������ÿ������
                noiseTexture.SetPixel(x, y, new Color(_v, _v, _v));
            }
        }

        //���²�������ʹ������Ч
        noiseTexture.Apply();
    }
}
