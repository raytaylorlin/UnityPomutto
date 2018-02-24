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

		public GameLogicController Controller;
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

			Vector3 pos = transform.localPosition;
			Vector2 testPos = new Vector2(pos.x, pos.y + downSpeed);
			if (Controller.GetBlock(testPos) == null)
			{
				transform.localPosition = testPos;
			}
		}

		private void CheckHorizontalMove()
		{
			if (m_Tweener != null)
			{
				return;
			}

			int factor = 0;
			if (InputManager.Instance.LeftPress && CanHorizontalMove(true))
			{
				factor = -1;
			}
			else if (InputManager.Instance.RightPress && CanHorizontalMove(false))
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

		private bool CanHorizontalMove(bool isLeftMove)
		{
			float currentX = transform.localPosition.x;
			if (isLeftMove)
			{
				if (currentX - Block.HALF_BLOCK_SIZE < 0)
				{
					return false;
				}
				Vector3 pos = transform.localPosition;
				Vector2 testPos = new Vector2(pos.x - Block.BLOCK_SIZE, pos.y - Block.BLOCK_SIZE);
				if (Controller.GetBlock(testPos) != null)
				{
					return false;
				}
			}
			else
			{
				if (currentX + Block.HALF_BLOCK_SIZE > GameLogicController.MAP_WIDTH)
				{
					return false;
				}
				Vector3 pos = transform.localPosition;
				Vector2 testPos = new Vector2(pos.x + Block.BLOCK_SIZE, pos.y - Block.BLOCK_SIZE);
				if (Controller.GetBlock(testPos) != null)
				{
					return false;
				}
			}
			return true;
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