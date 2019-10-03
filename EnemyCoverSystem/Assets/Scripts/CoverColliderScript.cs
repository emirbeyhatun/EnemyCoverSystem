using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverColliderScript : MonoBehaviour {
    private enemy_ai parent;
    private BossController boss;

    private void Awake()
    {
        parent = transform.parent.GetComponent<enemy_ai>();
        boss = transform.parent.GetComponent<BossController>();
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if(parent)
            parent.ColliderTrigger(other, 1);
        else if(boss)
            boss.ColliderTrigger(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if(parent)
            parent.ColliderTrigger(other, 0);
        else if (boss)
            boss.ColliderTrigger(other);

    }
}
