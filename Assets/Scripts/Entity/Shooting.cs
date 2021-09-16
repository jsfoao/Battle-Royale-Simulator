using System;
using UnityEngine;
using UnityEngine.Events;

public class Shooting : MonoBehaviour
{
    private ParticleSystem _particlesShoot;
    private ParticleSystem _particlesBlood;

    private Transform _transformModel;
    private EntityAI _entityAI;
    private Entity _entity;
    [NonSerialized] public float currentTime;
    
    [Tooltip("Shooting cooldown in seconds")] 
    public float fireRate = 1f;
    public int damage = 20;

    private UnityEvent OnShoot;

    public void Shoot()
    {
        OnShoot.Invoke();

        var position = _transformModel.position;
        var right = _transformModel.right;
        RaycastHit2D hit = Physics2D.Raycast(position + right * 0.8f, right * _entityAI.viewDistance);
        if (hit.collider != null)
        {
            // Damage entity if valid hit
            if (hit.collider.transform.CompareTag("Entity"))
            {
                Entity entity = hit.collider.GetComponent<Entity>();
                entity.DoDamage(damage);
            }
        }
        Debug.DrawRay(position + right * 0.8f, right * _entityAI.viewDistance, Color.red, 0.1f);
    }

    private void Start()
    {
        _entityAI = GetComponent<EntityAI>();
        _transformModel = transform.Find("Model");
        _entity = GetComponent<Entity>();
        currentTime = fireRate;
        
        OnShoot = new UnityEvent();
        OnShoot.AddListener(_entity.RemoveLoot);
    }
}
