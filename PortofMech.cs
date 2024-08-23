using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	public class PortofMech:Port
	{
		/// <summary>
		/// power
		/// </summary>
		public double Work;		//机械功
		/// <summary>
		/// speed
		/// </summary>
		public double MechN;	//机械转速
		/// <summary>
		/// inertia of spool
		/// </summary>
		public double MI;		//惯性矩
		public PortofMech()
		{
			this.Work = 0;
			this.MechN = 0;
			this.MI = 0;

		}
		public void WritePort(double W, double N, Double M)
		{
			this.Work = W;
			this.MechN = N;
			this.MI = M;
		}

		public void ReadPort(ref double W, ref double N, ref Double M)
		{
			W = this.Work;
			N = this.MechN;
			M = this.MI;


		}

	}
}
