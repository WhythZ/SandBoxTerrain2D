using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile Sprites")]
    [SerializeField] private Sprite grass;                  //ʯͷ��Ƭ
    [SerializeField] private Sprite dirt;                   //������Ƭ
    [SerializeField] private Sprite stone;                  //ʯͷ��Ƭ

    [Header("Terrain Size")]
    [SerializeField] private int worldSize = 100;           //��������������ͼ��ı߳�
    [SerializeField] private int heightMultiplier = 25;     //Ϊ���κ������[0,5]���������
    [SerializeField] private int heightAddition = 25;       //���εĻ������

    [Header("Terrain Shape")]
    [SerializeField] private float surfaceThrehold = 0.2f;  //��ֵԽ�󣬵���Խϡ�裬Խ������caveFreq����ѨԽ��
    [SerializeField] private float terrainFreq = 0.05f;     //����β�����صİ�������Ƶ��

    [Header("Terrain Layer")]
    [SerializeField] private int dirtLayerHeight = 5;       //������ĺ��

    [Header("Terrain Caves")]
    [SerializeField] private bool isGenerateCaves = false;  //�Ƿ����ɶ�Ѩ
    [SerializeField] private float caveFreq = 0.05f;        //��ն����ֵ�Ƶ������صİ�������Ƶ��

    [Header("Berlin Noise")]
    [SerializeField] private Texture2D noiseTexture;       //�洢���ɵ���������ͼ��
    private float seed;                                    //������ɵ��������

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
        for (int x = 0; x < worldSize; x++)
        {
            //��x�Խ�ȡ�ĸ߶Ⱥ�����������һ��[0,1]��Χ�ĵİ�������ֵ���ڴ˻���������һЩ�����������������Ҫ�İ�͹��ƽ�ĵ���
            float _height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;
            for (int y = 0; y < _height; y++)
            {
                //���ݸ߶����ò�ͬ�ĵ�����Ƭ��
                Sprite _sprite;
                if (y > _height - 1)
                    _sprite = grass;
                else if (y > _height - dirtLayerHeight)
                    _sprite = dirt;
                else
                    _sprite = stone;

                //�����Ƿ����ɶ�Ѩ
                if (isGenerateCaves)
                {
                    //���ڵ�ĻҶȴ���ĳ��[0,1]��Χ�ڵ���ֵʱ�����ɸõ���Ƭ������noiseTexture�ǻҶ�ͼ������rgb���߾��ɣ�Խ��Խ�ӽ���ɫ��
                    if (noiseTexture.GetPixel(x, y).r > surfaceThrehold)
                        PlaceTile(_sprite, x, y);
                }
                else
                    PlaceTile(_sprite, x, y);
            }
        }
    }

    private void PlaceTile(Sprite _sprite, int _x, int _y)
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
