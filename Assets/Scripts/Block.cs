using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Pomutto
{
    public class Block : MonoBehaviour
    {
        public Ease easeType;
        public float duration;
        public float offset = 64f;

        private Tweener m_Tweener;
        
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
    }
}

