using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	/// <summary>
	/// 热交换器
	/// </summary>
	public class CExchange:Component 
	{
		/// <summary>
		/// 低温端入口
		/// </summary>
		public PortofGas LowInport, realLowInport;
		/// <summary>
		/// 低温端出口
		/// </summary>
		public PortofGas LowOutport;
		/// <summary>
		/// 高温端入口
		/// </summary>
		public PortofGas HighInport0, HighInport1, HighInport2;
		/// <summary>
		/// 高温端出口
		/// </summary>
		public PortofGas HighOutport;
		public double DP, DPRatio,DesignDPRatio;
		public double DP2, DPRatio2, DesignDPRatio2;
		public double PreDP, PreDPRatio, PreDesignDPRatio;
		public double PreDP2, PreDPRatio2, PreDesignDPRatio2;
		public double Wades;
		/// <summary>
		/// 回热度
		/// </summary>
		public double Eta;

		public double ThermalRatio = 1;

		public double HighExDH = 0;
		public double LowExDH = 0;

		public CExchange(Engine eng)
		{
			LowInport = new PortofGas();
			realLowInport = new PortofGas();
			LowOutport = new PortofGas();
			HighInport1 = new PortofGas();
			HighInport0 = new PortofGas();
			HighOutport = new PortofGas();
			this.DP = 0;
			this.Engine = eng;
			eng.ComponentList.Add(this); 

		}


		public override void Run(int N)
		{
			sco2Calculate aa = new sco2Calculate();
			double t1 = 0, p1 = 0, h1 = 0, s1 = 0, d1 = 0, w1 = 0;//低温端入口
			double t2 = 0, p2 = 0, h2 = 0, s2 = 0, d2 = 0, w2 = 0;//低温端出口
			double t3 = 0, p3 = 0, h3 = 0, s3 = 0, d3 = 0, w3 = 0;//高温端入口
			double t4 = 0, p4 = 0, h4 = 0, s4 = 0, d4 = 0, w4 = 0;//高温段出口


			LowInport.ReadPort(ref t1, ref  p1, ref h1, ref s1, ref w1, ref d1);
			HighInport1.ReadPort(ref t3, ref  p3, ref h3, ref s3, ref w3, ref d3);

			if (this.Engine.boolDesign == true)
			{
				this.Wades = w1;
				this.PreDesignDPRatio = this.PreDP / p1;
				this.PreDesignDPRatio2 = this.PreDP2 / p3;
			}
			this.PreDPRatio = this.PreDesignDPRatio * (w1/ Wades);
			this.PreDPRatio2 = this.PreDesignDPRatio2 * (w1 / Wades);
			p1 = p1 * (1 - this.PreDPRatio);
			p3 = p3 * (1 - this.PreDPRatio2);

			if (this.Engine.boolDesign == true)
			{
				this.Wades = w1;
				this.DesignDPRatio = this.DP / p1;
				this.DesignDPRatio2 = this.DP2 / p3;
			}
			this.DPRatio = this.DesignDPRatio * (w1 / Wades);
			this.DPRatio2 = this.DesignDPRatio2* (w1 / Wades);


			w2 = w1;
			w4 = w3;
			p2 = p1 * (1 -this.DPRatio);
			p4 = p3* (1 - this.DPRatio2);

			double DHideal = h3 - aa.ReturnH(t1, p4);
			h4 = h3 - DHideal * this.Eta;
			t4 = aa.TFromH(p4, h4);
			s4 = aa.ReturnS(t4, p4);
			d4 = aa.ReturnD(t4, p4);//高温端出口



			double h4ideal = aa.ReturnH(t1, p3);
			h2 = h1 + DHideal * this.Eta;
			t2 = aa.TFromH(p2, h2);
			d2 = aa.ReturnD(t2, p2);
			s2 = aa.ReturnS(t2, p2);//低温端出口

			realLowInport.WritePort(t1, p1, h1, s1, w1, d1);
			LowOutport.WritePort(t2, p2, h2, s2, w2, d2);
			HighInport0.WritePort(t3, p3, h3, s3, w3, d3);
			HighOutport.WritePort(t4, p4, h4, s4, w4, d4);
		}

	}
}
