using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PathologicalGames;
using UnityEngine;

namespace Pomutto
{
	public class Point
	{
		public int x;
		public int y;

		public Point()
		{
			x = 0;
			y = 0;
		}

		public Point(int _x, int _y)
		{
			x = _x;
			y = _y;
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", x, y);
		}
	}
	
	public partial class GameLogicController : MonoBehaviour
	{
		public GameObject BlockPrefab;
		public Transform MapTransform;
		public BlockGroup CurrentGroup;
		public BlockGroup NextGroup;

		public const int LOGIC_WIDTH = 7;
		public const int LOGIC_REAL_HEIGHT = 12;
		public const int LOGIC_EXTEND_HEIGHT = 4;
		public const int LOGIC_HEIGHT = LOGIC_REAL_HEIGHT + LOGIC_EXTEND_HEIGHT;
		public const int MAP_WIDTH = Block.BLOCK_SIZE * (LOGIC_WIDTH - 1);
		public static Point GAMEOVER_POINT = new Point(LOGIC_WIDTH / 2, LOGIC_REAL_HEIGHT - 1);

		private List<List<Block>> m_Map;
		private Queue<Block> m_CheckClearQueue = new Queue<Block>();
		private HashSet<Block> m_BlockHasChecked = new HashSet<Block>();
		private List<List<Block>> m_ClearSetList = new List<List<Block>>();
		private HashSet<Block> m_FastFallBlocks = new HashSet<Block>();
		private HashSet<Block> m_FadingBlocks = new HashSet<Block>();
		private Vector3 m_InitGroupPosition;

		void Start()
		{
			DG.Tweening.DOTween.Init();
			
			InitBlocks();
			InitGroup();

			m_GameLogicFSM = GetComponent<PlayMakerFSM>();
		}

		private void InitBlocks()
		{
			m_Map = new List<List<Block>>(LOGIC_HEIGHT);
			for (int j = 0; j < LOGIC_HEIGHT; j++)
			{
				List<Block> row = new List<Block>(LOGIC_WIDTH);
				for (int i = 0; i < LOGIC_WIDTH; i++)
				{
					row.Add(null);
				}
				m_Map.Add(row);
			}

			List<List<int>> mapData = TestData.GenerateMapData();
			for (int j = 0; j < mapData.Count; j++)
			{
				for (int i = 0; i < mapData[j].Count; i++)
				{
					var data = mapData[j][i];
					if (data == -1)
					{
						continue;
					}
					Block block = BlockPool.Instance.Spawn(BlockPrefab, MapTransform, data);
					SetMap(i, j, block);
				}
			}
		}

		#region 工具方法

		private void SetMap(int x, int y, Block block)
		{
			if (block != null)
			{
				block.LogicPosition = new Point(x, y);
			}
			m_Map[y][x] = block;
		}

		private void SetMap(Point point, Block block)
		{
			SetMap(point.x, point.y, block);
		}

		private Block GetMap(int x, int y)
		{
			return m_Map[y][x];
		}
		
		private Block GetMap(Point point)
		{
			return GetMap(point.x, point.y);
		}

		#endregion

		private void InitGroup()
		{
			m_InitGroupPosition = CurrentGroup.transform.localPosition;
			CreateBlockGroup(NextGroup);
		}

		private void CreateBlockGroup(BlockGroup group)
		{
			group.SlaveBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform, 0);
			group.SlaveBlock.LogicPosition = new Point(0, 1);
			group.MasterBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform, 3);
			group.MasterBlock.LogicPosition = new Point(0, 0);
		}

		private void CheckSwitch()
		{
			if (GetMap(GAMEOVER_POINT) != null)
			{
				SendFSMEvent(Events.FSMEvent.GameOver);
			}
			else
			{
				SwitchBlockGroup();
			}
		}

		private void SwitchBlockGroup()
		{
			CurrentGroup.transform.localPosition = m_InitGroupPosition;
			CurrentGroup.SlaveBlock = NextGroup.SlaveBlock;
			CurrentGroup.SlaveBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.SlaveBlock.LogicPosition = new Point(0, 1);
			CurrentGroup.MasterBlock = NextGroup.MasterBlock;
			CurrentGroup.MasterBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.MasterBlock.LogicPosition = new Point(0, 0);
			CurrentGroup.RotateType = BlockGroup.ERotateType.Up;

			CurrentGroup.MasterBlock.Blink();
			CreateBlockGroup(NextGroup);
			
			SendFSMEvent(Events.FSMEvent.BlockGroupReset);
		}
		
		public void Tick()
		{
			if (m_GameLogicFSM.ActiveStateName == "BlockGroupControl")
			{
				InputManager.Instance.Tick();
				CurrentGroup.Tick();
			}
		}

		public Point GetLogicPosition(Vector2 realPos)
		{
			realPos.x -= realPos.x < 0 ? Block.BLOCK_SIZE : 0;
			realPos.y -= realPos.y < 0 ? Block.BLOCK_SIZE : 0;
			return new Point((int) (realPos.x / Block.BLOCK_SIZE),
				(int) (realPos.y / Block.BLOCK_SIZE));
		}

		public Block GetBlock(Vector2 realPos)
		{
			Point logicPos = GetLogicPosition(realPos);
			if (logicPos.x >= 0 && logicPos.x < LOGIC_WIDTH &&
			    logicPos.y >= 0 && logicPos.y < LOGIC_HEIGHT)
			{
				return m_Map[logicPos.y][logicPos.x];
			}
			return null;
		}

		public void StopBlock(Block block, Point finalPoint, bool realStop = true)
		{
			Debug.Log(string.Format("Stop block {0} at {1}", block, finalPoint));
			block.Stop();
			block.transform.SetParent(MapTransform);
			SetMap(finalPoint.x, finalPoint.y, block);
			if (realStop)
			{
				block.PlayAnimation(Block.EAnimationType.Fall);
				// 添加到检查队列，等待下轮的检测
				m_CheckClearQueue.Enqueue(block);
			}
		}

		public void CheckClearBlock()
		{
			/* 1.查找所有待清除的方块 */
			// 清空待清除的方块集合列表
			m_ClearSetList.Clear();
			m_BlockHasChecked.Clear();

			// 由于存在连续消除的情况，判定消除结束（切换到下一个方块组）的条件是：检查队列为空
			while (m_CheckClearQueue.Count > 0)
			{
				//取出开放列表中的一个方块
				Block checkBlock = m_CheckClearQueue.Dequeue();
				//初始化待清除方块集合
				var clearSet = new List<Block> {checkBlock};

				// 开始深度优先搜索
				CheckArround(clearSet, checkBlock);
				// 同色的方块不少于3个，则将这个集合加入到列表
				if (clearSet.Count >= 3)
				{
					m_ClearSetList.Add(clearSet);
				}
				// 否则重置搜索过的方块为“未检查”状态
				else
				{
					for (int i = 0; i < clearSet.Count; i++)
					{
						m_BlockHasChecked.Remove(clearSet[i]);
					}
				}
			}
			
			/* 2.清除所有需要清除的方块 */
			if (m_ClearSetList.Count > 0)
			{
				m_FadingBlocks.Clear();
				// 遍历每个待清除方块集合，并对所有方块清除
				for (int i = 0; i < m_ClearSetList.Count; i++)
				{
					var set = m_ClearSetList[i];
					int clearBlockCount = 0;

					for (int j = 0; j < set.Count; j++)
					{
						ClearBlock(set[j]);
						//对消除的方块数计数
						clearBlockCount += 1;
					}
//					this._gameScore.addScore(100 * count);
				}
			}
			else
			{
				SendFSMEvent(Events.FSMEvent.ClearBlocksCompleted);
			}
		}

		/// <summary>
		/// 深度优先搜索相邻的所有同色方块
		/// </summary>
		/// <param name="clearSet">同色方块集合</param>
		/// <param name="checkBlock">待检查的方块</param>
		private void CheckArround(List<Block> clearSet, Block checkBlock)
		{
			Point logicPos = checkBlock.LogicPosition;
			int x = logicPos.x;
			int y = logicPos.y;

			// 获取周围的4个方块（依次为左、右、上、下）
			var arroundBlockList = new List<Block>();
			if (y >= 0)
			{
				arroundBlockList.Add(x - 1 >= 0 ? GetMap(x - 1, y) : null);
				arroundBlockList.Add(x + 1 < LOGIC_WIDTH ? GetMap(x + 1, y) : null);
				arroundBlockList.Add(y + 1 < LOGIC_HEIGHT ? GetMap(x, y + 1) : null);
				arroundBlockList.Add(y - 1 >= 0 ? GetMap(x, y - 1) : null);
			}

			// 将方块标记为“已检查”状态，以免重复检测
			m_BlockHasChecked.Add(checkBlock);
			for (int i = 0; i < arroundBlockList.Count; i++)
			{
				var arroundBlock = arroundBlockList[i];
				// 纳入列表的条件：不为空，未检查过，颜色相同
				if (arroundBlock != null && !m_BlockHasChecked.Contains(arroundBlock) &&
				    arroundBlock.BlockType == checkBlock.BlockType)
				{
					clearSet.Add(arroundBlock);
					//递归检查下一个方块
					CheckArround(clearSet, arroundBlock);
				}
			}
		}

		/// <summary>
		/// 执行清除方块逻辑
		/// </summary>
		/// <param name="block">待清除的方块</param>
		private void ClearBlock(Block block)
		{
			Point logicPos = block.LogicPosition;
			m_FadingBlocks.Add(block);
			block.Fade(BlockFadeCallback);
			if (GetMap(logicPos) != null)
			{
				SetMap(logicPos, null);
			}
		}

		/// <summary>
		/// 方块消失动画完成的回调
		/// </summary>
		/// <param name="block">执行动画的方块</param>
		private void BlockFadeCallback(Block block)
		{
			block.Reset();
			BlockPool.Instance.Despawn(block.gameObject);

			m_FadingBlocks.Remove(block);
			if (m_FadingBlocks.Count == 0)
			{
				SendFSMEvent(Events.FSMEvent.ClearBlocksCompleted);
			}
		}

		/// <summary>
		/// 查找空隙并让悬空的方块掉落
		/// </summary>
		public void CheckFallBlock()
		{
			m_FastFallBlocks.Clear();
			// 一列一列地查找空隙
			for (int i = 0; i < LOGIC_WIDTH; i++)
			{
				int fallY = 0;
				// 从最底部往上查找
				for (int j = 0; j < LOGIC_HEIGHT; j++)
				{
					var checkBlock = GetMap(i, j);
					if (checkBlock != null)
					{
						var logicPos = checkBlock.LogicPosition;
						if (logicPos.y > fallY)
						{
							m_FastFallBlocks.Add(checkBlock);
							checkBlock.FastFall(fallY, BlockFastFallCallback);
							// 掉落的方块需要在逻辑矩阵中修改位置
							SetMap(logicPos.x, logicPos.y, null);

						}

						fallY += 1;
					}
				}
			}

			if (m_FastFallBlocks.Count == 0)
			{
				SendFSMEvent(Events.FSMEvent.NoFallBlocks);
			}
		}

		/// <summary>
		/// 方块快速下落动画完成的回调
		/// </summary>
		/// <param name="block">执行动画的方块</param>
		private void BlockFastFallCallback(Block block)
		{
			Point finalPoint = GetLogicPosition(block.transform.localPosition);
			StopBlock(block, finalPoint);

			m_FastFallBlocks.Remove(block);
			if (m_FastFallBlocks.Count == 0)
			{
				SendFSMEvent(Events.FSMEvent.StopFallBlock);
			}
		}

		private void OnGameOver()
		{
			for (int j = 0; j < m_Map.Count; j++)
			{
				for (int i = 0; i < m_Map[j].Count; i++)
				{
					if (m_Map[j][i] != null)
					{
						m_Map[j][i].GameOverJump();
					}
				}
			}
		}
	}
}