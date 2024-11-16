using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private int worldSize = 100;          //��������������ͼ��ı߳�
    [SerializeField] private Texture2D noiseTexture;       //�洢���ɵ���������ͼ��
    [SerializeField] private float seed;                   //������ɵ��������
    [SerializeField] private float caveFreq = 0.05f;       //��ն����ֵ�Ƶ������صİ�������Ƶ��
    
    [SerializeField] private Sprite tile;                  //��Ƭ��ͼ�����
    [SerializeField] private float surfaceValue = 0.2f;    //��ֵԽ�󣬵���Խϡ��
    [SerializeField] private float terrainFreq = 0.05f;    //����β�����صİ�������Ƶ��
    [SerializeField] private int heightMultiplier = 25;    //Ϊ���κ������[0,5]���������
    [SerializeField] private int heightAddition = 25;      //���εĻ������

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
                //���ڵ�ĻҶȴ���ĳ��[0,1]��Χ�ڵ���ֵʱ�������ɸõ����Ƭ����ֵԽ��Խ�ӽ���ɫ�����ɵ���ƬԽϡ��
                //����noiseTexture�ǻҶ�ͼ������r��g��b���߾���
                if (noiseTexture.GetPixel(x, y).r > surfaceValue)
                {
                    GameObject _newTile = new GameObject();
                    _newTile.name = "Tile";
                    _newTile.transform.parent = this.transform;
                    _newTile.AddComponent<SpriteRenderer>();
                    _newTile.GetComponent<SpriteRenderer>().sprite = tile;
                    //ƫ����0.5f��Ϊ��ȷ����Unity�������غϣ��ÿ�һ��
                    _newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
                }
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
