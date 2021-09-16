using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityController : MonoBehaviour
{
    private GameManager _gameManager;
    private EntityAI _entityAI;
    private Entity _entity;
    private Transform _transform;
    private Transform _transformModel;
    private Pathfinding _pathfinding;
    [NonSerialized] public Map map;
    private List<Tile> currentPath;
    [NonSerialized] public Transform moveTarget;
    [NonSerialized] public Transform aimTarget;

    [SerializeField] public float pickupRange = 1f;
    [SerializeField] private float sprintSpeed = 0.05f;
    [SerializeField] private float walkSpeed = 0.02f;
    
    [SerializeField] private float rotationSpeed = 8f;
    public Coroutine movement;
    [NonSerialized] public bool moving;
    private float targetTime = 3f;
    private float currentTime;
    public Vector3 currentTarget;

    public void DestroyEntity()
    {
        _gameManager.entitiesList.Remove(gameObject);
        Destroy(gameObject);
    }
    
    public void StopMovement()
    {
        if (movement == null)
        {
            return;
        }

        moving = false;
        StopCoroutine(movement);
    }
    
    public void LookAtTarget(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        _transformModel.right = Vector3.Lerp(_transformModel.right, direction, Time.deltaTime * speed);
    }

    public void MoveToRandomTarget(Rect area)
    {
        // Randomizes target in area (square)
        moveTarget.position = new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax),
            0f);
        
        if (map.TileFromWorldPosition(moveTarget.position).walkable == false) { return; }
        if (moveTarget.position.x < 0 || moveTarget.position.x > map.size.x) { return; }
        if (moveTarget.position.y < 0 || moveTarget.position.y > map.size.y) { return; }
        
        // Will move to target if it's walkable and inside map
        MoveToTarget(moveTarget);
    }

    public void MoveToRandomTarget(Vector3 center, float radius)
    {
        // Randomizes target in area (circle)
        Vector3 randomVec = new Vector3(Random.Range(0f, radius), Random.Range(0f, radius), 0f);
        moveTarget.position = center + randomVec;
        
        if (map.TileFromWorldPosition(moveTarget.position).walkable == false) { return; }
        if (moveTarget.position.x < 0 || moveTarget.position.x > map.size.x) { return; }
        if (moveTarget.position.y < 0 || moveTarget.position.y > map.size.y) { return; }
        
        // Will move to target if it's walkable and inside map
        MoveToTarget(moveTarget);
    }

    public void MoveToTarget(Transform target)
    {
        if (movement != null) { StopCoroutine(movement); }

        if (map.TileFromWorldPosition(_transform.position) == map.TileFromWorldPosition(target.position)) { return; }

        currentPath = _pathfinding.FindPath(_transform.position, target.position);
        movement = StartCoroutine(MoveAlongPath(currentPath));
    }

    private IEnumerator MoveAlongPath(List<Tile> tilePath)
    {
        int pathIndex = 0;
        currentTarget = tilePath[pathIndex].worldPosition;
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

            _transform.position = Vector2.MoveTowards(_transform.position, currentTarget, sprintSpeed);
            
            yield return null;
        }
    }

    private void Update()
    {
        LookAtTarget(aimTarget.position, rotationSpeed);
    }

    private void Start()
    {
        _transform = transform;
        _transformModel = transform.Find("Model");
        moveTarget = _transform.Find("MoveTarget");
        aimTarget = _transform.Find("AimTarget");
        _entity = GetComponent<Entity>();
        _entityAI = GetComponent<EntityAI>();
        _gameManager = FindObjectOfType<GameManager>();
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
        aimTarget = transform.Find("AimTarget");
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimTarget.position, 1f);
    }
}