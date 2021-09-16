using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{    
    [NonSerialized] public Vector2Int gridPosition;
    [NonSerialized] public Vector3 worldPosition;
    public bool walkable;

    [NonSerialized] public int gCost;
    [NonSerialized] public int hCost;
    public int fCost => gCost + hCost;
    [NonSerialized] public Tile parentTile;
    public List<Tile> neighbourTiles;
}
