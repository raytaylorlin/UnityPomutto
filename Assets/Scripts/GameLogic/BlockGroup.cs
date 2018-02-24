using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pomutto
{
	public class BlockGroup : MonoBehaviour
	{
		public delegate void BlockGroupStopDelegate(Point collisionPoint);
		public event BlockGroupStopDelegate OnBlockGroupStop;
		
		public float DownSpeed = -50f;
		public Ease EaseType;
		public float TweenDuration = 0.4f;

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
			if (InputManager.Instance.DownPress && m_Tweener == null)
			{
				downSpeed *= 8;
			}

			Vector3 pos = transform.localPosition;
			Vector2 targetPos = new Vector2(pos.x, pos.y + downSpeed);
			Vector2 testPos = new Vector2(targetPos.x, targetPos.y);
			Point testPoint = Controller.GetLogicPosition(testPos);
			Block testBlock = Controller.GetBlock(testPos);
			if (testPoint.y == -1 || testBlock != null)
			{
				if (OnBlockGroupStop != null)
				{
					Debug.Log("OnBlcokGroupStop " + testPoint);
					OnBlockGroupStop(testPoint);
				}
			}
			else
			{
				transform.localPosition = targetPos;
			}
		}

		private void CheckHorizontalMove()
		{
			if (m_Tweener != null || InputManager.Instance.DownPress)
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
				
			if (factor != 0 && m_Tweener == null)
			{
				m_Tweener = transform.DOLocalMoveX(Block.BLOCK_SIZE * factor, TweenDuration)
					.SetRelative(true)
					.SetEase(EaseType)
				    .OnComplete(OnTweenCompleted);
			}
		}

		private bool CanHorizontalMove(bool isLeftMove)
		{
			float currentX = transform.localPosition.x;
			if (isLeftMove && currentX - Block.HALF_BLOCK_SIZE < 0)
			{
				return false;
			}
			if (!isLeftMove && currentX + Block.HALF_BLOCK_SIZE > GameLogicController.MAP_WIDTH)
			{
				return false;
			}
			Vector3 pos = transform.localPosition;
			Vector2 testPos = new Vector2(pos.x + Block.BLOCK_SIZE * (isLeftMove ? -1 : 1), pos.y);
			if (Controller.GetBlock(testPos) != null)
			{
				return false;
			}
			return true;
		}

		private void CheckRotate()
		{
			if (InputManager.Instance.RotateClick && 
			    m_Tweener == null &&
			    !InputManager.Instance.DownPress)
			{
				m_Tweener = UpBlock.transform.DOLocalMoveY(-Block.BLOCK_SIZE, TweenDuration)
					.SetRelative(true)
					.SetEase(EaseType)
					.OnComplete(OnTweenCompleted);
				DownBlock.transform.DOLocalMoveY(Block.BLOCK_SIZE, TweenDuration)
					.SetRelative(true)
					.SetEase(EaseType);
				
				// 逻辑上交换上下方块
				Block temp = UpBlock;
				UpBlock = DownBlock;
				DownBlock = temp; 
			}
		}
		
		private void OnTweenCompleted()
		{
			m_Tweener = null;
		}
	}
}