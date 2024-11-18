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
    [SerializeField] private Sprite treeLeaf;               //��Ҷ��Ƭ
    [SerializeField] private Sprite treeLog;                //������Ƭ
    [SerializeField] private Sprite grass;                  //ʯͷ��Ƭ
    [SerializeField] private Sprite dirt;                   //������Ƭ
    [SerializeField] private Sprite stone;                  //ʯͷ��Ƭ

    [Header("Terrain Size")]
    [SerializeField] private int worldSize = 100;           //��������������ͼ��ı߳�
    [SerializeField] private int heightMultiplier = 25;     //Ϊ���κ������[0,5]���������
    [SerializeField] private int heightAddition = 25;       //���εĻ������

    //[Header("Terrain Tiles")]
    //[SerializeField] private List<>

    [Header("Terrain Shape")]
    [SerializeField] private float surfaceThrehold = 0.2f;  //��ֵԽ�󣬵���Խϡ�裬Խ������caveFreq����ѨԽ��
    [SerializeField] private float terrainFreq = 0.05f;     //����β�����صİ�������Ƶ��

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //������ĺ��

    [Header("Terrain Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //�Ƿ����ɶ�Ѩ
    [SerializeField] private float caveFreq = 0.05f;        //��ն����ֵ�Ƶ������صİ�������Ƶ��

    [Header("Surface Trees")]
    [SerializeField] private float treeChance = 0.07f;      //��ľ�ڵر�ݵ������ɵĸ���
    [SerializeField] private int maxTreeHeight = 7;         //���ɵ����߶�
    [SerializeField] private int minTreeHeight = 4;         //���ɵ���С�߶�

    [Header("Berlin Noise")]
    [SerializeField] private Texture2D noiseTexture;        //�洢���ɵ���������ͼ��
    private float seed;                                     //������ɵ��������

    private void Start()
    {
        //�������һ�����ӣ�ʹ��ÿ�����ɵ���������ͼ��ͬ
        seed = UnityEngine.Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        //��ȫ����noiseTexture����100x100��һ���Tile
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

        //ȡ��noiseTexture����ϵ�ĺ���y=PerlinNoise(f(x))���ߵ��·�������Ϊ����
        for (int _x = 0; _x < worldSize; _x++)
        {
            //��x�Խ�ȡ�ĸ߶Ⱥ�����������һ��[0,1]��Χ�ĵİ�������ֵ���ڴ˻���������һЩ�����������������Ҫ�İ�͹��ƽ�ĵ���
            float _height = Mathf.PerlinNoise((_x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;
            for (int _y = 0; _y < _height; _y++)
            {
                //���ݸ߶����ò�ͬ�ĵ�����Ƭ��
                Sprite _sprite;
                //�ݵز�
                if (_y > _height - 1)
                    _sprite = grass;
                //������
                else if (_y > _height - dirtLayerHeight)
                    _sprite = dirt;
                //ʯͷ��
                else
                    _sprite = stone;

                //�����Ƿ����ɶ�Ѩ
                if (isGenerateCaves)
                {
                    //���ڵ�ĻҶȴ���ĳ��[0,1]��Χ�ڵ���ֵʱ�����ɸõ���Ƭ������noiseTexture�ǻҶ�ͼ������rgb���߾��ɣ�Խ��Խ�ӽ���ɫ��
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
        //���ڶ�Ӧλ�÷���ש�飬Ȼ�����ש����ܸ������ɵ���������
        PlaceTileAt(_sprite,_x, _y);

        //�ڲݵ��ϰ�������������ע�⴫���������y+1��y�ͻ�ֱ�Ӹ��ǵ���Ƥ��
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
        //��������
        int _height = UnityEngine.Random.Range(minTreeHeight, maxTreeHeight);
        for (int i = 0; i < _height; i++)
            PlaceTileAt(treeLog, _x, _y + i);

        //������Ҷ
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
        //ƫ����0.5f��Ϊ��ȷ����Unity�������غϣ��ÿ�һ��
        _newTile.transform.position = new Vector2(_x + 0.5f, _y + 0.5f);
        _newTile.AddComponent<SpriteRenderer>();
        _newTile.GetComponent<SpriteRenderer>().sprite = _sprite;
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
