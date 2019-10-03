using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemy_conversation {
    public enemy_ai first_ai;
    public enemy_ai second_ai;
    AiManager aiManager;
    ConversationClips conversationClip;

    bool convStarted = false;



    enum States
    {
        GettingClose,
        Conversation,
        Nothing
    }
    States states = States.GettingClose;
    public IEnumerator coroutine;

    public enemy_conversation(enemy_ai first_ai , enemy_ai second_ai, AiManager aiManager)
    {
        this.first_ai = first_ai;
        this.second_ai = second_ai;
        this.aiManager = aiManager;
    }

    public void UpdateLoop () {

        if(second_ai.aStates != AlarmStates.NotAlarmed || first_ai.aStates != AlarmStates.NotAlarmed)
        {
            aiManager.RemoveFromConvList(this, conversationClip);
            return;
        }

        StateActions();


    }
    public void StateActions()
    {
        switch (states)
        {
            case States.GettingClose:
                GettingClose();
                break;
            case States.Conversation:
                Conversation();
                break;
            case States.Nothing:
                break;
            default:
                break;
        }
    }

    public void GettingClose()
    {
        if(aiManager.GettingClose(first_ai, second_ai)){
            states = States.Conversation;
        }
    }

    private void Conversation()
    {


        conversationClip = aiManager.Conversation(first_ai, second_ai,this);
        states = States.Nothing;

        
    }

    private void RemoveFromLoop()
    {
       // aiManager.RemoveFromConvList(this);
    }



  
}
