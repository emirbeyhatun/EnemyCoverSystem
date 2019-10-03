using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLSpace;

public enum MovementStates
{
    Patrol,
    LookAround,
    Stay,
    GoingToCover,
    InCover,
    GoingToCloserCover,
    Talking,
    GoToPlayer
}

public enum LifeStates
{
    Alive,
    Dead
}

public enum AlarmStates
{
    NotAlarmed,
    Alarmed
}
enum AttackStates
{
    StandAttack,
    CoverAttack,
    CeaseFire
}
enum AnimationStates
{
    StandToKneel,
    KneelToStand,
    Walk
}
public class enemy_ai : MonoBehaviour
{

    NavMeshAgent agent;
    [HideInInspector] public MovementStates mStates;//movement states
     public LifeStates lStates;//movement states
     public AlarmStates aStates;//alarm states
    AttackStates attStates;
    float timer;
    private Vector3 tempVec;
    float waitTime = 0;
    private Transform coverDestination;
    private bool inSight = false;
    private Vector3 lastSeenPosition;
    private bool isLastSeenPos = false;
    [HideInInspector]
    public Animator myAnimator;
    private float fireRate = 0;

    public int debugMode = 1;
    public int hp = 100;
    public GameObject lastSeenGameobject;
    public List<GameObject> waypoints;
    public int currentWaypoint = 0;
    public float accuracy = 3f;
    public Transform targetTransform;
    public Transform sightTransform;
    public float distance = 50f;
    public float shootDistance = 250f;
    public float angle = 60f;
    public float radius = 50;
    public float lookAroundTime;
    public LayerMask coverLayer;
    public LayerMask rayLayer;
    public GameObject modelSpine;
    public GameObject modelWeapon;
    public GameObject modelBullet;
    public float fireRateLimit = 1f;
    [HideInInspector]
    public Enemy_sound_manager soundManager;

    public CoverManager coverManager; 
    public AiManager aiManager;
    private int AlertForOnce = 0;
    public float timerToAlarm = 3;
    [HideInInspector] public bool startAlarmCountdown = false;
    [HideInInspector]
    public bool isEnableForConverstaion = true;
    [HideInInspector]
    public float conversationTimer = 0;
    private float stepTime = 0.5f;
    private float stepTimer = 0;
    private bool forceChangeDestination = false;
    private bool inShootSight = false;
    bool coverRotate = true;
    float coverAttackCooldown = 5f;
    float coverAttackTimer = 0f;
    float bullet_invoke_timer = 2f;
    bool reverse = false;


    Quaternion sightRotation;
    private void Awake()
    {
        sightRotation = sightTransform.rotation;
        agent = GetComponent<NavMeshAgent>();
        mStates = MovementStates.Patrol;
        aStates = AlarmStates.NotAlarmed;
        lStates = LifeStates.Alive;

        myAnimator = GetComponent<Animator>();
        soundManager = GetComponent<Enemy_sound_manager>();
        tempVec = new Vector3();
        int min = 0;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (Vector3.Distance(transform.position, waypoints[min].transform.position) > Vector3.Distance(transform.position, waypoints[i].transform.position))
            {
                min = i;
            }
        }
        currentWaypoint = min;
        if(waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypoint].transform.position);
            agent.isStopped = false;
        }
        else
        {
            myAnimator.SetBool("idleState", true);
        }

    }
    private void Start()
    {
        aiManager.AddEnemy(this);
    }
    private void Update()
    {
        if(lStates != LifeStates.Dead)
        {
            if (conversationTimer <= 0)
            {
                isEnableForConverstaion = true;
            }
            else
            {
                isEnableForConverstaion = false;
            }
            conversationTimer -= Time.deltaTime;
            stepTimer += Time.deltaTime;


            sightRotation = Quaternion.Euler(0, sightTransform.rotation.eulerAngles.y, 0);
            sightTransform.SetPositionAndRotation(sightTransform.position, sightRotation);
            Sight();
            waitTime += Time.deltaTime;
            waitTime %= 1;

            if (aStates == AlarmStates.NotAlarmed)
                NotAlarmedStates();
            else if (aStates == AlarmStates.Alarmed)
            {
                if (AlertForOnce == 0 && timerToAlarm <= 0)
                {
                    aiManager.AlertEnemies();
                    AlertForOnce = 1;
                }
                AlarmedStates();
            }
        }
        else
        {
            agent.isStopped = true;
        }

        if (startAlarmCountdown)
        {
            timerToAlarm  -= Time.deltaTime;
        }

    }

    private void NotAlarmedStates()
    {
        switch (mStates)
        {
            case MovementStates.Patrol:
                Patrol();
                break;
            case MovementStates.Talking:
                Talking();
                break;
            case MovementStates.LookAround:
                LookAround();
                break;
            default:
                break;
        }
    }

    public void ColliderTrigger(Collider other, int triggerState)
    {
        if(triggerState == 1)
        {
            if (!coverDestination)
                return;
            if (mStates == MovementStates.Stay)
                return;

            if (IsCoverFull(other.gameObject))
            {

                return;
            }
            if (GameObject.ReferenceEquals(other.gameObject, coverDestination.gameObject))
            {

                mStates = MovementStates.InCover;
                attStates = AttackStates.CoverAttack;
                myAnimator.SetBool("coverState", true);
                coverManager.fullCovers.Add(gameObject.GetInstanceID(), other.gameObject.GetInstanceID());
                //myAnimator.SetBool("standFire", false);
            }
        }
        else
        {
            if (coverManager.fullCovers.ContainsKey(gameObject.GetInstanceID()))
            {
                int temp;
                coverManager.fullCovers.TryGetValue(gameObject.GetInstanceID(), out temp);

                if(other.gameObject.GetInstanceID() == temp)
                {
                    coverManager.fullCovers.Remove(gameObject.GetInstanceID());
                }    
            }
        }

    }
    public bool IsCoverFull(GameObject cover)
    {
        if (coverManager.fullCovers.ContainsValue(cover.GetInstanceID()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public int GetMyCurrentCoverInstanceID(GameObject enemy)
    {
        int instanceID = 0;
        if(coverManager.fullCovers.TryGetValue(enemy.GetInstanceID(), out instanceID))
        {
            return instanceID;
        }
        else
        {
            return -1;

        }

    }
    private void AlarmedStates()
    {
        myAnimator.SetBool("idleState", false);
        switch (mStates)
        {
            case MovementStates.Patrol:
                break;
            case MovementStates.Stay:
                if(debugMode == 1)
                print("AlarmedStates() -- Stay");
                Stay();
                break;
            case MovementStates.InCover:
                if (debugMode == 1)
                    print("AlarmedStates() -- InCover");
                    InCover();

                break;
            case MovementStates.GoingToCover:
                if (debugMode == 1)
                    print("AlarmedStates() -- GoingToCover");
                    GoingToCover();
               // myAnimator.SetBool("standFire", false);
                break;
            case MovementStates.GoToPlayer:
                GoToPlayer();
                break;
            default:
                break;
        }

        switch (attStates)
        {
            case AttackStates.StandAttack:
                if (debugMode == 1)
                    print("AlarmedStates() -- StandAttack");
                StandAttack();
                break;
            case AttackStates.CoverAttack:
                if (debugMode == 1)
                    print("AlarmedStates() -- CoverAttack");
                CoverAttack();
                break;
            case AttackStates.CeaseFire:
                if (debugMode == 1)
                    print("AlarmedStates() -- CeaseFire");
                break;
            default:
                break;
        }
    }
    private void LateUpdate()
    {
        if (aStates == AlarmStates.Alarmed && lStates != LifeStates.Dead)
        { 
            switch (attStates)
            {
                case AttackStates.StandAttack:
                    LookAtPlayer();
                    break;

                default:
                    break;
            } 
        }
    }
    void Sight()
    {
        if (targetTransform == null) { return; }
        if (sightTransform == null) { return; }

        SightGap(targetTransform);

        ///if it sees a body it must go to alarm mode
        if(aStates != AlarmStates.Alarmed)
        {
            for (int l = 0; l < aiManager.deads.Count; l++)
            {
                SightGap(aiManager.deads[l].transform);
            }

        }
        else
        {
            IsInFireSight(targetTransform);
        }
    }

    void SightGap(Transform target)
    {

        if (Vector3.Distance(target.position, sightTransform.position) < distance)
        {
            Vector3 dir = target.position - sightTransform.position;
            if (debugMode == 1)
                print("Angle : " + Vector3.Angle(dir, sightTransform.forward));

            if (Vector3.Angle(dir, sightTransform.forward) < angle)
            {
                RaycastHit hit;
                //if (debugMode == 1)
                    //Debug.DrawRay(sightTransform.position, dir, Color.red, 0.3f);


                if (Physics.Raycast(sightTransform.position, dir, out hit, distance, rayLayer))
                {
                    if (GameObject.ReferenceEquals(hit.collider.gameObject, target.gameObject))
                    {
                        

                        if (debugMode == 1)
                            print("in sight");

                        aStates = AlarmStates.Alarmed;
                        startAlarmCountdown = true;

                        GetClosestCover();

                        lastSeenPosition = target.position;
                        lastSeenGameobject.transform.position = lastSeenPosition;
                        isLastSeenPos = true;
                        inSight = true;
                    }
                    else
                    {
                        enemy_ai enm = target.GetComponent<enemy_ai>();
                        enemy_ai enm2 = hit.collider.gameObject.GetComponentInParent<enemy_ai>();

                        if (enm && enm2)
                        {

                            if (GameObject.ReferenceEquals(enm.gameObject, enm2.gameObject))
                            {
                                aStates = AlarmStates.Alarmed;
                                startAlarmCountdown = true;

                                GetClosestCover();
                            }
                        }
                        else
                        {
                            if (aStates == AlarmStates.Alarmed && mStates != MovementStates.InCover)
                            {
                                GetClosestCover();
                            }
                        }
                        inSight = false;                        
                    }

                    

                }
                else
                {
                    inSight = false;
                }

            }
            else
            {
                inSight = false;
            }
        }
        else
        {
            inSight = false;
            if (aStates == AlarmStates.Alarmed && mStates != MovementStates.GoToPlayer)
            {
                GetClosestCover();
            }
        }
    }

    void IsInFireSight(Transform target)
    {
        if (Vector3.Distance(target.position, sightTransform.position) < shootDistance)
        {
            Vector3 dir = target.position - sightTransform.position;

            if (Vector3.Angle(dir, sightTransform.forward) < angle)
            {
                RaycastHit hit;

                if (Physics.Raycast(sightTransform.position, dir, out hit, shootDistance, rayLayer))
                {

                    if (GameObject.ReferenceEquals(hit.collider.gameObject, target.gameObject))
                    {
                        inShootSight = true;
                    }
                    else
                    {
                        inShootSight = false;
                    }
                }
            }
        }

    }
    void Patrol()
    {
        if (waypoints.Count <= 0)
            return;


        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].transform.position) < accuracy + 2)
        {
            timer = lookAroundTime;
            agent.isStopped = true;
            mStates = MovementStates.LookAround;
            myAnimator.SetBool("idleState", true);

            return;
        }

        if (Vector3.Magnitude(agent.velocity) > 0 && stepTimer > stepTime)
        { 
            soundManager.PlayFootSteps();
            stepTimer = 0;
        }
        if (debugMode == 1)
            print("Patrol setdest");
        agent.isStopped = false;
        SetDestination();

    }
    void Stay()
    {
        agent.isStopped = true;
        if ((Vector3.Distance(targetTransform.position, sightTransform.position) < distance))
        {
            myAnimator.SetBool("standFire", true);
        }
        else
        {
            myAnimator.SetBool("standFire", false);
            myAnimator.SetBool("idleState", true);
        }

    }
    void GoToPlayer()
    {
        if (debugMode == 1)
            print("GOTOPLAYER");

        if ((Vector3.Distance(targetTransform.position, sightTransform.position) > distance))
        {
            if (debugMode == 1)
                print("GOTOPLAYER INSIDE");
            agent.isStopped = false;
            myAnimator.SetBool("idleState", false);
            myAnimator.SetBool("standFire", false);
            myAnimator.SetBool("coverFire", false);
            myAnimator.SetBool("coverState", false);
            agent.SetDestination(targetTransform.position);
        }
        else
        {
            mStates = MovementStates.Stay;
        }
    }
    void Talking()
    {
        //agent.isStopped = true;
    }

    void InCover()
    {
        if (coverDestination && coverRotate)
        {
            coverRotate = false;
            StartCoroutine(RotateToAngle());
        }
    }

    void CoverAttack()
    {
        if (Vector3.Distance(targetTransform.position, sightTransform.position) > shootDistance)
            return;

        LookAtPlayer();
        myAnimator.SetBool("coverFire", false);
        if (coverAttackTimer > coverAttackCooldown)
        {
            myAnimator.SetBool("coverFire", true);
            coverAttackTimer = 0;
            bullet_invoke_timer = 2;

        }
        coverAttackTimer += Time.deltaTime;
    }
    bool CheckIfCoverExposed()
    {
        if (coverDestination)
        {
            RaycastHit hit;
            Vector3 dir = targetTransform.position - coverDestination.position;
            if(debugMode == 1)
            Debug.DrawRay(coverDestination.position, dir, Color.grey, 0.3f);
            if (Physics.Raycast(coverDestination.position, dir, out hit, 10000, rayLayer))
            {
                if (GameObject.ReferenceEquals(hit.collider.gameObject, targetTransform.gameObject))
                {
                    if (debugMode == 1)
                        print("Siperdeyken exposed oldum");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    void GoingToCover()
    {
        if ((coverManager.fullCovers.ContainsValue(coverDestination.GetInstanceID())))
        {
           
            GetClosestCover();

        }
        coverRotate = true;
        myAnimator.SetBool("coverState", false);
    }
    void GetClosestCover()
    {
        if (waitTime > 0.5) { return; }

        if (mStates == MovementStates.InCover)
        {
            if (!CheckIfCoverExposed())
            {

                if (debugMode == 1)
                    print("GetClosestCover - exposed olmadim");
                return;
            }



        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, coverLayer);
        int i = 0;

        List<Transform> coverPositions = new List<Transform>();

        for (i = 0; i < hitColliders.Length; i++)
        {
            Vector3 dir = targetTransform.position - hitColliders[i].transform.position;
            RaycastHit hit;
            if (debugMode == 1)
                Debug.DrawRay(hitColliders[i].transform.position, dir, Color.red, 0.3f);
            if (Physics.Raycast(hitColliders[i].transform.position, dir, out hit, 10000, rayLayer))
            {


                
                if (!GameObject.ReferenceEquals(hit.collider.gameObject, targetTransform.gameObject))
                {
                    if (!(coverManager.fullCovers.ContainsValue(hitColliders[i].gameObject.GetInstanceID())))
                    {
                        coverPositions.Add(hitColliders[i].transform);
                    }
                }
            }
        }
        if (coverPositions.Count <= 0)
        {
            if (debugMode == 1)
                print("coverPositions is zero");
            //TODO stay degilde belki oyuncuya yaklasir ates ederek
            if ((Vector3.Distance(targetTransform.position, sightTransform.position) > distance))
            {
                mStates = MovementStates.GoToPlayer;
            }
            else
            {
                mStates = MovementStates.Stay;

            }
            attStates = AttackStates.StandAttack;
            return;
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

        float coverPathLength = -1;
        if (coverPositions.Count > 0)
        {
            Vector3 dirr = targetTransform.position - coverPositions[closest].position;
            if (debugMode == 1)
                Debug.DrawRay(coverPositions[closest].position, dirr, Color.blue, 0.5f);

            coverPathLength = PathLength(coverPositions[closest].position);
        }
       
        float playerPathLength = PathLength(targetTransform.position);

        if ((coverPathLength <= 0  || (playerPathLength < coverPathLength)) && mStates != MovementStates.InCover)
        {
            if (debugMode == 1)
                print("target is closer than the cover");
            //TODO stay degilde belki oyuncuya yaklasir ates ederek
            if ((Vector3.Distance(targetTransform.position, sightTransform.position) > distance))
            {
                mStates = MovementStates.GoToPlayer;
            }
            else
            {
                mStates = MovementStates.Stay;

            }

            attStates = AttackStates.StandAttack;
            return;
        }
        if (coverPositions.Count > 0)
        {
            coverDestination = coverPositions[closest];
            agent.SetDestination(coverPositions[closest].position);
        }
        agent.isStopped = false;
        myAnimator.SetBool("standFire", false);

        mStates = MovementStates.GoingToCover;
        attStates = AttackStates.StandAttack;
    }

    public void GetCloserCover()
    {
        if ((Vector3.Distance(targetTransform.position, sightTransform.position) > shootDistance))
        {
            mStates = MovementStates.GoToPlayer;
            attStates = AttackStates.StandAttack;
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, coverLayer);
        int i = 0;

        List<Transform> coverPositions = new List<Transform>();
        Transform currentCover = null;

        for (i = 0; i < hitColliders.Length; i++)
        {
            Vector3 dir = targetTransform.position - hitColliders[i].transform.position;
            RaycastHit hit;
            if (debugMode == 1)
                Debug.DrawRay(hitColliders[i].transform.position, dir, Color.red, 0.3f);
            if (Physics.Raycast(hitColliders[i].transform.position, dir, out hit, 10000, rayLayer))
            {

                if (!GameObject.ReferenceEquals(hit.collider.gameObject, targetTransform.gameObject))
                {
                    if (!(coverManager.fullCovers.ContainsValue(hitColliders[i].gameObject.GetInstanceID())))
                    {
                        coverPositions.Add(hitColliders[i].transform);
                    }
                    if (hitColliders[i].gameObject.GetInstanceID() == GetMyCurrentCoverInstanceID(gameObject))
                    {
                        currentCover = hitColliders[i].transform;
                    }
                }
            }
        }
        if (coverPositions.Count <= 0)
        {
            if (debugMode == 1)
                print("Closer coverPositions is zero");

            return;
        }

        int closest = 0;
        int selected = 0;
        for (i = 0; i < coverPositions.Count; i++)
        {
            if (currentCover == null)
            {
                print("currentCover is null");
                break;

            }
            float currentCoverToPlayerDist = Vector3.Distance(currentCover.position, targetTransform.position);
            float enemyToCover = PathLength(coverPositions[i].position);
            float enemyToPlayerDist = PathLength(targetTransform.position);

            float pathLength = Vector3.Distance(coverPositions[i].position, targetTransform.position);

            if ((enemyToCover < enemyToPlayerDist && pathLength < currentCoverToPlayerDist) && !IsCoverFull(coverPositions[i].gameObject))
            {
                closest = i;
                selected++;
                break;
            }
        }
        if (selected == 0)
        {
            print("getclosercover there isnt any closer pos");
            return;

        }
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(coverPositions[closest].position.x, coverPositions[closest].position.y + 5, coverPositions[closest].position.z);

        Vector3 dirr = targetTransform.position - coverPositions[closest].position;
        if (debugMode == 1)
            Debug.DrawRay(coverPositions[closest].position, dirr, Color.blue, 0.3f);
        coverDestination = coverPositions[closest];
        agent.SetDestination(coverPositions[closest].position);
        agent.isStopped = false;
        myAnimator.SetBool("standFire", false);
        mStates = MovementStates.GoingToCover;
        attStates = AttackStates.StandAttack;

    }
    void StandAttack()
    {
        if (!inShootSight)
            return;
        if (fireRate > fireRateLimit && inSight)
        {
            bullet_invoke_timer = 2f;
            fireRate = 0;
            InstantiateBullet();
        }
        fireRate += Time.deltaTime;

    }
    void InstantiateBullet()
    {
        if(bullet_invoke_timer == 2)
            soundManager.PlayWeaponClips();

        Vector3 dir = targetTransform.position - modelWeapon.transform.position;
        dir.Normalize();
        GameObject clone = Instantiate(modelBullet, modelWeapon.transform.position, Quaternion.identity, null);
        clone.transform.rotation = Quaternion.LookRotation( dir);
        clone.GetComponent<Rigidbody>().AddForce(dir * 800);
        Destroy(clone, 8);
        if (bullet_invoke_timer>0 /*&& mStates == MovementStates.InCover*/)
        {
            Invoke("InstantiateBullet", 0.2f);
            bullet_invoke_timer -= 1;
        }
    }
    void LookAround()
    {
        timer -= Time.deltaTime;
        if (debugMode == 1)
            print("LookAround");

        if (timer <= 0)
        {

            timer = lookAroundTime;
            myAnimator.SetBool("idleState", false);
            forceChangeDestination = true;
            SetDestination();
            mStates = MovementStates.Patrol;
        }
    }
    void SetDestination()
    {
        Vector3 l_pos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 l_way = new Vector3(waypoints[currentWaypoint].transform.position.x, 0, waypoints[currentWaypoint].transform.position.z);
       
        //print(Vector3.Distance(l_pos, l_way));
        if (Vector3.Distance(l_pos, l_way) < accuracy + 1 || forceChangeDestination == true)
        {
            forceChangeDestination = false;
            if (!reverse && currentWaypoint >= waypoints.Count -1)
            {
                reverse = true;
                currentWaypoint = waypoints.Count - 1;
            }
            else if (reverse && currentWaypoint <= 0)
            {
                reverse = false;
                if (waypoints.Count > 1)
                    currentWaypoint = 1;
                else
                    currentWaypoint = 0;
            }

            if (reverse)
            {
                currentWaypoint -= 1;
            }
            else
            {
                currentWaypoint += 1;
            }
            
            agent.SetDestination(waypoints[currentWaypoint].transform.position);
        }
    }
    public void GotHit(int damage)
    {
        if(lStates != LifeStates.Dead)
        {
            hp -= damage;
            soundManager.PlayHitClips();
            aStates = AlarmStates.Alarmed;
            startAlarmCountdown = true;
        }
        
        if(hp<= 0)
        {
            startAlarmCountdown = false;
            timerToAlarm = 3;
            lStates = LifeStates.Dead;
            GetComponent<RagdollManager>().startRagdoll();
            aiManager.enemies.Remove(this);
            aiManager.deads.Add(this);
            //Destroy(GetComponent < enemy_ai > ());
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
        if(path.corners.Length != 0)
        {
            lastCorner = path.corners[0];


            while (i < path.corners.Length)
            {
                currentCorner = path.corners[i];
                currentPathLength += Vector3.Distance(lastCorner, currentCorner);
                if (debugMode == 1)
                    Debug.DrawLine(lastCorner, currentCorner, Color.green, 0.2f);
                lastCorner = currentCorner;
                i++;
            }

            return currentPathLength;
        }


        return 0;

    }
    void LookAtPlayer()
    {
        if (modelSpine)
        {
            Vector3 dir = targetTransform.position - transform.position;
            if(mStates != MovementStates.InCover)
            {
                
                if (Vector3.Angle(dir, transform.forward) > 120)// TODO diger acilarda geri geri gitmeli
                {
                    myAnimator.SetBool("walkBack", true);
                    Quaternion qua = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 180, transform.rotation.eulerAngles.z);
                    modelSpine.transform.parent.SetPositionAndRotation(modelSpine.transform.parent.position, qua);

                }
                else
                {

                    myAnimator.SetBool("walkBack", false);
                    Quaternion qua = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                    modelSpine.transform.parent.SetPositionAndRotation(modelSpine.transform.parent.position, qua);

                }

                tempVec.x = targetTransform.position.x;
                tempVec.y = transform.position.y;
                tempVec.z = targetTransform.position.z;

                modelSpine.transform.LookAt(tempVec);
                modelSpine.transform.Rotate(new Vector3(0, 40, 0));
            }
            else
            {

                Quaternion qua = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                modelSpine.transform.parent.SetPositionAndRotation(modelSpine.transform.parent.position, qua);

                tempVec.x = targetTransform.position.x;
                tempVec.y = transform.position.y;
                tempVec.z = targetTransform.position.z;

                transform.LookAt(tempVec);
                transform.Rotate(new Vector3(0, 40, 0));
            }

        }

    }
    IEnumerator RotateToAngle()
    {
        float t = 0;
        while (true)
        {
            yield return null;
            Quaternion rot = Quaternion.Lerp(transform.rotation, coverDestination.rotation, 0.1f);
            transform.SetPositionAndRotation(transform.position, rot);
            t += Time.deltaTime;

            if (t > 2f)
            {
                break;
            }

        }
    }
}
