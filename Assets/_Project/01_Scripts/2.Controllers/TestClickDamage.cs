using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestClickDamage : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSingleTargetDamage();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DamageSpecificMonsters(10f);
        }
    }

    private void HandleSingleTargetDamage()
    {
        Monster[] monsters = FindObjectsOfType<Monster>();
        TreasureChest[] chests = FindObjectsOfType<TreasureChest>();

        var targetMonster = monsters
            .Where(m => m.gameObject.activeSelf)
            .OrderBy(m => m.transform.position.x)
            .FirstOrDefault();

        var targetChest = chests
            .Where(c => c.gameObject.activeSelf)
            .OrderBy(c => c.transform.position.x)
            .FirstOrDefault();

        if (targetMonster != null && targetChest != null)
        {
            if (targetMonster.transform.position.x < targetChest.transform.position.x)
                targetMonster.TakeDamage(10f);
            else
                targetChest.TakeDamage(10f);
        }
        else if (targetMonster != null) targetMonster.TakeDamage(10f);
        else if (targetChest != null) targetChest.TakeDamage(10f);
    }

    private void DamageSpecificMonsters(float damage)
    {
        List<Monster> sortedMonsters = FindObjectsOfType<Monster>()
            .Where(m => m.gameObject.activeSelf)
            .OrderBy(m => m.transform.position.x)
            .ToList();

        if (sortedMonsters.Count >= 2) 
        {
            sortedMonsters[1].TakeDamage(damage);
        }

        if (sortedMonsters.Count >= 4) 
        {
            sortedMonsters[3].TakeDamage(damage);
        }
    }
}