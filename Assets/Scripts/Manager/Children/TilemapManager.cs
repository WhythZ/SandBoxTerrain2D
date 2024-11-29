using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    None = 0,
    Stone = 1,
    Dirt = 2,
    DirtGrass = 3,
    TreeLog = 4,
    TreeLeaf = 5
}

public class TilemapManager : Manager<TilemapManager>
{
    #region ClassFields
    [Header("Terrain Scale")]
    [SerializeField] private int worldLength = 128;            //����������ı߳���ͬʱҲ�Ƕ�Ӧ��������ͼ��ı߳���

    [Header("Terrain Chunks")]
    [SerializeField] private int chunkLength = 16;             //����������ı߳�
    [SerializeField] public GameObject[] chunks;           //�����������GameObject���б�

    [Header("Tile Prefabs")]
    [SerializeField] private List<GameObject> tilePrefabList;  //�����������Ƭ��Ԥ����
    #endregion

    #region ClassProperties
    public int WorldLength { get => worldLength; }
    public int ChunkLength { get => chunkLength; }
    #endregion

    #region Convenience
    private int chunkNumSqrt;
    #endregion

    public void InitChunks()
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
    }

    public void GenerateTileAt(TileType _type, int _x, int _y)
    {
        //�������None�������ڶ�Ӧλ�÷���ש�飬Ȼ�����ש����ܸ������ɵ���������
        if (_type != TileType.None)
            PlaceTileAt(_type, _x, _y);

        //�����ɲ�Ƥ��yλ�ã�ʱ�����������Ϸ���������y+1λ�ã�
        if (_type == TileType.DirtGrass)
        {
            int _chance = Mathf.RoundToInt(TerrainManager.instance.TreeChance * 100);
            int _random = UnityEngine.Random.Range(0, 100);
            if (_random < _chance)
                GenerateTreeAt(_x, _y + 1);
        }
    }

    private void GenerateTreeAt(int _x, int _y)
    {
        //��������
        int _height = UnityEngine.Random.Range(TerrainManager.instance.MinTreeHeight, TerrainManager.instance.MaxTreeHeight);
        for (int i = 0; i < _height; i++)
            PlaceTileAt(TileType.TreeLog, _x, _y + i);

        //������Ҷ
        PlaceTileAt(TileType.TreeLeaf, _x, _y + _height);
        PlaceTileAt(TileType.TreeLeaf, _x - 1, _y + _height);
        PlaceTileAt(TileType.TreeLeaf, _x + 1, _y + _height);
        PlaceTileAt(TileType.TreeLeaf, _x, _y + _height + 1);
        PlaceTileAt(TileType.TreeLeaf, _x - 1, _y + _height + 1);
        PlaceTileAt(TileType.TreeLeaf, _x + 1, _y + _height + 1);
        PlaceTileAt(TileType.TreeLeaf, _x, _y + _height + 2);
    }

    private void PlaceTileAt(TileType _type, int _x, int _y)
    {
        //ʵ������Ӧ������Ƭ��Ԥ���岢����
        GameObject _newTile = Instantiate(tilePrefabList[_type.GetHashCode()]);
        _newTile.name = _type.ToString();

        //����ʵ��������Ƭ�����λ�ã�_x��_y����ɢ�������������꣬ƫ����0.5fȷ���������غϣ��ÿ�һЩ��
        _newTile.transform.position = new Vector2(_x + 0.5f, _y + 0.5f);

        //�������Ƭ���ڵ������ŵ�����ֵ
        int _chunkIdx = (_y / chunkLength) * chunkNumSqrt + (_x / chunkLength);

        Debug.Log(_chunkIdx);

        //����Ƭ��������ȷ���������Ա����
        _newTile.transform.parent = chunks[_chunkIdx].transform;
    }
}
