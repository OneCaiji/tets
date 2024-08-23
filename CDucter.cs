using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	/// <summary>
	/// 管路
	/// </summary>
	public class CDucter:Component 
	{
		public PortofGas Inport, Outport;
		public double DP, Wades, DPR,DesignDPR;

		public CDucter(Engine eng)
		{
			Inport = new PortofGas();
			Outport = new PortofGas();
			DP = 0;
			DPR = 0;
			DesignDPR = 0;
			Wades = 10;
			this.Engine = eng;
			eng.ComponentList.Add(this); 
		}

		public void Set(double dp)
		{
			this.DP = dp;
		}
		public override void Run(int N)
		{
			double t = 0, p = 0, h = 0, s = 0, w = 0, d = 0;
			Inport.ReadPort(ref t, ref p, ref h, ref s, ref w, ref d);
			if (this.Engine.boolDesign == true)
			{
				this.Wades = w;
				this.DesignDPR = this.DP / p;
			}
			this.DPR =this.DesignDPR *(w / Wades );
			p = p * (1 - this.DPR);
			Outport.WritePort(t, p, h, s, w, d);
		}
	}
}
