using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pomutto.Config
{
	public class BlockConfig : MonoBehaviour
	{
		[Header("方块组参数")]
		public int BlockGroupDownSpeed;
		public float BlockGroupTweenDuration;
		
		[Header("游戏结束方块弹跳下落的参数")]
		public int XRandomMin;
		public int XRandomMax;
		public int YRandomMin;
		public int YRandomMax;
		public int JumpRandomMin;
		public int JumpRandomMax;
		public float TimeRandomMin;
		public float TimeRandomMax;
		public Ease JumpEase;
	}
}