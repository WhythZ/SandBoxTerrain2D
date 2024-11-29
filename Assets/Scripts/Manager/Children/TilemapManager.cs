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
    [SerializeField] private int worldLength = 128;            //正方形世界的边长（同时也是对应噪声材质图像的边长）

    [Header("Terrain Chunks")]
    [SerializeField] private int chunkLength = 16;             //正方形区块的边长
    [SerializeField] public GameObject[] chunks;           //存放所有区块GameObject的列表

    [Header("Tile Prefabs")]
    [SerializeField] private List<GameObject> tilePrefabList;  //所有种类的瓦片的预制体
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
        //被除数确保结果向上舍入，开辟列表空间用于存放区块GameObject，注意实际区块总数为平方
        chunkNumSqrt = (worldLength + chunkLength - 1) / chunkLength;
        chunks = new GameObject[chunkNumSqrt * chunkNumSqrt];
        //建立区块并确立父子关系
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
        //如果不是None，就先在对应位置放置砖块，然后检测该砖块可能附带生成的其他东西
        if (_type != TileType.None)
            PlaceTileAt(_type, _x, _y);

        //在生成草皮（y位置）时按概率在其上方生成树（y+1位置）
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
        //生成树干
        int _height = UnityEngine.Random.Range(TerrainManager.instance.MinTreeHeight, TerrainManager.instance.MaxTreeHeight);
        for (int i = 0; i < _height; i++)
            PlaceTileAt(TileType.TreeLog, _x, _y + i);

        //生成树叶
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
        //实例化对应类型瓦片的预制体并命名
        GameObject _newTile = Instantiate(tilePrefabList[_type.GetHashCode()]);
        _newTile.name = _type.ToString();

        //设置实例化的瓦片对象的位置（_x与_y是离散的整数世界坐标，偏移量0.5f确保和网格重合，好看一些）
        _newTile.transform.position = new Vector2(_x + 0.5f, _y + 0.5f);

        //计算该瓦片所在的区块编号的索引值
        int _chunkIdx = (_y / chunkLength) * chunkNumSqrt + (_x / chunkLength);

        Debug.Log(_chunkIdx);

        //将瓦片挂载在正确的区块上以便管理
        _newTile.transform.parent = chunks[_chunkIdx].transform;
    }
}
