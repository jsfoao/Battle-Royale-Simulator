using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GameCamera : MonoBehaviour
{
    private Map _map;
    private Transform _transform;
    private Camera _camera;

    [Header("Movement")] 
    [SerializeField] private float movementSpeed = 10f;

    private Vector3 direction = Vector3.zero;
    
    [Header("Zoom")] 
    [SerializeField] private float currentZoom = 15f;
    [SerializeField] private float minZoom = 20f;
    [SerializeField] private float maxZoom = 5f;
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float zoomSpeed = 20f;

    // Centers camera on map
    private void StationaryCamera()
    {
        _map = GameObject.Find("Map").GetComponent<Map>();
        Vector3 newPos = new Vector3(((_map.size.x - 1) * _map.offset) / 2, 
            ((_map.size.y - 1) * _map.offset) / 2, 
            -10f);
        transform.position = newPos;
    }
    
    private void CameraMovement()
    {
        // Input for vertical axis
        if (Input.GetKey(KeyCode.W)) { direction.y = 1; }
        else if (Input.GetKey(KeyCode.S)) { direction.y = -1; }
        else { direction.y = 0; }

        // Input for horizontal axis
        if (Input.GetKey(KeyCode.D)) { direction.x = 1; }
        else if (Input.GetKey(KeyCode.A)) { direction.x = -1; }
        else { direction.x = 0; }

        direction = direction.normalized;
        _transform.position += direction * movementSpeed * Time.deltaTime;
    }

    private void CameraZoom()
    {
        currentZoom -= Input.mouseScrollDelta.y * sensitivity;
        currentZoom = Mathf.Clamp(currentZoom, maxZoom, minZoom);
        float newSize = Mathf.MoveTowards(_camera.orthographicSize, currentZoom, zoomSpeed * Time.deltaTime);
        _camera.orthographicSize = newSize;
    }
    
    private void Update()
    {
        CameraMovement();
        CameraZoom();
    }

    private void Start()
    {
        _transform = transform;
        _camera = GetComponent<Camera>();
        StationaryCamera();
    }
}
