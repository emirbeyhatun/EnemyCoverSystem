using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiManager : MonoBehaviour {
    [HideInInspector]
    public List<enemy_ai> enemies;
    [HideInInspector]
    public List<enemy_ai> deads;
    [HideInInspector]
    public List<float> timer;
    private List<enemy_conversation> convObjects;
    public float dialogueDistance = 50;
    public float talkCooldown = 10;
    public List<ConversationClips> convClips;
    [HideInInspector] public bool Alarm = false;
    // Use this for initialization
    void Awake () {
        enemies = new List<enemy_ai>();
        timer = new List<float>();
        convObjects = new List<enemy_conversation>();
    }
	
    
	// Update is called once per frame
	public void AlertEnemies () {

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].aStates = AlarmStates.Alarmed;
        }
        Alarm = true;

    }
    public void AddEnemy(enemy_ai enemy)
    {
        enemies.Add(enemy);
        System.Random rnd = new System.Random();

        timer.Add(rnd.Next(10,22));
    }
    private void Update()
    {

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].mStates == MovementStates.InCover && timer[i] < 0)
            {
                System.Random rnd = new System.Random();
                enemies[i].GetCloserCover();
                timer[i] = rnd.Next(10, 22);
                
            }
            else if (enemies[i].mStates == MovementStates.InCover)
            {
                timer[i] -= Time.deltaTime;
            }
        }


        for (int i = 0; i < enemies.Count; i++)
        {
            for (int j = 0; j < enemies.Count; j++)
            {
                if (enemies[i].aStates == AlarmStates.NotAlarmed && enemies[j].aStates == AlarmStates.NotAlarmed 
                    && enemies[i].lStates == LifeStates.Alive && enemies[j].lStates == LifeStates.Alive &&
                    enemies[i].mStates != MovementStates.Talking && enemies[j].mStates != MovementStates.Talking )
                {
                    if(!GameObject.ReferenceEquals(enemies[i].gameObject, enemies[j].gameObject))
                    {
                        if(Vector3.Distance(enemies[i].transform.position, enemies[j].transform.position) <= dialogueDistance && enemies[i].isEnableForConverstaion && enemies[j].isEnableForConverstaion)
                        {

                            enemy_ai enemy1 = enemies[i];
                            enemy_ai enemy2 = enemies[j];
                            enemy1.mStates = MovementStates.Talking;
                            enemy2.mStates = MovementStates.Talking;
                            i = 0;
                            j = 0;
                            enemy1.GetComponent<Animator>().SetLayerWeight(1, 1);
                            enemy2.GetComponent<Animator>().SetLayerWeight(1, 1);
                            enemy_conversation e_conv = new enemy_conversation(enemy1, enemy2, this);
                            convObjects.Add(e_conv);
                        }
                    }

                }
            }

        }

        for (int i = 0; i < convObjects.Count; i++)
        {
            convObjects[i].UpdateLoop();
        }

    }
    
    public void StartCor(ConversationClips conv, List<AudioClip> starter, List<AudioClip> attender, enemy_ai enemyStarter, enemy_ai enemyAttender, bool isStarter, enemy_conversation conversationObj)
    {
        conversationObj.coroutine = PlayerTalk(conv, starter, attender, enemyStarter, enemyAttender, isStarter, conversationObj);
        StartCoroutine(conversationObj.coroutine);
    }
    public IEnumerator PlayerTalk(ConversationClips conv, List<AudioClip> starter, List<AudioClip> attender, enemy_ai enemyStarter, enemy_ai enemyAttender, bool isStarter, enemy_conversation conversationObj)
    {
        float time = 0;
        if (enemyStarter.aStates == AlarmStates.Alarmed || enemyAttender.aStates == AlarmStates.Alarmed)
        {
            enemyStarter.startAlarmCountdown = true;
            enemyAttender.startAlarmCountdown = true;
            enemyStarter.aStates = AlarmStates.Alarmed;
            enemyAttender.aStates = AlarmStates.Alarmed;
            enemyStarter.soundManager.talkSource.Stop();
            enemyAttender.soundManager.talkSource.Stop();
            StopCoroutine(conversationObj.coroutine);
            RemoveFromConvList(conversationObj, conv);
            yield break;
        }
        if (isStarter)
        {
            if(conv.i >= conv.starter.Count)
            {
                StopCoroutine(conversationObj.coroutine);
                RemoveFromConvList(conversationObj, conv);
                yield break;
            }
            time = PlayAudioFromList(starter, ref conv.i, enemyStarter);

            if (time >= 0)
            {
                enemyStarter.myAnimator.SetBool("conversation", true);
                enemyStarter.modelWeapon.transform.parent.gameObject.SetActive(false);

            }

        }
        else
        {
            if (conv.j >= conv.attending.Count)
            {
                StopCoroutine(conversationObj.coroutine);
                RemoveFromConvList(conversationObj, conv);
                yield break;
            }
            time = PlayAudioFromList(attender, ref conv.j, enemyAttender);

            if (time >= 0)
            {
                enemyAttender.myAnimator.SetBool("conversation", true);
                enemyAttender.modelWeapon.transform.parent.gameObject.SetActive(false);

            }

        }


        yield return new WaitForSeconds(time);
        if (isStarter)
        {
            enemyStarter.myAnimator.SetBool("conversation", false);
            enemyStarter.modelWeapon.transform.parent.gameObject.SetActive(true);

        }
        else
        {
            enemyAttender.myAnimator.SetBool("conversation", false);
            enemyAttender.modelWeapon.transform.parent.gameObject.SetActive(false);

        }
        conversationObj.coroutine = PlayerTalk(conv, starter, attender, enemyStarter, enemyAttender, !isStarter, conversationObj);
        StartCoroutine(conversationObj.coroutine);
    }

    private float PlayAudioFromList(List<AudioClip> list, ref int index, enemy_ai enemy)
    {
        if (list.Count <= index)
        {
            return 0;
        }
        enemy.soundManager.talkSource.clip = list[index];
        enemy.soundManager.talkSource.Play();
        index++;
        return enemy.soundManager.talkSource.clip.length;
    }

    public void RemoveFromConvList(enemy_conversation itemToRemove, ConversationClips conv)
    {


       itemToRemove.first_ai.conversationTimer = talkCooldown;
       itemToRemove.second_ai.conversationTimer = talkCooldown;
        itemToRemove.first_ai.isEnableForConverstaion = false;
        itemToRemove.second_ai.isEnableForConverstaion = false;
        convObjects.Remove(itemToRemove);
        itemToRemove.first_ai.soundManager.talkSource.Stop();
        itemToRemove.second_ai.soundManager.talkSource.Stop();

        itemToRemove.first_ai.myAnimator.SetLayerWeight(1, 0);
        itemToRemove.second_ai.myAnimator.SetLayerWeight(1, 0);

        itemToRemove.first_ai.myAnimator.SetBool("idleState", false);
        itemToRemove.second_ai.myAnimator.GetComponent<Animator>().SetBool("idleState", false);

       
       if (conv != null)
       {
         conv.i = 0;
         conv.j = 0;

       }
       if(itemToRemove.first_ai.waypoints.Count > 0)
        {
            itemToRemove.first_ai.GetComponent<NavMeshAgent>().SetDestination(itemToRemove.first_ai.waypoints[itemToRemove.first_ai.currentWaypoint].transform.position);

            //itemToRemove.second_ai.currentWaypoint += 1;
            //itemToRemove.second_ai.currentWaypoint %= itemToRemove.second_ai.waypoints.Count;
            itemToRemove.second_ai.GetComponent<NavMeshAgent>().SetDestination(itemToRemove.second_ai.waypoints[itemToRemove.second_ai.currentWaypoint].transform.position);
        }


       itemToRemove.first_ai.mStates = MovementStates.Patrol;
       itemToRemove.second_ai.mStates = MovementStates.Patrol;
    }

    public ConversationClips Conversation(enemy_ai first_ai, enemy_ai second_ai, enemy_conversation conversationObj)
    {
        System.Random rnd = new System.Random();
        int rndEnemy = rnd.Next(0, 1);
        int rndValue = rnd.Next(0, convClips.Count - 1);
        if (rndEnemy == 0)
        {
            StartCor(convClips[rndValue], convClips[rndValue].starter, convClips[rndValue].attending, second_ai, first_ai, true, conversationObj);

        }
        else if (rndEnemy == 1)
        {
            StartCor(convClips[rndValue], convClips[rndValue].starter, convClips[rndValue].attending, first_ai, second_ai, true, conversationObj);
        }
        return convClips[rndValue];

    }

    public bool GettingClose(enemy_ai first_ai, enemy_ai second_ai)
    {
        float dist = Vector3.Distance(first_ai.transform.position, second_ai.transform.position);
        if (dist > 5)
        {
            second_ai.GetComponent<NavMeshAgent>().isStopped = false;
            first_ai.GetComponent<NavMeshAgent>().isStopped = false;
            second_ai.GetComponent<NavMeshAgent>().SetDestination(first_ai.transform.position);
            first_ai.GetComponent<NavMeshAgent>().SetDestination(second_ai.transform.position);
            return false;
        }
        else
        {
            second_ai.myAnimator.SetBool("idleState", true);
            first_ai.myAnimator.SetBool("idleState", true);
            second_ai.GetComponent<NavMeshAgent>().isStopped = true;
            first_ai.GetComponent<NavMeshAgent>().isStopped = true;
            return true;

        }
    }
}
