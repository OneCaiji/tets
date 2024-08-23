using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	public class PortofGas:Port 
	{

		#region 元素定义
		/// <summary>
		/// // 气流马赫数
		/// </summary>
		public double GasMach;
		/// <summary>
		/// // 质量流率
		/// </summary>
		public double GasFlow;
		/// <summary>
		/// // 流路面积
		/// </summary>
		public double GasArea;
		/// <summary>
		/// // 气体静压
		/// </summary>
		public double GasPs;
		/// <summary>
		/// // 气体总压
		/// </summary>
		public double GasPt;
		/// <summary>
		/// // 气体静温
		/// </summary>
		public double GasTs;
		/// <summary>
		/// // 气体总温
		/// </summary>
		public double GasTt;
		/// <summary>
		/// // 气体密度
		/// </summary>
		public double GasDens;
		/// <summary>
		/// // 气体焓
		/// </summary>
		public double GasHi;
		/// <summary>
		/// // 气体熵
		/// </summary>
		public double GasSi;
		/// <summary>
		/// // 气体静焓
		/// </summary>
		public double GasHs;
		/// <summary>
		/// // 气体静熵
		/// </summary>
		public double GasSs;
		/// <summary>
		/// // 气体定容比热
		/// </summary>
		public double GasCp;
		/// <summary>
		/// // 当地音速
		/// </summary>
		public double LocalSonic;
		#endregion

		public PortofGas()
		{

			GasMach = 0;	// 气流马赫数
			GasFlow = 0;	// 质量流率
			GasArea = 0;	// 流路面积
			GasPs = 0;	// 气体静压
			GasPt = 0;	// 气体总压
			GasTs = 0;	// 气体静温
			GasTt = 0;	// 气体总温
			GasDens = 0;	// 气体密度
			GasHi = 0;	// 气体焓
			GasSi = 0;	// 气体熵
			GasHs = 0;	// 气体静焓	
			GasSs = 0;	// 气体静熵
			GasCp = 0;	// 气体定容比热
			LocalSonic = 0;// 当地音速

		}

		/// <summary>
		/// 设置端口数据
		/// </summary>
		/// <param name="T"></param>
		/// <param name="P"></param>
		/// <param name="H"></param>
		/// <param name="W"></param>
		/// <param name="Rho"></param>
		public void WritePort(double T, double P, double H,double S, double W,double Rho)
		{
			this.GasPt = P;
			this.GasTt = T;
			this.GasHi = H;
			this.GasSi = S;
			this.GasFlow = W;
			this.GasDens = Rho;
		}

		/// <summary>
		/// 从端口读数据
		/// </summary>
		/// <param name="T"></param>
		/// <param name="P"></param>
		/// <param name="H"></param>
		/// <param name="S"></param>
		/// <param name="W"></param>
		public void ReadPort(ref double T, ref double P, ref double H,ref double S, ref double W,ref double Rho)
		{
			P = this.GasPt;
			T = this.GasTt;
			H = this.GasHi;
			S = this.GasSi;
			W = this.GasFlow;
			Rho = this.GasDens;

		}
	}
}
