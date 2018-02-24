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
		private Vector3 m_InitGroupPosition;
		

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
			block.LogicPosition = new Point(x, y);
			m_Map[y][x] = block;
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
			InputManager.Instance.Tick();
			CurrentGroup.Tick();

			if (Input.GetKeyDown(KeyCode.B))
			{
				SwitchBlockGroup();
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
			
			// 切换下一组方块
			SwitchBlockGroup();
		}

		private void StopBlock(Block block, Point finalPoint)
		{
			block.transform.SetParent(MapTransform);
			SetMap(finalPoint.x, finalPoint.y, block);
			block.PlayAnimation(Block.EAnimationType.Fall);
			
			m_CheckClearQueue.Enqueue(block);
		}
	}
}