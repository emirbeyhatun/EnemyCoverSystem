using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLSpace;
using UnityEngine.AI;

public class BossController : MonoBehaviour {
    public float hp = 100;
    public AiManager aiManager;
    public NavMeshAgent agent;
    public Animator myAnimator;
    [HideInInspector] public bool inCover = false;
    float radius = 40;
    public LayerMask coverLayer;
    private Transform coveDest;
    [HideInInspector] public  List<GameObject> megaphones;
    private void Awake()
    {
        megaphones = new List<GameObject>();
        megaphones.AddRange(GameObject.FindGameObjectsWithTag("Megaphone"));
    }
    private void Start()
    {
        StartTalking();
    }
    private void Update()
    {
        if(aiManager.Alarm == true && !inCover)
        {
            //StopTalking();
            //GetClosestCover();
            inCover = true;
        }
    }

    void GetClosestCover()
    {


        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, coverLayer);
        int i = 0;

        List<Transform> coverPositions = new List<Transform>();

        for (i = 0; i < hitColliders.Length; i++)
        {
            coverPositions.Add(hitColliders[i].transform);
        }
        int closest = 0;
        for (i = 0; i < coverPositions.Count; i++)
        {

            float pathLength = PathLength(coverPositions[i].position);
            float pathLength2 = PathLength(coverPositions[closest].position);
            if (pathLength <= 0)
                continue;

            if (pathLength < pathLength2)
            {
                closest = i;
            }
        }

        if (coverPositions.Count > 0)
        {
           

            agent.SetDestination(coverPositions[closest].position);
            coveDest = coverPositions[closest];
            agent.isStopped = false;
            myAnimator.SetBool("Run", true);
        }



    }
    public void ColliderTrigger(Collider other)
    {
        if (coveDest)
        {
            if (ReferenceEquals(coveDest.gameObject, other.gameObject))
            {
                myAnimator.SetBool("Kneel", true);
                myAnimator.SetBool("Run", false);
                agent.isStopped = true;
                inCover = false;
            }

        }
    }

    float PathLength(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(target, path);
        if (path == null)
        {
            return 0;
        }
        int i = 1;
        float currentPathLength = 0;
        Vector3 lastCorner;
        Vector3 currentCorner;
        if (path.corners.Length != 0)
        {
            lastCorner = path.corners[0];

            while (i < path.corners.Length)
            {
                currentCorner = path.corners[i];
                currentPathLength += Vector3.Distance(lastCorner, currentCorner);
                lastCorner = currentCorner;
                i++;
            }

            return currentPathLength;
        }

        return 0;

    }
    public void GotHit(float amount)
    {
        hp -= amount;
        GetClosestCover();
        StopTalking();
        if ( !inCover)
        {
            GetClosestCover();
            inCover = true;
        }
        GetComponent<Enemy_sound_manager>().PlayHitClips();
        if(hp<= 0)
        {
            GetComponent<RagdollManagerGen>().startRagdoll();
            Destroy(this);

        }
    }
    public void StartTalking()
    {
        for (int i = 0; i < megaphones.Count; i++)
        {
            megaphones[i].GetComponent<AudioSource>().Play();
        }
        GetComponent<Enemy_sound_manager>().PlayTalkClips();
    }
    public void StopTalking()
    {
        for (int i = 0; i < megaphones.Count; i++)
        {
            megaphones[i].GetComponent<AudioSource>().Stop();
        }
        GetComponent<Enemy_sound_manager>().talkSource.Stop();
    }
}
