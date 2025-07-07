using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints = new Transform[4];

    [Header("Players")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    void Start()
    {
        InitializeReferences();
        SpawnPlayers();
    }

    void InitializeReferences()
    {
        FindSpawnPoints();
        FindPlayers();
        ValidateReferences();
    }

    void FindSpawnPoints()
    {
        // Tìm tất cả spawn points theo tên
        for (int i = 0; i < 4; i++)
        {
            if (spawnPoints[i] == null)
            {
                string spawnPointName = $"RandomSpawnPoint_{i + 1}";
                GameObject spawnPointObj = GameObject.Find(spawnPointName);

                if (spawnPointObj != null)
                {
                    spawnPoints[i] = spawnPointObj.transform;
                }
            }
        }
    }

    void FindPlayers()
    {
        // Tìm P1_Jet
        if (player1 == null)
        {
            GameObject playerObj = GameObject.Find("P1_Jet");
            if (playerObj != null)
            {
                player1 = playerObj;
            }
        }

        // Tìm P2_Jet
        if (player2 == null)
        {
            GameObject playerObj = GameObject.Find("P2_Jet");
            if (playerObj != null)
            {
                player2 = playerObj;
            }
        }
    }

    void ValidateReferences()
    {
        // Check spawn points
        int validSpawnPoints = spawnPoints.Count(sp => sp != null);
        if (validSpawnPoints < 4)
        {
            Debug.LogWarning($"[SpawnPointManager] Only found {validSpawnPoints}/4 spawn points!");
        }

        // Check players
        if (player1 == null)
        {
            Debug.LogError("[SpawnPointManager] P1_Jet not found!");
        }

        if (player2 == null)
        {
            Debug.LogError("[SpawnPointManager] P2_Jet not found!");
        }
    }

    [ContextMenu("Spawn Players")]
    public void SpawnPlayers()
    {
        if (!CanSpawn())
        {
            Debug.LogError("[SpawnPointManager] Cannot spawn - missing references!");
            return;
        }

        // Check GameManager setting
        bool useFixedSpawn = GameManager.Instance != null ? GameManager.Instance.FixedSpawn : false;

        if (useFixedSpawn)
        {
            SpawnPlayersFixed();
        }
        else
        {
            SpawnPlayersRandom();
        }
    }

    void SpawnPlayersFixed()
    {
        // Fixed spawn: Player 1 ở spawn point 1, Player 2 ở spawn point 3
        if (spawnPoints[0] != null && player1 != null)
        {
            player1.transform.position = spawnPoints[0].position;
            RotatePlayerTowardsCenter(player1);
            player1.SetActive(true);
        }

        if (spawnPoints[2] != null && player2 != null)
        {
            player2.transform.position = spawnPoints[2].position;
            RotatePlayerTowardsCenter(player2);
            player2.SetActive(true);
        }
    }

    void SpawnPlayersRandom()
    {
        // Random spawn: Chọn 2 spawn points khác nhau từ 4 points
        List<Transform> availableSpawnPoints = spawnPoints.Where(sp => sp != null).ToList();

        if (availableSpawnPoints.Count < 2)
        {
            Debug.LogError("[SpawnPointManager] Need at least 2 spawn points for random spawning!");
            return;
        }

        // Shuffle và lấy 2 points đầu tiên
        for (int i = 0; i < availableSpawnPoints.Count; i++)
        {
            Transform temp = availableSpawnPoints[i];
            int randomIndex = Random.Range(i, availableSpawnPoints.Count);
            availableSpawnPoints[i] = availableSpawnPoints[randomIndex];
            availableSpawnPoints[randomIndex] = temp;
        }

        // Spawn Player 1
        if (player1 != null)
        {
            player1.transform.position = availableSpawnPoints[0].position;
            RotatePlayerTowardsCenter(player1);
            player1.SetActive(true);
        }

        // Spawn Player 2
        if (player2 != null)
        {
            player2.transform.position = availableSpawnPoints[1].position;
            RotatePlayerTowardsCenter(player2);
            player2.SetActive(true);
        }
    }

    void RotatePlayerTowardsCenter(GameObject player)
    {
        if (player == null) return;

        // Tính vector từ player position đến center (0,0,0)
        Vector3 directionToCenter = (Vector3.zero - player.transform.position).normalized;
        
        // Tính góc trong degrees
        // Sprite mặc định hướng lên (Vector3.up), nên dùng Vector3.up làm reference
        float angle = Vector3.SignedAngle(Vector3.up, directionToCenter, Vector3.forward);
        
        // Apply rotation quanh trục Z
        player.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    bool CanSpawn()
    {
        return player1 != null && player2 != null && spawnPoints.Any(sp => sp != null);
    }

    // Method để respawn players (có thể gọi từ game logic khác)
    public void RespawnPlayers()
    {
        SpawnPlayers();
    }

    // Method để get spawn point position (useful cho other scripts)
    public Vector3 GetSpawnPointPosition(int index)
    {
        if (index >= 0 && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }

        Debug.LogWarning($"[SpawnPointManager] Invalid spawn point index: {index}");
        return Vector3.zero;
    }

    // Method để disable players (useful cho game over, restart, etc.)
    public void DisablePlayers()
    {
        if (player1 != null)
        {
            player1.SetActive(false);
        }

        if (player2 != null)
        {
            player2.SetActive(false);
        }
    }
}
