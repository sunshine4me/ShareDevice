using System;

namespace MiniTouch {
    public class Banner {


        /// <summary>
        /// 版本信息
        /// </summary>
        public int Version {
            get; set;
        }

        /// <summary>
        /// 设备支持的最大触摸点数
        /// </summary>
        public int MaxContacts {
            get; set;
        }

        /// <summary>
        /// 设备的最大宽度
        /// </summary>
        public int MaxX {
            get; set;
        }

        /// <summary>
        /// 设备的最大高度
        /// </summary>
        public int MaxY {
            get; set;
        }

        /// <summary>
        /// 设备的最大压力值
        /// </summary>
        public int MaxPressure {
            get; set;
        }

        /// <summary>
        /// 进程ID
        /// </summary>
        public int Pid {
            get; set;
        }


        /// <summary>
        ///真实设备宽度和minitouch显示的宽度的百分比
        /// </summary>
        public double PercentX {
            get; set;
        }

        /// <summary>
        /// 真实设备高度和minitouch显示的高度的百分比
        /// </summary>
        public double PercentY {
            get; set;
        }


        public override String ToString() {
            return "Banner [Version=" + Version + ", Pid="
                + Pid + ", MaxContacts=" + MaxContacts + ", Maxx="
                + MaxX + ", MaxY=" + MaxY + ", MaxPressure=" + MaxPressure + "]";
        }
    }
}
