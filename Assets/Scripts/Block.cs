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
        
        public Ease easeType;
        public float duration;
        public float offset = 64f;

        [Header("Debug")] 
        
        [ReadOnly] public EType BlockType;

        private Tweener m_Tweener;
        private EType m_Type;
        private SpriteRenderer m_Renderer;
        
        private static Vector4 RED_HSL = new Vector4(0.35f, 0, 0, 0);
        private static Vector4 BLUE_HSL = new Vector4(0, 0, 0, 0);
        private static Vector4 GREEN_HSL = new Vector4(0.7f, 0, 0, 0);
        private static Vector4 YELLOW_HSL = new Vector4(0.5f, 0, 0, 0);

        private static Vector4[] HSL_MAP =
        {
            RED_HSL, BLUE_HSL, GREEN_HSL, YELLOW_HSL
        };
        
        void Start ()
        {
            DOTween.Init();
            
        }
	
        void Update ()
        {
            if (m_Tweener == null)
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    float x = transform.localPosition.x;
                    m_Tweener = transform.DOLocalMoveX(x - offset, duration).SetEase(easeType);
                    m_Tweener.OnComplete(OnTweenCompleted);
                }
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    float x = transform.localPosition.x;
                    m_Tweener = transform.DOLocalMoveX(x + offset, duration).SetEase(easeType);
                    m_Tweener.OnComplete(OnTweenCompleted);
                }
            }
        }

        private void OnTweenCompleted()
        {
            m_Tweener = null;
        }

        public void SetType(EType type)
        {
            BlockType = m_Type = type;
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Renderer.material.SetVector("_HSLAAdjust", HSL_MAP[(int) type]);
        }
    }
}

