using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLSpace;

    public class Bullet : MonoBehaviour {
    public GameObject hitHole;
    public float hitForce = 16.0f;    // hit force 
    public int Damage = 25;
    public bool enableHit = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player)
            {
                player.Damage(25);
                Destroy(gameObject);
            }
           // Destroy(gameObject);
        }

        else if(other.gameObject.layer == 0 || other.gameObject.layer == 15)
        {
           // GameObject clone2 = Instantiate(hitHole, transform.position, Quaternion.LookRotation(transform.forward), other.transform);
            gameObject.SetActive(false);
            Destroy(gameObject,5);


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

