using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pomutto.Config;
using UnityEngine;

namespace Pomutto
{
	public class BlockGroup : MonoBehaviour
	{
		public enum ERotateType
		{
			Up, Right, Down, Left
		}

		private class RotateTweenParam
		{
			public int x;
			public int y;
			public Ease xEase;
			public Ease yEase;
		}


		public GameLogicController Controller;
		public Block SlaveBlock;
		public Block MasterBlock;

		private float downSpeed;
		private float tweenDuration;

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

		void Start()
		{
			downSpeed = ConfigManager.Instance.BlockConfig.BlockGroupDownSpeed;
			tweenDuration = ConfigManager.Instance.BlockConfig.BlockGroupTweenDuration;
		}

		public void Tick()
		{
			CheckDownMove();
			CheckHorizontalMove();
			CheckRotate();
		}

		private void CheckDownMove()
		{
			float ds = downSpeed * Time.deltaTime;
			if (InputManager.Instance.DownPress && m_Tweener == null)
			{
				ds *= 8;
			}

			if (IsRotateVertical)
			{
				Point testPoint;
				Vector2 downBlockPos = transform.localPosition + DownBlock.transform.localPosition;
				Vector2 testPos = new Vector2(downBlockPos.x, downBlockPos.y + ds);
				if (!TestPositionValid(testPos, out testPoint))
				{
					Controller.StopBlock(UpBlock, new Point(testPoint.x, testPoint.y + 2));
					Controller.StopBlock(DownBlock, new Point(testPoint.x, testPoint.y + 1));
					Controller.SendFSMEvent(Events.FSMEvent.StopTwoBlock);
				}
				else
				{
					transform.Translate(0, ds, 0);
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
				Vector2 testPos = new Vector2(pos.x, pos.y + ds);
				if (!TestPositionValid(testPos, out testPoint))
				{
					Controller.StopBlock(MasterBlock, new Point(testPoint.x, testPoint.y + 1));
					stopBlockCount += 1;
					other = SlaveBlock;
					otherPoint = new Point(testPoint.x + (RotateType == ERotateType.Left ? -1 : 1), testPoint.y + 1);
				}

				pos = transform.localPosition + SlaveBlock.transform.localPosition;
				testPos = new Vector2(pos.x, pos.y + ds);
				if (!TestPositionValid(testPos, out testPoint))
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
					transform.Translate(0, ds, 0);
				}
			}
		}

		private bool TestPositionValid(Vector2 testPos, out Point testPoint)
		{
			testPoint = Controller.GetLogicPosition(testPos);
			Block testBlock = Controller.GetBlock(testPos);
			return testPoint.x >= 0 && testPoint.x < GameLogicController.LOGIC_WIDTH &&
			       testPoint.y >= 0 && testPoint.y < GameLogicController.LOGIC_HEIGHT &&
			       testBlock == null;
		}

		private bool TestPositionValid(Vector2 testPos)
		{
			Point testPoint;
			return TestPositionValid(testPos, out testPoint);
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
				Debug.Log(string.Format("HorizontalMove: {0}", factor));	
				m_Tweener = transform.DOLocalMoveX(Block.BLOCK_SIZE * factor, tweenDuration)
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
				return TestPositionValid(testPos);
			}
			else
			{
				Block checkBlock = IsRotateHorizontal ? RightBlock : DownBlock;
				Vector2 blockPos = GetMapPosition(checkBlock);
				Vector2 testPos = new Vector2(blockPos.x + Block.BLOCK_SIZE, blockPos.y);
				return TestPositionValid(testPos);
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

			RotateTweenParam slaveParam = null;
			RotateTweenParam masterParam = null;
			// 贴左边界，从下往左旋转的情况
			switch (RotateType)
			{
				case ERotateType.Up:
					if (CanHorizontalMove(false))
					{
						slaveParam = new RotateTweenParam()
						{
							x = 1,
							y = -1,
							xEase = OUT_EASE_TYPE,
							yEase = IN_EASE_TYPE
						};
					}
					else if (CanHorizontalMove(true))
					{
						slaveParam = new RotateTweenParam()
						{
							x = 0,
							y = -1,
							yEase = OUT_EASE_TYPE
						};
						masterParam = new RotateTweenParam()
						{
							x = -1,
							y = 0,
							xEase = OUT_EASE_TYPE
						};
					}

					break;
				case ERotateType.Right:
					Vector2 blockPos = GetMapPosition(MasterBlock);
					Vector2 testPos1 = new Vector2(blockPos.x, blockPos.y - Block.BLOCK_SIZE);
					Vector2 testPos2 = new Vector2(blockPos.x + Block.BLOCK_SIZE, blockPos.y - Block.BLOCK_SIZE);
					if (TestPositionValid(testPos1) && TestPositionValid(testPos2))
					{
						slaveParam = new RotateTweenParam()
						{
							x = -1,
							y = -1,
							xEase = IN_EASE_TYPE,
							yEase = OUT_EASE_TYPE
						};
					}
					else
					{
						slaveParam = new RotateTweenParam()
						{
							x = -1,
							y = 0,
							xEase = OUT_EASE_TYPE
						};
						masterParam = new RotateTweenParam()
						{
							x = 0,
							y = 1,
							yEase = OUT_EASE_TYPE
						};
					}

					break;
				case ERotateType.Down:
					if (CanHorizontalMove(true))
					{
						slaveParam = new RotateTweenParam()
						{
							x = -1,
							y = 1,
							xEase = OUT_EASE_TYPE,
							yEase = IN_EASE_TYPE
						};
					}
					else if (CanHorizontalMove(false))
					{
						slaveParam = new RotateTweenParam()
						{
							x = 0,
							y = 1,
							yEase = OUT_EASE_TYPE
						};
						masterParam = new RotateTweenParam()
						{
							x = 1,
							y = 0,
							xEase = OUT_EASE_TYPE
						};
					}

					break;
				case ERotateType.Left:
					slaveParam = new RotateTweenParam()
					{
						x = 1,
						y = 1,
						xEase = IN_EASE_TYPE,
						yEase = OUT_EASE_TYPE
					};
					break;
			}

			if (slaveParam != null)
				m_Tweener = DoBlockTween(SlaveBlock, slaveParam);
			if (masterParam != null)
				m_Tweener = DoBlockTween(MasterBlock, masterParam);

			if (m_Tweener != null)
				m_Tweener.OnComplete(OnRotateTweenCompleted);

			if (slaveParam != null || masterParam != null)
				RotateType = (ERotateType) (((int) RotateType + 1) % 4);
		}

		private Tweener DoBlockTween(Block block, RotateTweenParam param)
		{
			Debug.Log(string.Format("{0}: x = {1}, xEase = {2}, y = {3}, yEase = {4}",
				block, param.x, param.xEase, param.y, param.yEase));
			Tweener tweener = null;
			if (param.x != 0)
			{
				tweener = block.transform.DOLocalMoveX(Block.BLOCK_SIZE * param.x, tweenDuration)
					.SetRelative(true).SetEase(param.xEase);
			}
			if (param.y != 0)
			{
				tweener = block.transform.DOLocalMoveY(Block.BLOCK_SIZE * param.y, tweenDuration)
					.SetRelative(true).SetEase(param.yEase);
			}
			return tweener;
		}

		private void OnRotateTweenCompleted()
		{
			
			OnTweenCompleted();
		}
		
		private void OnTweenCompleted()
		{
			m_Tweener = null;
		}
	}
}