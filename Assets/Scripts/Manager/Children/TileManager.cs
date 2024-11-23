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

public class TileManager : Manager<TileManager>
{
    [Header("Tile Prefabs")]
    //所有种类的瓦片的预制体
    [SerializeField] private List<GameObject> tilePrefabDict;

    [Header("Scene Tiles")]
    //所有处于当前场景内的瓦片实例
    [SerializeField] public List<GameObject> tileList;

    public void GenerateTileAt(TileType _type, int _x, int _y)
    {
        //先在对应位置放置砖块，然后检测该砖块可能附带生成的其他东西
        PlaceTileAt(_type, _x, _y);

        //在生成草皮（y位置）时按概率在其上方生成树（y+1位置）
        if (_type == TileType.DirtGrass)
        {
            int _chance = Mathf.RoundToInt(TerrainManager.instance.TreeChance * 100);
            int _random = UnityEngine.Random.Range(0, 100);
            if (_random < _chance)
                PlaceTreeAt(_x, _y + 1);
        }
    }

    public void PlaceTileAt(TileType _type, int _x, int _y)
    {
        GameObject _newTile = Instantiate(tilePrefabDict[_type.GetHashCode()]);

        //设置实例化的瓦片对象的父级与位置（_x与_y是离散的整数世界坐标，偏移量0.5f确保和网格重合，好看一些）
        _newTile.transform.parent = this.transform;
        _newTile.transform.position = new Vector2(_x + 0.5f, _y + 0.5f);

        _newTile.name = _type.ToString();
    }

    private void PlaceTreeAt(int _x, int _y)
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
}
