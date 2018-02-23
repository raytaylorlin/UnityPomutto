using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;

namespace Pomutto
{
	public class BlockPool
	{
		private GameObject m_BlockPrefab;
		private SpawnPool m_BlockPool;
		private const string BLOCK_POOL_NAME = "Block";
		
		private static BlockPool m_Instance;
		public static BlockPool Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new BlockPool();
				}

				return m_Instance;
			}
		}

		private BlockPool()
		{
			m_BlockPrefab = Resources.Load("Prefabs/Block") as GameObject;
			m_BlockPool = PoolManager.Pools[BLOCK_POOL_NAME];
		}

		public Block Spawn()
		{
			Transform transform = m_BlockPool.Spawn(m_BlockPrefab.transform);
			Block block = transform.GetComponent<Block>();
			block.SetType((Block.EType) Random.Range(0, (int) Block.EType.Max));
			return block;
		}
	}
}