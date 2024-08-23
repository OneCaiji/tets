using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{ 
	/// <summary>
	/// 冷凝器
	/// </summary>
	public class Condenser:Component 
	{

		public PortofGas Inport, Outport, BleedIn, realInport;
		/// <summary>
		/// 冷凝器压比
		/// </summary>
		public double DP, Wades, DPR, DesignDPR;
		public double PreDP, PreDPRatio, PreDesignDPRatio;
		public double T2;

		public double CoolerDH=0;

		public Condenser(Engine eng)
		{
			Inport = new PortofGas();
			realInport = new PortofGas();
			Outport = new PortofGas();
			BleedIn = new PortofGas();
			T2 = 500;
			DP = 0;
			this.Engine = eng;
			eng.ComponentList.Add(this); 
		}
		public void Set( double DP,double T2)
		{

		this.DP = DP;
		this.T2 = T2;

		}

		public override void Run(int N)
		{
			double Tin = Inport.GasTt;
			double Pin = Inport.GasPt;

			if (this.Engine.boolDesign == true)
			{
				this.Wades = Inport.GasFlow ;
				this.PreDesignDPRatio = this.PreDP / Inport.GasPt ;
			}
			this.PreDPRatio = this.PreDesignDPRatio * (Inport.GasFlow / Wades);
			Pin = Pin * (1 - this.PreDPRatio);


			double Tbleedin=BleedIn .GasTt;
			double Pbleedin = BleedIn.GasPt;
			double Win = Inport.GasFlow;
			double Wbleedin = BleedIn.GasFlow;
			double Wout = Win + Wbleedin;

			double Tout = this.T2;
			if (this.Engine.boolDesign == true)
			{
				this.Wades = Win;
				this.DesignDPR = this.DP / Pin;
			}
			this.DPR = this.DesignDPR * (Win / Wades);
			double Pout = Pin * (1 - this.DPR);


			sco2Calculate aa = new sco2Calculate();


			double h1 = aa.ReturnH(Tin, Pin);
			double s1 = aa.ReturnS(Tin, Pin);
			double d1 = aa.ReturnD(Tin, Pin);

			double hbleedin = aa.ReturnH(Tin, Pin);
			double sbleedin = aa.ReturnS(Tin, Pin);
			double dbleedin = aa.ReturnD(Tin, Pin);

			double h2 = aa.ReturnH(Tout, Pout);
			double s2 = aa.ReturnS(Tout, Pout);
			double d2 = aa.ReturnD(Tout, Pout);

			BleedIn.WritePort(Tbleedin, Pbleedin, hbleedin, sbleedin, Wbleedin, dbleedin);
			realInport.WritePort(Tin, Pin, h1, s1, Win, d1);
			Outport.WritePort(Tout, Pout, h2, s2, Wout, d2);
		}

	}
}
