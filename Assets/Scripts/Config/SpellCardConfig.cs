using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto
{
	public class SpellCardConfig : MonoBehaviour
	{
		[Header("公共卡")]

		[Header("【时间减速】减速百分比")]
		public float SlowDownPercent;

		[Header("【时间减速】减速时间")]
		public float SlowDownDuration;

		[Header("【迷雾】扰乱视觉时间")]
		public List<uint> FrogDuration;

		[Header("【潜行】扰乱NEXT区域和卡槽时间")]
		public float SneakDuration;

		[Header("【治愈】清除障碍方块数")]
		public uint ClearObstacleBlockCount;

		[Header("【回复生命】恢复量")]
		public uint RestoreHp;

		[Header("【愤怒】加速百分比")]
		public float SpeedUpPercent;

		[Header("【愤怒】加速时间")]
		public float SpeedUpDuration;

		[Header("【魔法增幅】符卡提升速度百分比")]
		public float SpellCardSpeedUpPercent;

		[Header("【魔法增幅】符卡累积时间")]
		public float SpellCardSpeedUpDuration;

		[Header("【攻击之剑】上升的行数")]
		public uint RiseLine;
	}
}