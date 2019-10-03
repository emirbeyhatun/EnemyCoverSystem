using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ConversationClips {

    public List<AudioClip> starter;
    public List<AudioClip> attending;
    [HideInInspector]
    public int i = 0;
    [HideInInspector]
    public int j = 0;

    private enemy_conversation Obj;
}
