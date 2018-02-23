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

        
        private EType m_Type;
        private SpriteRenderer m_Renderer;
        private Vector2 m_LogicPosition;
        
        private static Vector4 RED_HSL = new Vector4(0.35f, 0, 0, 0);
        private static Vector4 BLUE_HSL = new Vector4(0, 0, 0, 0);
        private static Vector4 GREEN_HSL = new Vector4(0.7f, 0, 0, 0);
        private static Vector4 YELLOW_HSL = new Vector4(0.5f, 0, 0, 0);

        private static Vector4[] HSL_MAP =
        {
            RED_HSL, BLUE_HSL, GREEN_HSL, YELLOW_HSL
        };

        public static int BLOCK_SIZE = 40;
        
        
        public Vector2 LogicPosition
        {
            get { return m_LogicPosition; }
            set
            {
                m_LogicPosition = value;
                transform.localPosition = new Vector3(m_LogicPosition.y * BLOCK_SIZE, m_LogicPosition.x * BLOCK_SIZE);
                transform.name = string.Format("Block(r{0}_c{1})", m_LogicPosition.x, m_LogicPosition.y);
            }
        }
        
        void Start ()
        {
            DOTween.Init();
            
        }
	
        void Update ()
        {
        }

        

        public void SetType(EType type)
        {
            BlockType = m_Type = type;
            m_Renderer = GetComponent<SpriteRenderer>();
            m_Renderer.material.SetVector("_HSLAAdjust", HSL_MAP[(int) type]);
        }

    }
}

