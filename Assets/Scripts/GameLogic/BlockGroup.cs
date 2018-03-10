using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pomutto
{
	public class BlockGroup : MonoBehaviour
	{
		public enum ERotateType
		{
			Up, Right, Down, Left
		}

		public float DownSpeed = -50f;
		public Ease EaseType;
		public float TweenDuration = 0.4f;

		public GameLogicController Controller;
		public Block SlaveBlock;
		public Block MasterBlock;

		[Header("Debug")]
		public ERotateType RotateType = ERotateType.Up;

		private Tweener m_Tweener;

		public Vector3 Position
		{
			get { return transform.localPosition; }
		}

		private bool IsHorizontal
		{
			get { return RotateType == ERotateType.Left || RotateType == ERotateType.Right; }
		}

		private bool IsVertical
		{
			get { return RotateType == ERotateType.Up || RotateType == ERotateType.Down; }
		}

		public BlockGroup(Block slave, Block master)
		{
			SlaveBlock = slave;
			MasterBlock = master;
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

			if (RotateType == ERotateType.Up)
			{
				Point testPoint;
				var wp = MasterBlock.transform.TransformPoint(MasterBlock.transform.localPosition);
				var mp = transform.InverseTransformPoint(wp);
				Vector2 testPos = new Vector2(MasterBlock.transform.localPosition.x, MasterBlock.transform.localPosition.y + downSpeed);
				if (TestPositionCanStop(testPos, out testPoint))
				{
					Controller.StopBlock(SlaveBlock, new Point(testPoint.x, testPoint.y + 2));
					Controller.StopBlock(MasterBlock, new Point(testPoint.x, testPoint.y + 1));
					Controller.SendFSMEvent(Events.FSMEvent.StopTwoBlock);
				}
				else
				{
					transform.Translate(0, downSpeed, 0);
				}

			}
			else if (RotateType == ERotateType.Down)
			{
				Point testPoint;
				Vector2 test = new Vector2(SlaveBlock.transform.localPosition.x, MasterBlock.transform.localPosition.y + downSpeed);
				if (TestPositionCanStop(test, out testPoint))
				{
					Controller.StopBlock(MasterBlock, new Point(testPoint.x, testPoint.y + 2));
					Controller.StopBlock(SlaveBlock, new Point(testPoint.x, testPoint.y + 1));
					Controller.SendFSMEvent(Events.FSMEvent.StopTwoBlock);
				}
				else
				{
					transform.Translate(0, downSpeed, 0);
				}
			}
			else
			{
				int stopBlockCount = 0;
				Point testPoint;
				Vector2 test = new Vector2(MasterBlock.transform.localPosition.x, MasterBlock.transform.localPosition.y + downSpeed);
				if (TestPositionCanStop(test, out testPoint))
				{
					Controller.StopBlock(MasterBlock, new Point(testPoint.x, testPoint.y + 1));
					stopBlockCount += 1;
				}
				test = new Vector2(SlaveBlock.transform.localPosition.x, SlaveBlock.transform.localPosition.y + downSpeed);
				if (TestPositionCanStop(test, out testPoint))
				{
					Controller.StopBlock(SlaveBlock, new Point(testPoint.x, testPoint.y + 2));
					stopBlockCount += 1;
				}

				Debug.Log("stopBlockCount: " + stopBlockCount);
				if (stopBlockCount == 0)
				{
					SlaveBlock.transform.Translate(0, downSpeed, 0);
					MasterBlock.transform.Translate(0, downSpeed, 0);
				}
				else if (stopBlockCount == 1)
				{
					Controller.SendFSMEvent(Events.FSMEvent.StopOneBlock);
				}
				else
				{
					Controller.SendFSMEvent(Events.FSMEvent.StopTwoBlock);
				}
			}

//			Vector3 pos = transform.localPosition;
////			Vector3 pos = Position;
//			Vector2 targetPos = new Vector2(pos.x, pos.y + downSpeed);
//			Vector2 testPos = new Vector2(targetPos.x, targetPos.y);
//			Point testPoint = Controller.GetLogicPosition(testPos);
//			Block testBlock = Controller.GetBlock(testPos);
//			if (testPoint.y == -1 || testBlock != null)
//			{
////				if (RotateType == ERotateType.Up)
////				{
////
////				}
////				else if (RotateType == ERotateType.Down)
////				{
//					Controller.StopBlock(MasterBlock, new Point(testPoint.x, testPoint.y + 2));
//					Controller.StopBlock(SlaveBlock, new Point(testPoint.x, testPoint.y + 1));
////				}
//
//				Controller.SendFSMEvent(Events.FSMEvent.StopBlock);
//			}
//			else
//			{
//
//				transform.Translate(0, downSpeed, 0);
//			}
		}

		private bool TestPositionCanStop(Vector2 testPos, out Point testPoint)
		{
			testPoint = Controller.GetLogicPosition(testPos);
			Block testBlock = Controller.GetBlock(testPos);
			return testPoint.y == -1 || testBlock != null;
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
			if (!InputManager.Instance.RotateClick ||
			    m_Tweener != null ||
			    InputManager.Instance.DownPress)
			{
				return;
			}

			float currentX = transform.localPosition.x;
			// 贴左边界，从下往左旋转的情况
//			if (currentX - Block.HALF_BLOCK_SIZE < 0 &&
                //				RotateType == ERotateType.Down)
                //			{
                //				m_Tweener = SlaveBlock.transform.DOLocalMoveY(Block.BLOCK_SIZE, TweenDuration)
                //					.SetRelative(true)
                //					.SetEase(Ease.OutQuad)
                //					.OnComplete(OnTweenCompleted);
                //				MasterBlock.transform.DOLocalMoveX(Block.BLOCK_SIZE, TweenDuration)
                //					.SetRelative(true)
                //					.SetEase(Ease.OutQuad);
                //			}
                //			// 贴右边界，从上往右旋转的情况
                //			else if (currentX + Block.HALF_BLOCK_SIZE > GameLogicController.MAP_WIDTH &&
                //					 RotateType == ERotateType.Up)
                //			{
                //				m_Tweener = SlaveBlock.transform.DOLocalMoveY(-Block.BLOCK_SIZE, TweenDuration)
                //					.SetRelative(true)
                //					.SetEase(Ease.OutQuad)
                //					.OnComplete(OnTweenCompleted);
                //				MasterBlock.transform.DOLocalMoveX(-Block.BLOCK_SIZE, TweenDuration)
                //					.SetRelative(true)
                //					.SetEase(Ease.OutQuad);
                //			}
                //			else
			{
				int xFactor = 1;
				int yFactor = 1;
				Ease xEase = Ease.OutQuad;
				Ease yEase = Ease.OutQuad;
				switch (RotateType)
				{
					case ERotateType.Up:
						xFactor = 1;
						yFactor = -1;
						xEase = Ease.OutQuad;
						yEase = Ease.InQuad;
						break;
					case ERotateType.Right:
						xFactor = -1;
						yFactor = -1;
						xEase = Ease.InQuad;
						yEase = Ease.OutQuad;
						break;
					case ERotateType.Down:
						xFactor = -1;
						yFactor = 1;
						xEase = Ease.OutQuad;
						yEase = Ease.InQuad;
						break;
					case ERotateType.Left:
						xFactor = 1;
						yFactor = 1;
						xEase = Ease.InQuad;
						yEase = Ease.OutQuad;
						break;
				}
				// 从方块做一个圆弧轨迹旋转
				m_Tweener = SlaveBlock.transform.DOLocalMoveX(Block.BLOCK_SIZE * xFactor, TweenDuration)
					.SetRelative(true)
					.SetEase(xEase)
					.OnComplete(OnTweenCompleted);
				SlaveBlock.transform.DOLocalMoveY(Block.BLOCK_SIZE * yFactor, TweenDuration)
					.SetRelative(true)
					.SetEase(yEase);
			}

			RotateType = (ERotateType) (((int) RotateType + 1) % 4);
		}
		
		private void OnTweenCompleted()
		{
			m_Tweener = null;
		}
	}
}