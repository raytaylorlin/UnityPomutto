using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;

namespace Pomutto
{
	public class GameLogicController : MonoBehaviour
	{
		private List<List<Block>> m_Map;
		private const int LOGIC_WIDTH = 7;
		private const int LOGIC_REAL_HEIGHT = 12;
		private const int LOGIC_EXTEND_HEIGHT = 4;
		private const int LOGIC_HEIGHT = LOGIC_REAL_HEIGHT + LOGIC_EXTEND_HEIGHT;


		

		void Start()
		{
			InitBlocks();


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

			for (int i = 0; i < LOGIC_WIDTH; i++)
			{
				m_Map[0][i] = BlockPool.Instance.Spawn();
			}
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}