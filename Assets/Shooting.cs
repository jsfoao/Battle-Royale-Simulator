using UnityEngine;
using UnityEngine.Events;

public class Shooting : MonoBehaviour
{
    private Transform _transform;
    private EntityController _controller;
    public float fireRate = 1f;
    public float currentTime;
    public int damage = 10;

    private UnityEvent OnShoot;

    public void AimToTarget(GameObject target)
    {
        if (target == null) { return; }
        _controller.aimTarget.position = target.transform.position;
    }
    
    public void Shoot(GameObject entityObject)
    {
        Debug.DrawLine(transform.position, entityObject.transform.position, Color.red);
        Entity entity = entityObject.GetComponent<Entity>();
        entity.Health -= damage;
        OnShoot.Invoke();
    }

    private void Start()
    {
        _transform = transform;
        _controller = GetComponent<EntityController>();
        currentTime = fireRate;
        OnShoot = new UnityEvent();
    }
}
