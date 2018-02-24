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
		private bool m_HasInit = false;
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
//			m_BlockPrefab = Resources.Load("Prefabs/Block") as GameObject;
			m_BlockPool = PoolManager.Pools[BLOCK_POOL_NAME];
		}

		public Block Spawn(GameObject prefab, Transform parent)
		{
//			int type = Random.Range(0, (int) Block.EType.Max);
			int type = 3;
			
			return Spawn(prefab, parent, type);
		}
		
		public Block Spawn(GameObject prefab, Transform parent, int type)
		{
			Transform transform = m_BlockPool.Spawn(prefab.transform, parent);
			Block block = transform.GetComponent<Block>();
			block.SetType((Block.EType) type);
			return block;
		}

		public void Despawn(GameObject go)
		{
			m_BlockPool.Despawn(go.transform);
		}
	}
}