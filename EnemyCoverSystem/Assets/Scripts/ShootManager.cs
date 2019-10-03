using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLSpace;

public class ShootManager : MonoBehaviour
{
    public Transform BulletTracePoint;
    public LayerMask bulletLayer;
    public float hitForce = 16.0f;    // hit force 
    public GameObject muzzleFlash;
    public GameObject hitHole;
    public void PlayerShoot()
    {
        GameObject clone = Instantiate(muzzleFlash, BulletTracePoint.transform.position,Quaternion.identity);
        Destroy(clone, 0.1f);
        RaycastHit rayHit;
        Debug.DrawRay(BulletTracePoint.position, BulletTracePoint.transform.forward*50, Color.red, 1f);
        if (Physics.Raycast(BulletTracePoint.position, BulletTracePoint.transform.forward, out rayHit, 50, bulletLayer))
        {
            enemy_ai enemy = rayHit.collider.GetComponentInParent<enemy_ai>();
            BossController boss = rayHit.collider.GetComponentInParent<BossController>();
            if (enemy != null)
            {
                GameObject clone2 = Instantiate(hitHole, rayHit.point, Quaternion.LookRotation(rayHit.normal), enemy.transform);
                if (rayHit.collider.CompareTag("Head")){

                    enemy.GotHit(100);
                }
                else
                {
                    enemy.GotHit(25);

                }
                Spherecast(rayHit.collider.transform, 0.1f);
            }
            else if(boss != null)
            {
                GameObject clone2 = Instantiate(hitHole, rayHit.point, Quaternion.LookRotation(rayHit.normal), boss.transform);
                if (rayHit.collider.CompareTag("Head")){

                    boss.GotHit(100);
                }
                else
                {
                    boss.GotHit(25);

                }
                Spherecast(rayHit.collider.transform, 0.2f);
            }
            else
            {
                GameObject clone2 = Instantiate(hitHole, rayHit.point, Quaternion.LookRotation(rayHit.normal));

            }
        }
    }

    public void Spherecast(Transform hit, float radius)
    {
        BodyColliderScript bcs = hit.GetComponent<BodyColliderScript>();
        if (bcs)
        {
            int[] parts = new int[] { bcs.index };
            bcs.ParentRagdollManager.startHitReaction(parts, transform.forward * hitForce);
        }
    }
}
