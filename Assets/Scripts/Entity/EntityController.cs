using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityController : MonoBehaviour
{
    private Transform _transform;
    private Pathfinding _pathfinding;
    [NonSerialized] public Map map;
    private List<Tile> currentPath;
    [NonSerialized] public Transform target;
    
    [SerializeField] private float sprintSpeed = 0.05f;
    [SerializeField] private float walkSpeed = 0.02f;
    
    [SerializeField] private float rotationSpeed = 8f;
    public Coroutine movement;
    public bool moving;
    private float targetTime = 3f;
    private float currentTime;
    
    // Moves to random target in designated area
    public void MoveToRandomTarget(Rect area)
    {
        target.position = new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax),
            0f);
        
        if (map.TileFromWorldPosition(target.position).walkable == false) { return; }

        MoveToTarget(target);
    }
    
    public void MoveToTarget(Transform target)
    {
        if (movement != null) { StopCoroutine(movement); }

        if (map.TileFromWorldPosition(_transform.position) == map.TileFromWorldPosition(target.position)) { return; }

        currentPath = _pathfinding.FindPath(_transform.position, target.position);
        movement = StartCoroutine(MoveAlongPath(currentPath));
    }
    
    public void LookAtTarget(Vector3 target, float speed)
    {
        Vector3 direction = _transform.position - target;
        _transform.right = Vector3.Lerp(_transform.right, -direction, Time.deltaTime * speed);
    }

    public IEnumerator MoveAlongPath(List<Tile> tilePath)
    {
        int pathIndex = 0;
        Vector3 currentTarget = tilePath[pathIndex].worldPosition;
        while (true)
        {
            moving = true;
            if (_transform.position == currentTarget)
            {
                pathIndex++;
                if (pathIndex >= tilePath.Count)
                {
                    moving = false;
                    yield break;
                }

                currentTarget = tilePath[pathIndex].worldPosition;
            }
            
            // Rotate to moving direction
            LookAtTarget(currentTarget, rotationSpeed);
            
            _transform.position = Vector2.MoveTowards(_transform.position, currentTarget, sprintSpeed);
            
            yield return null;
        }
    }

    private void Start()
    {
        _transform = transform;
        target = transform.GetChild(0);
        _pathfinding = FindObjectOfType<Pathfinding>();
        map = FindObjectOfType<Map>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (currentPath != null)
        {
            foreach (Tile tile in currentPath)
            {
                Gizmos.DrawCube(new Vector3(tile.worldPosition.x, tile.worldPosition.y, -1f),
                    new Vector3(.3f, .3f, .3f));
            }
        }
    }
}