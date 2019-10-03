using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLSpace
{
    public class RagdolTest : MonoBehaviour
    {

        private RagdollManager rMan;
        private enemy_ai ai;

        void Start()
        {
            rMan = GetComponent<RagdollManager>();
            ai = GetComponent<enemy_ai>();


        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.F))
            {
                ai.lStates = LifeStates.Dead;
                rMan.startRagdoll();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                rMan.blendToMecanim();
            }

        }
    }
}
