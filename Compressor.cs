using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	/// <summary>
	/// 压气机
	/// </summary>
	public class Compressor:CRotator 
	{
		/// <summary>
		/// 缩放因子
		/// </summary>
		public double PRscaller,Effscaller,WAscaller;
		
		/// <summary>
		/// //喘振裕度
		/// </summary>
		public double SurgeMargin;

		/// <summary>
		/// work of compressor (W)
		/// </summary>
		public double Work;
		/// <summary>
		/// 效率
		/// </summary>
		public double Eff, Effds;
		/// <summary>
		/// 压比
		/// </summary>
		public double PR, PRds;
		/// <summary>
		/// 换算流量
		/// </summary>
		public double WAc,WAcds;
		/// <summary>
		/// 物理流量
		/// </summary>
		public double Wa,Wads;     
		/// <summary>
		/// /辅助线
		/// </summary>
		public double MapR,MapRds;

		public PortofGas BleedOut,BleedToCon;
		public double WBleedOut;
		public double WToCon=0;
		public double Tin, Pin;

		public double EffScalerModifier = 0;


		public Compressor(Engine eng)
		{
			InPort = new PortofGas();
			OutPort = new PortofGas();
			BleedOut = new PortofGas();
			BleedToCon = new PortofGas();
			MechPort = new PortofMech();
			MapR = 0.85;
			PRscaller = 1.0;
			Effscaller = 1.0;
			WAscaller = 3.0;
			Tin = 288.15;
			Pin = 7000;
			Wa = 0;
			WBleedOut = 0;
			this.PR = 1;
			this.Engine = eng;
			eng.ComponentList.Add(this);  
 
		}

		public override void Run(int N)
		{
			if (this.Engine.boolDesign)
			{
				this.CompMap = new CCompMap(this.MapPath);
				this.CompMap.Load();
			}

			double T3=this.Tin ;
			if (this.Engine.boolDesign)
			{
				this.MechN = this.MechNds;
				this.PNC = MechNds; 
				this.MapR = this.MapRds;
			}
			else
			{
				this.PNC = this.MechN;
			}
			MapCal();
			double POut = this.Pin * this.PR;
			ThComp(PR ,this.Eff ,this.Tin ,this.Pin,ref T3  );
			#region 计算详细截面参数
			sco2Calculate aa3 = new sco2Calculate();
			aa3.TPflash(T3, POut);
			double H3 = aa3.enthalpy;
			double S3 = aa3.entropy;
			double Rho3 = aa3.density;
			sco2Calculate aa2 = new sco2Calculate();
			aa2.TPflash(Tin, Pin);
			double H2 = aa2.enthalpy;
			double S2 = aa2.entropy;
			double Rho2 = aa2.density;
			#endregion			
			InPort.WritePort(Tin, Pin, H2, S2, this.Wa, Rho2);
			OutPort.WritePort(T3, POut, H3, S3, this.Wa * (1 - this.WBleedOut- this.WToCon), Rho3);
			BleedOut.WritePort(T3, POut, H3, S3, this.Wa * this.WBleedOut, Rho3);
			BleedToCon.WritePort(T3, POut, H3, S3, this.Wa * this.WToCon, Rho3);
			this.Work = (H3 - H2) * this.Wa;
			MechPort.WritePort(this.Work, this.PNC, this.MI);
		}
		void MapCal()
		{
			double Pstd = 8130;
			double Tstd = 310;
			double Theta = Math.Sqrt(this.Tin / Tstd);
			double Delta = this.Pin / Pstd;
			this.WAcds = this.Wads * Theta / Delta;
			this.CNC = this.PNC * Theta/100;

			this.Eff = this.CompMap.Find("Efficiency", this.CNC, this.MapR);
			this.WAc = this.CompMap.Find("Mass", this.CNC, this.MapR);//查出来的物理流量
			this.PR = this.CompMap.Find("Pressure", this.CNC, this.MapR);


			if (this.Engine.boolDesign)
			{
				this.Effscaller = this.Effds / this.Eff;
				this.PRscaller = (this.PRds - 1.0) / (this.PR - 1.0);			
				this.WAscaller = this.WAcds / this.WAc;
			}
			if (this.Engine.boolDesign)
			{
				this.EffScalerModifier = 0;
			}
			this.PR = this.PRscaller * (this.PR - 1.0) + 1.0;
			this.Eff = this.Effscaller * this.Eff * (1 + this.EffScalerModifier);
			//this.Eff = 0.65;
			this.WAc = this.WAscaller * this.WAc;
			Wa = this.WAc * Delta / Theta;
		}
		private void ThComp(double PR, double Eta, double Ti, double Pi, ref double To)
		{
			sco2Calculate aa = new sco2Calculate();
			double Po = Pi * PR;
			aa.TPflash(Ti, Pi);
			double Hi = aa.enthalpy;
			double Si = aa.entropy;
			double Rhoi = aa.density;
			double To_Ideal = aa.TFromS(Po, Si);
			aa.TPflash(To_Ideal, Po);
			double Ho_Ideal = aa.enthalpy;
			double Ho = Hi + (Ho_Ideal - Hi) / Eta;
			To = aa.TFromH(Po, Ho);

		}
	
	}


}
