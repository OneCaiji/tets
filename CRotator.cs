using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	abstract public class CRotator:Component
	{
		/// <summary>
		/// /轴线输出端口
		/// </summary>
		public PortofGas OutPort;
		/// <summary>
		/// 进气端口
		/// </summary>
		public PortofGas InPort;
		/// <summary>
		/// 引起端口
		/// </summary>
		public PortofGas BleedPort;
		/// <summary>
		/// //机械端口
		/// </summary>
		public PortofMech MechPort;
		/// <summary>
		/// 部件所用特性图
		/// </summary>
		public CCompMap CompMap;
		/// <summary>
		/// 特性图名称和地址
		/// </summary>
		public string MapPath;
		/// <summary>
		/// //叶片级数
		/// </summary>
		public int Statges;
		/// <summary>
		/// //惯性矩
		/// </summary>
		public double MI;
		/// <summary>
		/// //部件转速
		/// </summary>        
		public double MechN;
		public double MechNds;
		/// <summary>
		///  相对物理转速
		/// </summary>
		public double PNC;
		/// <summary>
		/// 相对换算转速
		/// </summary>
		public double CNC;
		public abstract override void Run(int N);

	}
}
