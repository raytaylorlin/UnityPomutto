using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto
{
	public class GameLogicConfig : MonoBehaviour
	{
		[Header("每个方块消除时获得的能量")]
		public uint EnergyGainPerBlock;

		[Header("方块消除Combo时获得的能量（索引0为2连击）")]
		public List<uint> EnergyComboGainPerBlock;

		[Header("每个方块消除时获得的灵玉数")]
		public float JadeGainPerBlock;

		[Header("方块消除Combo时获得的灵玉数（索引0为2连击）")]
		public List<float> JadComboGainPerBlock;

		[Header("每个灵玉产生的障碍方块数")]
		public uint ObstacleBlockPerJade;

		[Header("每个大灵玉产生的障碍方块数")]
		public uint ObstacleBlockPerLargeJade;

		[Header("每个大灵玉使用时获得的能量")]
		public uint EnergyGainPerLargeJade;
	}
}