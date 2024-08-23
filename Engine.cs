using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SCO2
{
	public abstract class Engine
	{
		public StringBuilder ResultInfo;
		/// <summary>
		/// // 设计点计算(TRUE)
		/// </summary>
		public bool boolDesign;
		/// <summary>
		/// // 发动机非线性方程组方程个数
		/// </summary>
		public int EquationNumber;

		public Dictionary<string, object> ComponetInfo;
		/// <summary>
		/// // 发动机部件列表
		/// </summary>
		public ArrayList ComponentList;
		/// <summary>
		/// //错误列表  
		/// </summary>
		public ArrayList ErrorInfo;
		/// <summary>
		/// // 发动机独立变量列表
		/// </summary>
		public ArrayList VarList;
		/// <summary>
		/// // 发动机误差量列表
		/// </summary>	
		public ArrayList ErrorList;
		public CSolver Solver;
		public double[] InitalVars; 

		public abstract void AssembleEngine();
		public abstract void ReadData(string inputfileName);
		public abstract void DesignPoint();
		public abstract void OffDesign(ref bool isRun);
		public abstract void SingleRun(double[] X, double[] Y);
		public abstract string Performance();
		/// <summary>
		/// 发动机截面参数列表
		/// </summary>
		public Dictionary<string, PortofGas> GasPortList;

		/// <summary>
		/// 实例化列表
		/// </summary>
		public Engine()
		{
			ComponentList = new ArrayList();
			ErrorList = new ArrayList();
			VarList = new ArrayList();
			ErrorInfo = new ArrayList();
			GasPortList = new Dictionary<string, PortofGas>();
			ComponetInfo = new Dictionary<string, object>();
			ResultInfo = new StringBuilder();
		}


		/// <summary>
		/// 从前往后顺序计算
		/// </summary>
		public void DoSequenceCal()
		{
			this.ComponetInfo.Clear(); //不清空则多次运行必出错
			for (int N = 0; N < this.ComponentList.Count; N++)
			{
				Component COM = this.ComponentList[N] as Component;
				COM.Run(N);
			}
		}
	}
}
