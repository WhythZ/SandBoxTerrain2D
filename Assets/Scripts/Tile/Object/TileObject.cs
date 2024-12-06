using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileObject : MonoBehaviour 
{
    #region Components
    //protected BoxCollider2D cd;
    //protected Rigidbody2D rb;
    //protected Animator anim;
    #endregion

    [Header("Type")]
    [SerializeField] protected TileType type;
    public TileType Type { get => type; }

    [Header("Texture")]
    [SerializeField] protected Sprite[] textures;

    protected virtual void Start()
    {
        #region Components
        //cd = GetComponent<BoxCollider2D>();
        //rb = GetComponent<Rigidbody2D>();
        //anim = GetComponent<Animator>(); 
        #endregion

        //初始化瓦片材质为任意一种
        GetComponentInParent<SpriteRenderer>().sprite = textures[UnityEngine.Random.Range(0, textures.Length)];
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
    Diamond = 9,
    Sand = 10,
    Snow = 11
}