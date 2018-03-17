using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PlayMakerFSMMethodAttribute : PropertyAttribute
    {
    }

    public partial class GameLogicController
    {
        private PlayMakerFSM m_GameLogicFSM;

        public void SendFSMEvent(string eventName)
        {
            m_GameLogicFSM.SendEvent(eventName);
        }

        [PlayMakerFSMMethod]
        public void OnEnterBlockGroupControlState()
        {
            Tick();
        }

        [PlayMakerFSMMethod]
        public void OnEnterBlockGroupResetState()
        {
            CheckSwitch();
        }

        [PlayMakerFSMMethod]
        public void OnEnterCheckClearBlockState()
        {
            CheckClearBlock();
        }

        [PlayMakerFSMMethod]
        public void OnEnterWaitFallBlocksState()
        {
            CheckFallBlock();
        }
        
        [PlayMakerFSMMethod]
        public void OnEnterGameOverState()
        {
            OnGameOver();
        }
    }
}
