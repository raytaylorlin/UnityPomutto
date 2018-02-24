using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Pomutto
{
    public class Block : MonoBehaviour
    {
        public enum EType
        {
            Red,
            Blue,
            Green,
            Yellow,
            Max
        }

        public enum EAnimationType
        {
            Normal,
            Fall,
            Fade
        }
        
        public Ease easeType;
        public float duration;
        public float offset = 64f;

        [Header("Debug")] 
        
        [ReadOnly] public EType BlockType;

        
        private EType m_Type;
        private SpriteRenderer m_Renderer;
        private Animator m_Animator;
        private Point m_LogicPosition;
        private static bool m_InitConfig = false;
        private static Dictionary<EAnimationType, string> m_AnimationMap = new Dictionary<EAnimationType, string>();
        private Action<Block> m_FadeAnimationCompletedCallback;
        
        private static Vector4 RED_HSL = new Vector4(0.35f, 0, 0, 1);
        private static Vector4 BLUE_HSL = new Vector4(0, 0, 0, 1);
        private static Vector4 GREEN_HSL = new Vector4(0.7f, 0, 0, 1);
        private static Vector4 YELLOW_HSL = new Vector4(0.5f, 0, 0, 1);

        private static Vector4[] HSL_MAP =
        {
            RED_HSL, BLUE_HSL, GREEN_HSL, YELLOW_HSL
        };

        public const int BLOCK_SIZE = 40;
        public const int HALF_BLOCK_SIZE = BLOCK_SIZE / 2;
        
        
        public Point LogicPosition
        {
            get { return m_LogicPosition; }
            set
            {
                m_LogicPosition = value;
                transform.localPosition = new Vector3(m_LogicPosition.x * BLOCK_SIZE, m_LogicPosition.y * BLOCK_SIZE);
//                transform.name = string.Format("Block(r{0}_c{1})", m_LogicPosition.y, m_LogicPosition.x);
            }
        }

        void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Animator = GetComponent<Animator>();

            if (!m_InitConfig)
            {
                InitAnimationMap();
                m_InitConfig = true;
            }
        }

        private void InitAnimationMap()
        {
            m_AnimationMap.Add(EAnimationType.Normal, "Normal");
            m_AnimationMap.Add(EAnimationType.Fall, "Fall");
            m_AnimationMap.Add(EAnimationType.Fade, "Fade");
        }

        public void Reset()
        {
//            PlayAnimation(EAnimationType.Normal);
            SetType(m_Type);
        }
        
        public void SetType(EType type)
        {
            BlockType = m_Type = type;
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Renderer.material.SetFloat("_Alpha", 1);
            m_Renderer.material.SetVector("_HSLAAdjust", HSL_MAP[(int) type]);
        }

        public void FastFall(int logicY, Action<Block> callback)
        {
            transform.DOLocalMoveY(Block.BLOCK_SIZE * logicY, 1f)
                .SetEase(Ease.OutSine)
                .OnComplete(delegate
                {
                    callback(this);
                });
        }

        public void PlayAnimation(EAnimationType type)
        {
            m_Animator.Play(m_AnimationMap[type]);
        }

        public void Fade(Action<Block> callback)
        {
            PlayAnimation(EAnimationType.Fade);
            m_FadeAnimationCompletedCallback = callback;
        }

        // Called from Animation event
        public void OnFadeAnimationCompleted()
        {
            if (m_FadeAnimationCompletedCallback != null)
            {
                m_FadeAnimationCompletedCallback(this);
            }
        }
    }
}

