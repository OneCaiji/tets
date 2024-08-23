using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	public class CMixees: Component 
	{
		public PortofGas InPort;
		public PortofGas CoolIn;
		public PortofGas OutPort;

		public CMixees(Engine eng)
		{
			InPort = new PortofGas();
			CoolIn = new PortofGas();
			OutPort = new PortofGas();
			this.Engine = eng;
			this.Engine.ComponentList.Add(this);
		}
		public override void Run(int N)
		{
			double t1 = 0, p1 = 0, h1 = 0, s1 = 0, d1 = 0, w1 = 0;//入口
			double t3 = 0, p3 = 0, h3 = 0, s3 = 0, d3 = 0, w3 = 0;//冷气入口
			double t2 = 0, p2 = 0, h2 = 0, s2 = 0, d2 = 0, w2 = 0;//出口
			sco2Calculate aa = new sco2Calculate();

			InPort.ReadPort(ref t1, ref  p1, ref h1, ref s1, ref w1, ref d1);
			CoolIn.ReadPort(ref t3, ref  p3, ref h3, ref s3, ref w3, ref d3);

			h2 = (h1 * w1 + h3 * w3) / (w3 + w1);
			t2 = aa.TFromH(p1, h2);
			s2 = aa.ReturnS(t2, p1);
			d2 = aa.ReturnD(t2, p1);

			OutPort.WritePort(t2, p2, h2, s2, w2, d2);
		}
	}
}
