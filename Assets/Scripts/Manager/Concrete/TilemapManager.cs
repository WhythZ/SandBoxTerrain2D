using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapManager : Manager<TilemapManager>
{
    #region Convenience
    private int chunkNumSqrt;
    #endregion

    [Header("Tile Prefabs")]
    [SerializeField] private List<GameObject> tilePrefabList;  //所有种类的瓦片的预制体

    [Header("Map Scale")]
    [SerializeField] private int worldLength = 128;            //正方形世界的边长（同时也是对应噪声材质图像的边长）
    public int WorldLength { get => worldLength; }
    [SerializeField] private int chunkLength = 16;             //正方形区块的边长

    [Header("Map Tiles")]
    [SerializeField] private GameObject[] chunks;              //存放所有区块GameObject
    private TileType[] tileTypesBeforePlacing;                 //存放所有预放置的瓦片类型
    [SerializeField] private GameObject[] tiles;               //存放所有瓦片GameObject

    public void InitTilemap()
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
        //计算传入坐标对应在线性列表中对应的索引
        int _tileIdx = _x + _y * WorldLength;

        //若设置在该位置上的瓦片类型与原有的不同，则可能需要进行覆盖
        if (_type != tileTypesBeforePlacing[_tileIdx])
        {
            //以下情况下无需进行覆盖
            if (_type == TileType.Air) return;
            if (_type == TileType.TreeLeaf && tileTypesBeforePlacing[_tileIdx] == TileType.TreeLog) return;
        }
        //执行覆盖
        tileTypesBeforePlacing[_tileIdx] = _type;

        #region DerivingStructures
        //在生成草皮（_y位置）时按概率在其上方生成树（_y+1位置）
        if (_type == TileType.DirtGrass) DeriveTreeAt(_x, _y + 1);
        #endregion
    }

    private void DeriveTreeAt(int _x, int _y)
    {
        //树的生成是考究概率的
        int _chance = Mathf.RoundToInt(TerrainManager.instance.TreeChance * 100);
        int _random = UnityEngine.Random.Range(0, 100);
        if (_random >= _chance) return;

        //预设置树干
        int _height = UnityEngine.Random.Range(TerrainManager.instance.MinTreeHeight, TerrainManager.instance.MaxTreeHeight);
        for (int i = 0; i < _height; i++)
            PreSetTileAt(TileType.TreeLog, _x, _y + i);

        //预设置树叶
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
        //若放置位置超出了世界范围，则不予生成
        if (_x >= worldLength || _y >= worldLength) return;

        #region Instantiation
        //实例化对应类型瓦片的预制体并命名
        GameObject _newTile = Instantiate(tilePrefabList[_type.GetHashCode()]);
        _newTile.name = _type.ToString();

        //以离散的整数_x与_y为世界坐标，设置实例化瓦片对象的位置
        _newTile.transform.position = new Vector2(_x, _y);
        #endregion

        #region AddToChunksArray
        //计算该瓦片所在的区块编号的索引值
        int _chunkIdx = (_y / chunkLength) * chunkNumSqrt + (_x / chunkLength);
        //将瓦片挂载在正确的区块上以便管理
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