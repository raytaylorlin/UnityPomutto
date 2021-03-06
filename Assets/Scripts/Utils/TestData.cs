﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pomutto
{
    public class TestData
    {
        public static List<List<int>> GenerateMapData()
        {
            int logicHeight = GameLogicController.LOGIC_HEIGHT;
            int logicWidth = GameLogicController.LOGIC_WIDTH;
            List<List<int>> map = new List<List<int>>();

            for (int j = 0; j < 10; j++)
            {
                List<int> row = new List<int>();
                for (int i = 0; i < logicWidth; i++)
                {
                    row.Add(1);
                }
                
//                for (int i = 3; i < 4; i++)
//                {
//                    row.Add(-1);
//                }
//                
//                for (int i = 4; i < logicWidth; i++)
//                {
//                    row.Add(1);
//                }
                map.Add(row);
            }

            return map;
        }
    }
}
