using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pomutto
{
	public class BlockGroup : MonoBehaviour
	{
		public float DownSpeed = -50f;
		public Ease EaseType;
		public float HorizontalDuration = 0.4f;
		
		public Block UpBlock;
		public Block DownBlock;
		
		private Tweener m_Tweener;

		public BlockGroup(Block up, Block down)
		{
			UpBlock = up;
			DownBlock = down;
		}

		public void Tick()
		{
			CheckDownMove();
			CheckHorizontalMove();
			CheckRotate();
		}

		private void CheckDownMove()
		{
			float downSpeed = DownSpeed * Time.deltaTime;
			if (InputManager.Instance.DownPress)
			{
				downSpeed *= 8;
			}

//			Vector3 pos = Vector3.zero;
//			pos = UpBlock.transform.localPosition;
//			UpBlock.transform.localPosition = new Vector3(pos.x, pos.y + downSpeed);
//			pos = DownBlock.transform.localPosition;
//			DownBlock.transform.localPosition = new Vector3(pos.x, pos.y + downSpeed);

			Vector3 pos = transform.localPosition;
			transform.localPosition = new Vector3(pos.x, pos.y + downSpeed);
		}

		private void CheckHorizontalMove()
		{
			if (m_Tweener != null)
			{
				return;
			}

			int factor = 0;
			if (InputManager.Instance.LeftPress)
			{
				factor = -1;
			}
			else if (InputManager.Instance.RightPress)
			{
				factor = 1;
			}
				
			if (factor != 0)
			{
				float x = transform.localPosition.x;
				m_Tweener = transform.DOLocalMoveX(x + Block.BLOCK_SIZE * factor, HorizontalDuration)
					.SetEase(EaseType)
				    .OnComplete(OnTweenCompleted);
			}
		}
		
		private void OnTweenCompleted()
		{
			m_Tweener = null;
		}

		private void CheckRotate()
		{
			
		}
	}
}