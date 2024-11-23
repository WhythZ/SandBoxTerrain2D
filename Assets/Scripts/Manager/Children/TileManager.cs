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
    //�����������Ƭ��Ԥ����
    [SerializeField] private List<GameObject> tilePrefabDict;

    [Header("Scene Tiles")]
    //���д��ڵ�ǰ�����ڵ���Ƭʵ��
    [SerializeField] public List<GameObject> tileList;

    public void GenerateTileAt(TileType _type, int _x, int _y)
    {
        //���ڶ�Ӧλ�÷���ש�飬Ȼ�����ש����ܸ������ɵ���������
        PlaceTileAt(_type, _x, _y);

        //�����ɲ�Ƥ��yλ�ã�ʱ�����������Ϸ���������y+1λ�ã�
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

        //����ʵ��������Ƭ����ĸ�����λ�ã�_x��_y����ɢ�������������꣬ƫ����0.5fȷ���������غϣ��ÿ�һЩ��
        _newTile.transform.parent = this.transform;
        _newTile.transform.position = new Vector2(_x + 0.5f, _y + 0.5f);

        _newTile.name = _type.ToString();
    }

    private void PlaceTreeAt(int _x, int _y)
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
}
