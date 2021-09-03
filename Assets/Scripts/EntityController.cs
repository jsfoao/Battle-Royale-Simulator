using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    private Transform _transform;
    private Pathfinding _pathfinding;
    private Map _map;
    private List<Tile> currentPath;
    public Transform target;

    [SerializeField] private float speed;

    private IEnumerator MoveAlongPath(List<Tile> tilePath)
    {
        Debug.Log("moving along path...");
        int pathIndex = 0;
        Vector3 currentTarget = tilePath[pathIndex].worldPosition;
        while (true)
        {
            if (_transform.position == currentTarget)
            {
                pathIndex++;
                if (pathIndex >= tilePath.Count)
                {
                    Debug.Log("no more tiles");
                    yield break;
                }
                currentTarget = tilePath[pathIndex].worldPosition;
                Debug.Log("next tile");
            }

            _transform.position = Vector2.MoveTowards(_transform.position, currentTarget, speed);
            yield return null;
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPath = _pathfinding.FindPath(transform.position, target.position);
            StartCoroutine(MoveAlongPath(currentPath));
        }
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
                Gizmos.DrawCube(new Vector3(tile.worldPosition.x, tile.worldPosition.y, 10f), new Vector3(.3f, .3f, .3f));
            }
        }
    }
}
