using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	public class CPTurbine:CRotator
	{
		/// <summary>
		/// 涡轮换算转速设计点值
		/// </summary>
		public double CNTds;
		/// <summary>
		/// 缩放因子
		/// </summary>
		public double CNTscaller = 1.0,PRscaller = 1.0,Effscaller = 1.0,WGcscaller = 1.0;
		/// <summary>
		/// 涡轮发出功率（W）
		/// </summary>
		public double WorkProvide;
		/// <summary>
		/// 特性图辅助 工作点位置
		/// </summary>
		public double MapR,MapRds;
		/// <summary>
		/// 设计点压比、效率、流量
		/// </summary>
		public double PRds, Effds, WGcmapds;
		/// <summary>
		/// 压比、效率、流量
		/// </summary>
		public double PRmap, Effmap, WGcmap;
		public double Wades;
		public double PreDP, PreDPRatio, PreDesignDPRatio;

		public PortofGas BleedIn;
		public double Tin, Pin, P5;
		public double WorkOut = 0;
		public double EffScalerModifier = 0;
		public CPTurbine(Engine eng)
		{
			InPort = new PortofGas();
			OutPort =new PortofGas();
			BleedIn = new PortofGas();
			MechPort = new PortofMech();
			Tin = 900;
			Pin = 20000;
			P5 = 2000;
			this.Engine = eng;
			eng.ComponentList.Add(this); 
		}

		public override void Run(int N)
		{
			double t1 = 0, p1 = 0, h1 = 0, s1 = 0, d1 = 0, w1 = 0;//入口
			double t2 = 0, p2 = 0, h2 = 0, s2 = 0, d2 = 0, w2 = 0;//出口
			double t3 = 0, p3 = 0, h3 = 0, s3 = 0, d3 = 0, w3 = 0;//冷气

			InPort.ReadPort(ref t1, ref  p1, ref h1, ref s1, ref w1, ref d1);
			BleedIn.ReadPort(ref t3, ref  p3, ref h3, ref s3, ref w3, ref d3);

			if (this.Engine.boolDesign)
			{
				this.CompMap = new CCompMap(MapPath);
				this.CompMap.Load();
				this.CompMap.CreatPRofTurbin(); //格式转换
			}
			MapCal(p1, this.Tin, w1);
			double PR = p1 / this.P5;
			ThTurbin(PR, this.Effmap, this.Tin, p1, ref t2);
			sco2Calculate aa = new sco2Calculate();

			h1 = aa.ReturnH(Tin, p1);
			h2 = aa.ReturnH(t2, this.P5);
			

			h2 = (h2 * w1 + h3 * w3) / (w1 + w3);
			t2 = aa.TFromH(this.P5, h2);
			s2 = aa.ReturnS(t2, this.P5);
			d2 = aa.ReturnD(t2, this.P5);
			w2 = w1 + w3;
			this.WorkOut = (h1 - h2) * w2;

			double P5_cal = p1 / this.PRmap;
			double Error1 = (P5_cal - this.P5) / P5_cal;
			Engine.ErrorList.Add(Error1);
			OutPort.WritePort(t2, this.P5, h2, s2, w2, d2);

		}
		void ThTurbin(double PR, double Eta, double Ti, double Pi, ref double To)
		{
			sco2Calculate aa = new sco2Calculate();
			double Po = Pi / PR;
			double xx = 0;
			aa.TPflash(Ti, Pi);
			double Hi = aa.enthalpy;
			double Si = aa.entropy;
			double Rhoi = aa.density;
			double To_Ideal = aa.TFromS(Po, Si);
			aa.TPflash(To_Ideal, Po);
			double Ho_Ideal = aa.enthalpy;
			double Ho = Hi + (Ho_Ideal - Hi) *Eta;
			To = aa.TFromH(Po, Ho);
			xx = aa.TFromH(Po, Ho_Ideal);
			
		}


		private void MapCal(double Pi, double Ti, double WGi)
		{
			if (this.Engine.boolDesign)
			{
				CNTscaller = CNTds * Math.Sqrt(Ti) / this.PNC;
				PRmap = PRds;
			}
			this.CNC = CNTscaller * this.PNC / Math.Sqrt(Ti);
			this.PRmap = this.CompMap.Find("Pressure", this.CNC, this.MapR);
			this.WGcmap = this.CompMap.Find("Mass", this.CNC, this.MapR);
			this.Effmap = this.CompMap.Find("Efficiency", this.CNC, this.MapR);

			if (this.Effmap > 0.9)//特性图中不存在大于0.88的点
			{
				this.Effmap = 0.9;
			}

			double WGccal = WGi * Math.Sqrt(Ti) / Pi * 1000;	//计算实际涡轮换算流量
			if (this.Engine.boolDesign)
			{
				this.Effscaller = this.Effds / this.Effmap;
				this.WGcscaller = WGccal / this.WGcmap;
				this.PRscaller = this.PRds / this.PRmap; //设计点处理，需要计算得出发动机的压比
			}
			if (this.Engine.boolDesign)
			{
				this.EffScalerModifier = 0;
			}
			this.Effmap = this.Effscaller * this.Effmap*(1+this.EffScalerModifier );	//查表得出的效率乘以比例因子
			this.WGcmap = this.WGcscaller * this.WGcmap;	//计算流量乘以比例因子
			this.PRmap  = this.PRscaller  * this.PRmap;

			if (this.Effmap > 0.9)
			{
				this.Effmap = 0.9;
			}
			double Error = (WGccal - this.WGcmap) / WGccal;
			Engine.ErrorList.Add(Error);
		}

	}
}
