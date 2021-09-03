using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{    
    [NonSerialized] public Vector2Int gridPosition;
    [NonSerialized]
    public Vector3 worldPosition;
    public bool walkable;
    
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public Tile parentTile;

    public List<Tile> neighbourTiles;

}
