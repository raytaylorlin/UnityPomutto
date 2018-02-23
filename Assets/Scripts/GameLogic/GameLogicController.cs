using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;

namespace Pomutto
{
	public class GameLogicController : MonoBehaviour
	{
		public GameObject BlockPrefab;
		public Transform MapTransform;
		public Transform GroupContainer;
		
		private List<List<Block>> m_Map;
		private const int LOGIC_WIDTH = 7;
		private const int LOGIC_REAL_HEIGHT = 12;
		private const int LOGIC_EXTEND_HEIGHT = 4;
		private const int LOGIC_HEIGHT = LOGIC_REAL_HEIGHT + LOGIC_EXTEND_HEIGHT;

		public BlockGroup CurrentGroup;
		public BlockGroup NextGroup;
		private Vector3 m_InitGroupPosition;
		

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
				for (int j = 0; j < LOGIC_WIDTH; j++)
				{
					Block block = BlockPool.Instance.Spawn(BlockPrefab, MapTransform);
					block.LogicPosition = new Vector2(i, j);
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
			group.UpBlock.LogicPosition = new Vector2(1, 0);
			group.DownBlock = BlockPool.Instance.Spawn(BlockPrefab, group.transform);
			group.DownBlock.LogicPosition = new Vector2(0, 0);
		}

		private void SwitchBlockGroup()
		{
			CurrentGroup.transform.localPosition = m_InitGroupPosition;
			CurrentGroup.UpBlock = NextGroup.UpBlock;
			CurrentGroup.UpBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.UpBlock.LogicPosition = new Vector2(1, 0);
			CurrentGroup.DownBlock = NextGroup.DownBlock;
			CurrentGroup.DownBlock.transform.SetParent(CurrentGroup.transform);
			CurrentGroup.DownBlock.LogicPosition = new Vector2(0, 0);
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
	}
}