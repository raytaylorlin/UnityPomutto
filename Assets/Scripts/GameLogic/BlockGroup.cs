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
		public float TweenDuration = 0.4f;

		public GameLogicController Controller;
		public Block SlaveBlock;
		public Block MasterBlock;

		private const Ease OUT_EASE_TYPE = Ease.OutQuad;
		private const Ease IN_EASE_TYPE = Ease.InQuad;

		[Header("Debug")]
		public ERotateType RotateType = ERotateType.Up;

		private Tweener m_Tweener;

		public Vector3 Position
		{
			get { return transform.localPosition; }
		}

		private Block UpBlock
		{
			get
			{
				if (IsRotateVertical)
				{
					return RotateType == ERotateType.Up ? SlaveBlock : MasterBlock;
				}
				return null;
			}
		}
		
		private Block DownBlock
		{
			get
			{
				if (IsRotateVertical)
				{
					return RotateType == ERotateType.Up ? MasterBlock : SlaveBlock;
				}
				return null;
			}
		}
		
		private Block LeftBlock
		{
			get
			{
				if (IsRotateHorizontal)
				{
					return RotateType == ERotateType.Left ? SlaveBlock : MasterBlock;
				}
				return null;
			}
		}
		
		private Block RightBlock
		{
			get
			{
				if (IsRotateHorizontal)
				{
					return RotateType == ERotateType.Left ? MasterBlock : SlaveBlock;
				}
				return null;
			}
		}

		private bool IsRotateHorizontal
		{
			get { return RotateType == ERotateType.Left || RotateType == ERotateType.Right; }
		}

		private bool IsRotateVertical
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

			if (IsRotateVertical)
			{
				Point testPoint;
				Vector2 downBlockPos = transform.localPosition + DownBlock.transform.localPosition;
				Vector2 testPos = new Vector2(downBlockPos.x, downBlockPos.y + downSpeed);
				if (!TestPositionValid(testPos, true, out testPoint))
				{
					Controller.StopBlock(UpBlock, new Point(testPoint.x, testPoint.y + 2));
					Controller.StopBlock(DownBlock, new Point(testPoint.x, testPoint.y + 1));
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
				Block other = null;
				Point otherPoint = new Point();
				var pos = transform.localPosition + MasterBlock.transform.localPosition;
//				Vector2 test = new Vector2(MasterBlock.transform.localPosition.x, MasterBlock.transform.localPosition.y + downSpeed);
				Vector2 testPos = new Vector2(pos.x, pos.y + downSpeed);
				if (!TestPositionValid(testPos, true, out testPoint))
				{
					Controller.StopBlock(MasterBlock, new Point(testPoint.x, testPoint.y + 1));
					stopBlockCount += 1;
					other = SlaveBlock;
					otherPoint = new Point(testPoint.x + (RotateType == ERotateType.Left ? -1 : 1), testPoint.y + 1);
				}

				pos = transform.localPosition + SlaveBlock.transform.localPosition;
				testPos = new Vector2(pos.x, pos.y + downSpeed);
				if (!TestPositionValid(testPos, true, out testPoint))
				{
					Controller.StopBlock(SlaveBlock, new Point(testPoint.x, testPoint.y + 1));
					stopBlockCount += 1;
					other = MasterBlock;
					otherPoint = new Point(testPoint.x + (RotateType == ERotateType.Left ? 1 : -1), testPoint.y + 1);
				}

				if (stopBlockCount > 0)
				{
					Debug.Log("stopBlockCount: " + stopBlockCount);
					if (stopBlockCount == 1)
					{
						if (other != null)
						{
							Controller.StopBlock(other, otherPoint, false);
						}

						Controller.SendFSMEvent(Events.FSMEvent.StopOneBlock);
					}
					else
					{
						Controller.SendFSMEvent(Events.FSMEvent.StopTwoBlock);
					}
				}
				else
				{
					transform.Translate(0, downSpeed, 0);
				}
			}
		}

		private bool TestPositionValid(Vector2 testPos, bool isVertical, out Point testPoint)
		{
			testPoint = Controller.GetLogicPosition(testPos);
			Block testBlock = Controller.GetBlock(testPos);
			if (isVertical)
			{
				return testPoint.y >= 0 && testPoint.y < GameLogicController.LOGIC_HEIGHT &&
				    testBlock == null;
			}
			else
			{
				return testPoint.x >= 0 && testPoint.x < GameLogicController.LOGIC_WIDTH &&
					testBlock == null;
			}
		}
		
		private bool TestPositionValid(Vector2 testPos, bool isVertical)
		{
			Point testPoint;
			return TestPositionValid(testPos, isVertical, out testPoint);
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
					.SetEase(OUT_EASE_TYPE)
				    .OnComplete(OnTweenCompleted);
			}
		}

		private bool CanHorizontalMove(bool isLeftMove)
		{
			if (isLeftMove)
			{
				Block checkBlock = IsRotateHorizontal ? LeftBlock : DownBlock;
				Vector2 blockPos = GetMapPosition(checkBlock);
				Vector2 testPos = new Vector2(blockPos.x - Block.BLOCK_SIZE, blockPos.y);
				return TestPositionValid(testPos, false);
			}
			else
			{
				Block checkBlock = IsRotateHorizontal ? RightBlock : DownBlock;
				Vector2 blockPos = GetMapPosition(checkBlock);
				Vector2 testPos = new Vector2(blockPos.x + Block.BLOCK_SIZE, blockPos.y);
				return TestPositionValid(testPos, false);
			}
		}

		private Vector2 GetMapPosition(Block block)
		{
			return transform.localPosition + block.transform.localPosition;
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
			if (currentX - Block.HALF_BLOCK_SIZE < 0 &&
				RotateType == ERotateType.Down)
			{
				m_Tweener = SlaveBlock.transform.DOLocalMoveY(Block.BLOCK_SIZE, TweenDuration)
					.SetRelative(true)
					.SetEase(Ease.OutQuad)
					.OnComplete(OnTweenCompleted);
				MasterBlock.transform.DOLocalMoveX(Block.BLOCK_SIZE, TweenDuration)
					.SetRelative(true)
					.SetEase(Ease.OutQuad);
			}
			// 贴右边界，从上往右旋转的情况
			else if (currentX + Block.HALF_BLOCK_SIZE > GameLogicController.MAP_WIDTH &&
					 RotateType == ERotateType.Up)
			{
				m_Tweener = SlaveBlock.transform.DOLocalMoveY(-Block.BLOCK_SIZE, TweenDuration)
					.SetRelative(true)
					.SetEase(Ease.OutQuad)
					.OnComplete(OnTweenCompleted);
				MasterBlock.transform.DOLocalMoveX(-Block.BLOCK_SIZE, TweenDuration)
					.SetRelative(true)
					.SetEase(Ease.OutQuad);
			}
			else
			{
				int xFactor = 1;
				int yFactor = 1;
				Ease xEase = OUT_EASE_TYPE;
				Ease yEase = OUT_EASE_TYPE;
				switch (RotateType)
				{
					case ERotateType.Up:
						xFactor = 1;
						yFactor = -1;
						xEase = OUT_EASE_TYPE;
						yEase = IN_EASE_TYPE;
						break;
					case ERotateType.Right:
						xFactor = -1;
						yFactor = -1;
						xEase = IN_EASE_TYPE;
						yEase = OUT_EASE_TYPE;
						break;
					case ERotateType.Down:
						xFactor = -1;
						yFactor = 1;
						xEase = OUT_EASE_TYPE;
						yEase = IN_EASE_TYPE;
						break;
					case ERotateType.Left:
						xFactor = 1;
						yFactor = 1;
						xEase = IN_EASE_TYPE;
						yEase = OUT_EASE_TYPE;
						break;
				}
				// 从方块做一个圆弧轨迹旋转
				m_Tweener = SlaveBlock.transform.DOLocalMoveX(Block.BLOCK_SIZE * xFactor, TweenDuration)
					.SetRelative(true)
					.SetEase(xEase)
					.OnComplete(OnRotateTweenCompleted);
				SlaveBlock.transform.DOLocalMoveY(Block.BLOCK_SIZE * yFactor, TweenDuration)
					.SetRelative(true)
					.SetEase(yEase);
			}
		}

		private void OnRotateTweenCompleted()
		{
			RotateType = (ERotateType) (((int) RotateType + 1) % 4);
			OnTweenCompleted();
		}
		
		private void OnTweenCompleted()
		{
			m_Tweener = null;
		}
	}
}