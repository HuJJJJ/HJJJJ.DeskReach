﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HJJJJ.OpenDesk.Plugins.DrawingBoard.Windows.Entities
{

    [Serializable]
    public class LineSegment
    {
        public LineSegment()
        {
            Tracks = new List<Point>();
        }

        /// <summary>
        /// 线段的运动轨迹
        /// </summary>
        public List<Point> Tracks { get; set; }

        /// <summary>
        /// 线段的颜色
        /// </summary>
        public Color Color { get; set; } = Color.Red;

        /// <summary>
        /// 线段的宽度
        /// </summary>
        public int Width { get; set; } = 2;
    }
}
