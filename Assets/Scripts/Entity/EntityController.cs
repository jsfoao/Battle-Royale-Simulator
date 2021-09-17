using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityController : MonoBehaviour
{
    private GameManager _gameManager;
    private Transform _transform;
    private Transform _transformModel;
    private Pathfinding _pathfinding;
    [NonSerialized] public Map map;
    private List<Tile> currentPath;
    [NonSerialized] public Transform moveTarget;
    [NonSerialized] public Transform aimTarget;
    
    [SerializeField][Tooltip("Speed while shooting")] 
    public float walkSpeed = 1f;
    [SerializeField][Tooltip("Speed while wandering/looting")] 
    public float sprintSpeed = 2f;
    [NonSerialized] public float moveSpeed;
    
    [SerializeField] private float rotationSpeed = 8f;
    public Coroutine movement;
    [NonSerialized] public bool moving;
    private float targetTime = 3f;
    private float currentTime;
    [NonSerialized] public Vector3 currentTarget;

    #region Aiming
    
    // Lerp rotation to aimTarget
    public void AimToTarget(Vector3 target)
    {
        aimTarget.position = Vector3.Lerp(aimTarget.position, target, rotationSpeed * Time.deltaTime);
    }
    
    // Locks rotation to a target
    public void LockOnTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        _transformModel.right = direction.normalized;
    }
    #endregion
    
    #region Movement
    public void SetWalkSpeed()
    {
        moveSpeed = walkSpeed;
    }

    public void SetSprintSpeed()
    {
        moveSpeed = sprintSpeed;
    }
    
    public void StopMovement()
    {
        if (movement == null) { return; }
        moving = false;
        StopCoroutine(movement);
    }

    // Move to given position
    public void MoveToPosition(Vector3 position)
    {
        moveTarget.position = position;
            
        if (map.TileFromWorldPosition(moveTarget.position).walkable == false) { return; }
        if (moveTarget.position.x < 0 || moveTarget.position.x > map.size.x) { return; }
        if (moveTarget.position.y < 0 || moveTarget.position.y > map.size.y) { return; }
        
        BeginMovement();
    }
    
    // Move to random position in area
    public void MoveToRandomPosition(Rect area)
    {
        // Randomizes target in area (square)
        moveTarget.position = new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax),
            0f);
        
        if (map.TileFromWorldPosition(moveTarget.position).walkable == false) { return; }
        if (moveTarget.position.x < 0 || moveTarget.position.x > map.size.x) { return; }
        if (moveTarget.position.y < 0 || moveTarget.position.y > map.size.y) { return; }
        
        // Will begin movement if target tile is walkable and inside map
        BeginMovement();
    }

    private void BeginMovement()
    {
        // Stops moving if player is currently moving
        if (movement != null) { StopCoroutine(movement); }

        // Returns if is already on target destination
        if (map.TileFromWorldPosition(_transform.position) == map.TileFromWorldPosition(moveTarget.position)) { return; }

        // Begins movement to moveTarget
        currentPath = _pathfinding.FindPath(_transform.position, moveTarget.position);
        movement = StartCoroutine(MoveAlongPath(currentPath));
    }

    private IEnumerator MoveAlongPath(List<Tile> tilePath)
    {
        if (tilePath == null) { yield return null; }
        
        int pathIndex = 0;
        currentTarget = tilePath[pathIndex].worldPosition;
        
        while (true)
        {
            moving = true;
            // Will iterate to next target once it reached current one
            if (_transform.position == currentTarget)
            {
                pathIndex++;
                
                // If reached end of tilePath
                if (pathIndex >= tilePath.Count)
                {
                    // Entity is not moving
                    moving = false;
                    yield break;
                }

                // currentTarget is next position in tilePath
                currentTarget = tilePath[pathIndex].worldPosition;
            }

            // Lerp to next position
            _transform.position = Vector2.MoveTowards(_transform.position, currentTarget, moveSpeed * Time.deltaTime);
            
            yield return null;
        }
    }
    #endregion
    
    private void Update()
    {
        LockOnTarget(aimTarget.position);
    }
    
    private void Start()
    {
        _transform = transform;
        _transformModel = transform.Find("Model");
        moveTarget = _transform.Find("MoveTarget");
        aimTarget = _transform.Find("AimTarget");
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