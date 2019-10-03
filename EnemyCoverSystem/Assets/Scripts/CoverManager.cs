using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverManager : MonoBehaviour {
    [HideInInspector]
    public Dictionary<int , int> fullCovers;
    // Use this for initialization
    void Awake () {
        fullCovers = new Dictionary<int , int>();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
