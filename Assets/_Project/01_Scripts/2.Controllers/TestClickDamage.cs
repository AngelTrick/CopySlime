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
            Monster[] monsters = FindObjectsOfType<Monster>();

            var targetMonster = monsters
                .Where(m => m.gameObject.activeSelf)
                .OrderBy(m => m.transform.position.x)
                .FirstOrDefault();

            if (targetMonster != null)
            {
                targetMonster.TakeDamage(10f);
            }
        }
    }
}
