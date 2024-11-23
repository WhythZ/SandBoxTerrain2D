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

    [Header("Texture")]
    [SerializeField] protected Sprite defaultTexture;

    protected virtual void Start()
    {
        #region Components
        //cd = GetComponent<BoxCollider2D>();
        //rb = GetComponent<Rigidbody2D>();
        //anim = GetComponent<Animator>(); 
        #endregion

        //³õÊ¼»¯ÍßÆ¬²ÄÖÊ
        GetComponent<SpriteRenderer>().sprite = defaultTexture;
    }
}
