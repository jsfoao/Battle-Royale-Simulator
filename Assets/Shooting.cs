using UnityEngine;
using UnityEngine.Events;

public class Shooting : MonoBehaviour
{
    private Transform _transform;
    private Transform _transformModel;
    private EntityController _controller;
    private Entity _entity;
    public float fireRate = 1f;
    public float currentTime;
    public int damage = 20;

    private UnityEvent OnShoot;

    public void AimToTarget(Vector3 target)
    {
        _controller.aimTarget.position = target;
    }
    
    public void Shoot()
    {
        _entity.Loot -= 1;
        
        Debug.DrawRay(_transformModel.position + _transformModel.right * 0.8f, _transformModel.right * 10f, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(_transformModel.position + _transformModel.right * 0.8f, _transformModel.right * 10f);
        if (hit.collider != null)
        {
            if (hit.collider.transform.CompareTag("Entity"))
            {
                Entity entity = hit.collider.GetComponent<Entity>();
                entity.Health -= damage;
            }
        }
        OnShoot.Invoke();
    }

    private void Start()
    {
        _transform = transform;
        _transformModel = transform.Find("Model");
        _controller = GetComponent<EntityController>();
        _entity = GetComponent<Entity>();
        currentTime = fireRate;
        OnShoot = new UnityEvent();
    }
}
