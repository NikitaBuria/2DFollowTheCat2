using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Keeps one enemy on each spawn point (the spawner's child Transforms) and brings it back respawnDelay
// seconds after it dies. Enemies come from one ObjectPool built from enemyPrefab, so the same objects
// are reused instead of being destroyed and created again (like EnemySummoner's minions).
// The enemy decides nothing about respawning: Enemy.Despawn hands it back here after its death animation.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [Tooltip("Seconds between a body disappearing and a new enemy on that point (scaled time: pauses with the game).")]
    [SerializeField, Min(0f)] private float respawnDelay = 20f;

    private ObjectPool<Enemy> pool;
    private Transform[] spawnPoints;
    // Which enemy stands on each point (null = empty), and when an empty point fills again.
    private Enemy[] occupants;
    private float[] respawnTimes;
    private readonly Dictionary<Enemy, int> pointOf = new();

    private void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"{name}: EnemySpawner has no enemyPrefab assigned.", this);
            enabled = false;
            return;
        }

        spawnPoints = new Transform[transform.childCount];
        for (int i = 0; i < spawnPoints.Length; i++) spawnPoints[i] = transform.GetChild(i);
        occupants = new Enemy[spawnPoints.Length];
        respawnTimes = new float[spawnPoints.Length];

        pool = new ObjectPool<Enemy>(
            CreateEnemy,
            actionOnRelease: enemy => enemy.gameObject.SetActive(false),
            actionOnDestroy: enemy => Destroy(enemy.gameObject),
            defaultCapacity: spawnPoints.Length,
            maxSize: spawnPoints.Length);
    }

    private void Start()
    {
        for (int i = 0; i < spawnPoints.Length; i++) Spawn(i);
    }

    private void Update()
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == null && Time.time >= respawnTimes[i]) Spawn(i);
        }
    }

    private void OnDestroy()
    {
        pool?.Clear();
    }

    private void Spawn(int point)
    {
        Enemy enemy = pool.Get();
        Vector3 position = spawnPoints[point].position;
        enemy.transform.position = position;
        if (enemy.TryGetComponent(out Rigidbody2D body))
        {
            body.position = position;
            body.linearVelocity = Vector2.zero;
        }

        enemy.gameObject.SetActive(true);
        enemy.ResetForSpawn();

        occupants[point] = enemy;
        pointOf[enemy] = point;
    }

    // Built asleep and owned by this spawner; Spawn positions and wakes it.
    private Enemy CreateEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, transform);
        enemy.AssignToPool(Release);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    // Called by Enemy.Despawn once the death animation has played: free the point and start its timer.
    private void Release(Enemy enemy)
    {
        if (pointOf.TryGetValue(enemy, out int point))
        {
            occupants[point] = null;
            respawnTimes[point] = Time.time + respawnDelay;
            pointOf.Remove(enemy);
        }

        pool.Release(enemy);
    }
}
