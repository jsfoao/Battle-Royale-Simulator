using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityController : MonoBehaviour
{
    private Transform _transform;
    private Pathfinding _pathfinding;
    private Map _map;
    private List<Tile> currentPath;
    public Transform target;

    [SerializeField] private float speed;
    private Coroutine movement;
    private float targetTime = 3f;
    private float currentTime;

    private bool startSim;
    
    public enum State
    {
        guarding,
        searching
    }
    [SerializeField] private State state = State.searching;

    private void SetRandomTarget()
    {
        target.position = new Vector3(Random.Range(0f, _map.worldSize.x - 1), Random.Range(0f, _map.worldSize.y -1), 0f);
        if (_map.TileFromWorldPosition(target.position).walkable == false)
        {
            return;
        }
        MoveToTarget(target);
    }

    private void MoveToTarget(Transform target)
    {
        if (movement != null)
        {
            StopCoroutine(movement);
        }
        
        if (_map.TileFromWorldPosition(_transform.position) == _map.TileFromWorldPosition(target.position))
        {
            return;
        }
        currentPath = _pathfinding.FindPath(_transform.position, target.position);
        movement = StartCoroutine(MoveAlongPath(currentPath));
    }

    private IEnumerator MoveAlongPath(List<Tile> tilePath)
    {
        int pathIndex = 0;
        Vector3 currentTarget = tilePath[pathIndex].worldPosition;
        while (true)
        {
            if (_transform.position == currentTarget)
            {
                pathIndex++;
                if (pathIndex >= tilePath.Count)
                {
                    yield break;
                }

                currentTarget = tilePath[pathIndex].worldPosition;
            }

            _transform.position = Vector2.MoveTowards(_transform.position, currentTarget, speed);
            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            startSim = !startSim;
        }

        if (startSim)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = Random.Range(1f, 3f);
                SetRandomTarget();
            }
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
                Gizmos.DrawCube(new Vector3(tile.worldPosition.x, tile.worldPosition.y, 0f),
                    new Vector3(.3f, .3f, .3f));
            }
        }
    }
}