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
        public void OnEnterBlockGroupResetState()
        {
            SwitchBlockGroup();
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
    }
}
