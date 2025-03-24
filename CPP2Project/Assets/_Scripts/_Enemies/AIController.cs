using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    public Transform player;
    private List<EnemyAI> enemies = new List<EnemyAI>();

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        if (player == null)
        {
            Debug.LogError("Player not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.UpdateAI(player != null ? player.position : Vector3.zero);
            }
        }
    }

    //Register enemy to the controller
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            Debug.Log($"Enemy {enemy.name} registered.");
        }
    }

    //Unregister an enemy on death
    public void UnregisterEnemy(EnemyAI enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            Debug.Log($"Enemy {enemy.name} unregistered.");
        }
    }

}
