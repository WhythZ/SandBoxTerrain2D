using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapManager : Manager<TilemapManager>
{
    #region Convenience
    private int chunkNumSqrt;
    #endregion

    [Header("Tile Prefabs")]
    [SerializeField] private List<GameObject> tilePrefabList;  //�����������Ƭ��Ԥ����

    [Header("Map Scale")]
    [SerializeField] private int worldLength = 128;            //����������ı߳���ͬʱҲ�Ƕ�Ӧ��������ͼ��ı߳���
    public int WorldLength { get => worldLength; }
    [SerializeField] private int chunkLength = 16;             //����������ı߳�

    [Header("Map Tiles")]
    [SerializeField] private GameObject[] chunks;              //�����������GameObject
    private TileType[] tileTypesBeforePlacing;                 //�������Ԥ���õ���Ƭ����
    [SerializeField] private GameObject[] tiles;               //���������ƬGameObject

    public void InitTilemap()
    {
        #region Chunks
        //������ȷ������������룬�����б�ռ����ڴ������GameObject��ע��ʵ����������Ϊƽ��
        chunkNumSqrt = (worldLength + chunkLength - 1) / chunkLength;
        chunks = new GameObject[chunkNumSqrt * chunkNumSqrt];
        //�������鲢ȷ�����ӹ�ϵ
        for (int i = 0; i < chunkNumSqrt * chunkNumSqrt; i++)
        {
            chunks[i] = new GameObject();
            chunks[i].name = i.ToString();
            chunks[i].transform.parent = this.transform;
        }
        #endregion

        #region Tiles
        tileTypesBeforePlacing = new TileType[worldLength * worldLength];
        tiles = new GameObject[worldLength * worldLength];
        #endregion
    }

    public void GenerateTilemap()
    {
        for (int _y = 0; _y < worldLength; _y++)
        {
            for (int _x = 0; _x < worldLength; _x++)
            {
                PlaceTileAt(tileTypesBeforePlacing[_x + _y * worldLength], _x, _y);
            }
        }
    }

    public void PreSetTileAt(TileType _type, int _x, int _y)
    {
        //���㴫�������Ӧ�������б��ж�Ӧ������
        int _tileIdx = _x + _y * WorldLength;

        //�������ڸ�λ���ϵ���Ƭ������ԭ�еĲ�ͬ���������Ҫ���и���
        if (_type != tileTypesBeforePlacing[_tileIdx])
        {
            //���������������и���
            if (_type == TileType.Air) return;
            if (_type == TileType.TreeLeaf && tileTypesBeforePlacing[_tileIdx] == TileType.TreeLog) return;
        }
        //ִ�и���
        tileTypesBeforePlacing[_tileIdx] = _type;

        #region DerivingStructures
        //�����ɲ�Ƥ��_yλ�ã�ʱ�����������Ϸ���������_y+1λ�ã�
        if (_type == TileType.DirtGrass) DeriveTreeAt(_x, _y + 1);
        #endregion
    }

    private void DeriveTreeAt(int _x, int _y)
    {
        //���������ǿ������ʵ�
        int _chance = Mathf.RoundToInt(TerrainManager.instance.TreeChance * 100);
        int _random = UnityEngine.Random.Range(0, 100);
        if (_random >= _chance) return;

        //Ԥ��������
        int _height = UnityEngine.Random.Range(TerrainManager.instance.MinTreeHeight, TerrainManager.instance.MaxTreeHeight);
        for (int i = 0; i < _height; i++)
            PreSetTileAt(TileType.TreeLog, _x, _y + i);

        //Ԥ������Ҷ
        PreSetTileAt(TileType.TreeLeaf, _x, _y + _height);
        PreSetTileAt(TileType.TreeLeaf, _x - 1, _y + _height);
        PreSetTileAt(TileType.TreeLeaf, _x + 1, _y + _height);
        PreSetTileAt(TileType.TreeLeaf, _x, _y + _height + 1);
        PreSetTileAt(TileType.TreeLeaf, _x - 1, _y + _height + 1);
        PreSetTileAt(TileType.TreeLeaf, _x + 1, _y + _height + 1);
        PreSetTileAt(TileType.TreeLeaf, _x, _y + _height + 2);
    }

    private void PlaceTileAt(TileType _type, int _x, int _y)
    {
        //������λ�ó��������緶Χ����������
        if (_x >= worldLength || _y >= worldLength) return;

        #region Instantiation
        //ʵ������Ӧ������Ƭ��Ԥ���岢����
        GameObject _newTile = Instantiate(tilePrefabList[_type.GetHashCode()]);
        _newTile.name = _type.ToString();

        //����ɢ������_x��_yΪ�������꣬����ʵ������Ƭ�����λ��
        _newTile.transform.position = new Vector2(_x, _y);
        #endregion

        #region AddToChunksArray
        //�������Ƭ���ڵ������ŵ�����ֵ
        int _chunkIdx = (_y / chunkLength) * chunkNumSqrt + (_x / chunkLength);
        //����Ƭ��������ȷ���������Ա����
        _newTile.transform.parent = chunks[_chunkIdx].transform;
        #endregion

        #region AddToTilesList
        tiles[_x + _y * worldLength] = _newTile;
        #endregion
    }
}

public enum TileType
{
    Air = 0,
    Stone = 1,
    Dirt = 2,
    DirtGrass = 3,
    TreeLog = 4,
    TreeLeaf = 5,
    Coal = 6,
    Iron = 7,
    Gold = 8,
    Diamond = 9
}