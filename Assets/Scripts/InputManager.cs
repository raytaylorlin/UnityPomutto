using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto
{
	public class InputManager
	{
		private static InputManager m_Instance;

		public static InputManager Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new InputManager();
				}
				return m_Instance;
			}
		}
		
		private InputManager()
		{
		}

		public bool LeftPress { get; set; }
		public bool RightPress { get; set; }
		public bool DownPress { get; set; }

		public void Tick()
		{
			LeftPress = 
				Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
			RightPress = 
				Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
			DownPress = 
				Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
		}
	}
}