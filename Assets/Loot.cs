using UnityEngine;
using UnityEngine.Events;

public class Loot : MonoBehaviour
{
    private GameManager _gameManager;
    private Transform _transform;
    [SerializeField] private float pickupRange = 2f;
    
    public UnityEvent OnPickup;

    private void DestroyLoot()
    {
        _gameManager.lootsList.Remove(gameObject);
        Destroy(gameObject);
    }
    
    private void Update()
    {
        foreach (var entityObject in _gameManager.entitiesList)
        {
            float distance = (entityObject.transform.position - _transform.position).magnitude;
            if (distance <= pickupRange)
            {
                Entity entity = entityObject.GetComponent<Entity>();
                
                entity.AddLoot();
                OnPickup.Invoke();
            }
        }
    }

    private void Start()
    {
        _transform = transform;
        _gameManager = FindObjectOfType<GameManager>();

        OnPickup = new UnityEvent();
        OnPickup.AddListener(DestroyLoot);
    }

    private void OnDrawGizmos()
    {
        _transform = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_transform.position, pickupRange);
    }
}
