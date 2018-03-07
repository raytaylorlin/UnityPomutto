using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto
{
	public class WeatherConfig : MonoBehaviour
	{
		[Header("【冰雹】符卡提升速度百分比")]
		public uint Hail_SpellCardSpeedUpPercent;

		[Header("【台风】加速百分比")]
		public float Typhoon_SpeedUpDuration;

//		[Header("【晴岚】减速时间")]
//		public float SlowDownDuration;
//
//		[Header("【迷雾】扰乱视觉时间")]
//		public List<uint> FrogDuration;
	}
}