using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	/// <summary>
	/// 热源
	/// </summary>
	public class Source:Component 
	{
		public PortofGas Inport, CTOutport, PTOutport;
		public double WaPtR;
		public double Tout;
		public double DP, Wades, DPR,DesignDPR;
		public double PreDP, PreDPRatio, PreDesignDPRatio;
		public double SourceDH = 0;

		public Source(Engine eng)
		{
			Inport = new PortofGas();
			CTOutport = new PortofGas();
			PTOutport = new PortofGas();
			Tout = 900;
			WaPtR = 0;
			DP = 0;
			this.Engine = eng;
			eng.ComponentList.Add(this); 
		}
		public override void Run(int N)
		{
			sco2Calculate aa = new sco2Calculate();
			double Pin = Inport.GasPt;
			if (this.Engine.boolDesign == true)
			{
				this.Wades = Inport.GasFlow;
				this.PreDesignDPRatio = this.PreDP / Inport.GasPt;
			}
			this.PreDPRatio = this.PreDesignDPRatio * (Inport.GasFlow / Wades);
			Pin = Pin * (1 - this.PreDPRatio);

			double Tin = Inport.GasTt;
			double Pout = Pin;
			double w = Inport.GasFlow;
			double h1 = aa.ReturnH(Tin, Pin);
			double s1 = aa.ReturnS(Tin, Pin);
			double d1 = aa.ReturnD(Tin, Pin);

			if (this.Engine.boolDesign == true)
			{
				this.Wades = w;
				this.DesignDPR = this.DP / Pin;
			}
			this.DPR = this.DesignDPR * (w / Wades);
			Pout = Pout * (1 - this.DPR);

			double h2 = aa.ReturnH(this.Tout, Pout);
			double s2 = aa.ReturnS(this.Tout, Pout);
			double d2 = aa.ReturnD(this.Tout, Pout);
			Inport.WritePort(Tin, Pin, h1, s1, w, d1);
			CTOutport.WritePort(this.Tout, Pout, h2, s2, w, d2);
			this.SourceDH = (h2 - h1) * w;
		}
		


	}
}
