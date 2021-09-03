using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    private Transform _transform;
    private Pathfinding _pathfinding;
    private Map _map;
    private List<Tile> currentPath;
    public Transform target;
    private void Update()
    {
        currentPath = _pathfinding.FindPath(transform.position, target.position);
    }

    private void Start()
    {
        _transform = transform;
        _pathfinding = FindObjectOfType<Pathfinding>();
        _map = FindObjectOfType<Map>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (currentPath != null)
        {
            foreach (Tile tile in currentPath)
            {
                Gizmos.DrawCube(new Vector3(tile.worldPosition.x, tile.worldPosition.y, -10f), new Vector3(.3f, .3f, .3f));
            }
        }

        Gizmos.color = Color.red;
        
        Gizmos.DrawSphere(new Vector3(target.position.x, target.position.y, -20f), target.localScale.x / 2);
    }
}
