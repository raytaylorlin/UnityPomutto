using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto.Config
{
	public class ConfigManager : MonoBehaviour
	{
		public BlockConfig BlockConfig;

		private static ConfigManager instance;

		public static ConfigManager Instance
		{
			get { return instance; }
		}

		void Awake()
		{
			instance = this;
		}
	}
}