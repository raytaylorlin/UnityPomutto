using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PathologicalGames;
using UnityEngine;

namespace Pomutto
{
	public struct Point
	{
		public int x;
		public int y;

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
	
	public class GameLogicController : MonoBehaviour
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

		private List<List<Block>> m_Map;
		private Queue<Block> m_CheckClearQueue = new Queue<Block>();
		private HashSet<Block> m_BlockHasChecked = new HashSet<Block>();
		private List<List<Block>> m_ClearSetList = new List<List<Block>>();
		private HashSet<Block> m_FastFallBlocks = new HashSet<Block>();
		private Vector3 m_InitGroupPosition;
		private bool m_IsWaitingClearBlocks = false;
		

		public List<List<Block>> Map
		{
			get { return m_Map; }
		}

		void Start()
		{
			DG.Tweening.DOTween.Init();
			
			InitBlocks();
			InitGroup();
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

			for (int j = 0; j < 1; j++)
			{
				for (int i = 0; i < LOGIC_WIDTH - 5; i++)
				{
					Block block = BlockPool.Instance.Spawn(BlockPrefab, MapTransform);
					SetMap(i, j, block);
				}
			}
		}

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

		private void InitGroup()
		{
			m_InitGroupPosition = CurrentGroup.transform.localPosition;
			Debug.Log(m_InitGroupPosition);
			CreateBlockGroup(NextGroup);
			SwitchBlockGroup();
			
			CurrentGroup.OnBlockGroupStop += CurrentGroupOnBlockGroupStop;
		}

		private void CreateBlockGroup(BlockGroup group)
		{
			group.UpBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform);
			group.UpBlock.LogicPosition = new Point(0, 1);
			group.DownBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform);
			group.DownBlock.LogicPosition = new Point(0, 0);
		}

		private void SwitchBlockGroup()
		{
			CurrentGroup.transform.localPosition = m_InitGroupPosition;
			CurrentGroup.UpBlock = NextGroup.UpBlock;
			CurrentGroup.UpBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.UpBlock.LogicPosition = new Point(0, 1);
			CurrentGroup.DownBlock = NextGroup.DownBlock;
			CurrentGroup.DownBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.DownBlock.LogicPosition = new Point(0, 0);
			CreateBlockGroup(NextGroup);
		}
		
		void Update()
		{
			if (!m_IsWaitingClearBlocks)
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
		
		private void CurrentGroupOnBlockGroupStop(Point collisionPoint)
		{
			StopBlock(CurrentGroup.UpBlock, new Point(collisionPoint.x, collisionPoint.y + 2));
			StopBlock(CurrentGroup.DownBlock, new Point(collisionPoint.x, collisionPoint.y + 1));

			CheckClearSquare();
			// 切换下一组方块
//			SwitchBlockGroup();
		}

		private void StopBlock(Block block, Point finalPoint)
		{
			block.transform.SetParent(MapTransform);
			SetMap(finalPoint.x, finalPoint.y, block);
			block.PlayAnimation(Block.EAnimationType.Fall);
			
			m_CheckClearQueue.Enqueue(block);
		}

		private void CheckClearSquare()
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
					Debug.Log("Find clear set");
					for (int i = 0; i < clearSet.Count; i++)
					{
						Debug.Log(clearSet[i].name);
					}
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
				//遍历每个待清除方块集合，并对所有方块清除
				for (int i = 0; i < m_ClearSetList.Count; i++)
				{
					var set = m_ClearSetList[i];
					int clearBlockCount = 0;

					for (int j = 0; j < set.Count; j++)
					{
						var block = set[j];
						var logicPos = block.LogicPosition;
						block.Fade(BlockFadeCallback);
						if (GetMap(logicPos) != null)
						{
//							this._gameField[logicPos.y][logicPos.x].clear();
							SetMap(logicPos, null);
						}

						//对消除的方块数计数
						clearBlockCount += 1;
					}
//					this._gameScore.addScore(100 * count);
				}

				/* 3.查找空隙并让悬空的方块掉落 */
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
								SetMap(logicPos.x, fallY, checkBlock);
								// 掉落的方块要添加到开放列表，等待下轮的检测
//								m_CheckClearQueue.Enqueue(checkBlock);
							}

							fallY += 1;
						}
					}
				}
			}
			
			if (m_FastFallBlocks.Count == 0)
			{
				m_IsWaitingClearBlocks = false;
				SwitchBlockGroup();
			}
			// TODO 检查游戏结束
			
		}

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

		private void BlockFastFallCallback(Block block)
		{
			m_FastFallBlocks.Remove(block);
			StopBlock(block, block.LogicPosition);
			if (m_FastFallBlocks.Count == 0)
			{
				if (m_CheckClearQueue.Count == 0)
				{
					m_IsWaitingClearBlocks = false;
					SwitchBlockGroup();
				}
				else
				{
					CheckClearSquare();
				}
			}
		}
		
		private void BlockFadeCallback(Block block)
		{
			block.Reset();
			BlockPool.Instance.Despawn(block.gameObject);
		}
	}
}