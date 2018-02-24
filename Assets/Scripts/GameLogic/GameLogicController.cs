using System.Collections;
using System.Collections.Generic;
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
	}
	
	public class GameLogicController : MonoBehaviour
	{
		public GameObject BlockPrefab;
		public Transform MapTransform;
		public Transform GroupContainer;
		
		private List<List<Block>> m_Map;
		public const int LOGIC_WIDTH = 7;
		public const int LOGIC_REAL_HEIGHT = 12;
		public const int LOGIC_EXTEND_HEIGHT = 4;
		public const int LOGIC_HEIGHT = LOGIC_REAL_HEIGHT + LOGIC_EXTEND_HEIGHT;
		public const int MAP_WIDTH = Block.BLOCK_SIZE * (LOGIC_WIDTH - 1);

		public BlockGroup CurrentGroup;
		public BlockGroup NextGroup;
		private Vector3 m_InitGroupPosition;

		public List<List<Block>> Map
		{
			get { return m_Map; }
		}

		void Start()
		{
			InitBlocks();
			InitGroup();
		}

		private void InitBlocks()
		{
			m_Map = new List<List<Block>>(LOGIC_HEIGHT);
			for (int i = 0; i < LOGIC_HEIGHT; i++)
			{
				List<Block> row = new List<Block>(LOGIC_WIDTH);
				for (int j = 0; j < LOGIC_WIDTH; j++)
				{
					row.Add(null);
				}
				m_Map.Add(row);
			}

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < LOGIC_WIDTH - 3; j++)
				{
					Block block = BlockPool.Instance.Spawn(BlockPrefab, MapTransform);
					block.LogicPosition = new Point(i, j);
					m_Map[i][j] = block;
				}
			}
		}

		private void InitGroup()
		{
			m_InitGroupPosition = CurrentGroup.transform.localPosition;
			Debug.Log(m_InitGroupPosition);
			CreateBlockGroup(NextGroup);
			SwitchBlockGroup();
		}

		private void CreateBlockGroup(BlockGroup group)
		{
			group.UpBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform);
			group.UpBlock.LogicPosition = new Point(1, 0);
			group.DownBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform);
			group.DownBlock.LogicPosition = new Point(0, 0);
		}

		private void SwitchBlockGroup()
		{
			CurrentGroup.transform.localPosition = m_InitGroupPosition;
			CurrentGroup.UpBlock = NextGroup.UpBlock;
			CurrentGroup.UpBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.UpBlock.LogicPosition = new Point(1, 0);
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
			return new Point((int) (realPos.y / Block.BLOCK_SIZE),
				(int) (realPos.x / Block.BLOCK_SIZE));
		}

		public Block GetBlock(Vector2 realPos)
		{
			Point logicPos = GetLogicPosition(realPos);
			if (logicPos.x >= 0 && logicPos.x < LOGIC_HEIGHT &&
			    logicPos.y >= 0 && logicPos.y < LOGIC_WIDTH)
			{
				return m_Map[logicPos.x][logicPos.y];
			}
			return null;
		}
	}
}