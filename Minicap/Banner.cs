using System;

namespace Minicap {
    public class Banner {

        /// <summary>
        /// 版本信息
        /// </summary>
        public int Version {
            get;
            set;
        }

        /// <summary>
        /// banner长度
        /// </summary>
        public int Length {
            get;
            set;
        }

        /// <summary>
        /// 进程ID
        /// </summary>
        public int Pid {
            get;
            set;
        }


        /// <summary>
        /// 设备的真实宽度
        /// </summary>
        public int RealWidth {
            get;
            set;
        }

        /// <summary>
        /// 设备的真实高度
        /// </summary>
        public int RealHeight {
            get;
            set;
        }

        /// <summary>
        /// 设备的虚拟宽度
        /// </summary>
        public int VirtualWidth {
            get;
            set;
        }

        /// <summary>
        /// 设备的虚拟高度
        /// </summary>
        public int VirtualHeight {
            get;
            set;
        }

        /// <summary>
        /// 设备方向
        /// </summary>
        public int Orientation {
            get;
            set;
        }

        /// <summary>
        /// 设备信息获取策略
        /// </summary>
        public int Quirks {
            get;
            set;
        }

        public override String ToString() {
            return "Banner [version=" + Version + ", length=" + Length + ", pid="
                + Pid + ", readWidth=" + RealWidth + ", readHeight="
                + RealHeight + ", virtualWidth=" + VirtualWidth + ", virtualHeight=" + VirtualHeight + ", orientation="
                + Orientation + ", quirks=" + Quirks + "]";
        }
    }
}
