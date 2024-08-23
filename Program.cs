using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SCO2
{
	class Program
	{	
		static void Main(string[] args)
		{
			SimpleSCO2 aa = new SimpleSCO2(); 
			aa.DesignPoint();
			aa.OffCal();
		}
	}
	public class SimpleSCO2:Engine 
	{
		StreamWriter sw = new StreamWriter("Performance.csv");
		StreamWriter sw1 = new StreamWriter("Off-performance.csv");
		#region 总体性能定义
		double 压比=0;
		double 压气机功 = 0;
		double 输出功 = 0;
		double 吸收热 = 0;
		double 效率 = 0;
		#endregion
		#region 部件定义
		Compressor 压气机;
		CTurbine 涡轮;
		CExchange 热交换器;
		Source 热源 ;
		Condenser 冷凝器 ;
		CSolver solver;
		#endregion
		public SimpleSCO2()
		{
			GasPortList = new Dictionary<string, PortofGas>();
			this.压气机 = new Compressor(this);//1
			this.涡轮 = new CTurbine(this);//2
			this.热交换器 = new CExchange(this);//5
			this.热源 = new Source(this);//7
			this.冷凝器 = new Condenser(this);//10

		}
		public override void AssembleEngine()
		{
			涡轮.InPort = 压气机.OutPort;
			涡轮.MechPort = 压气机.MechPort;
			涡轮.BleedIn = 压气机.BleedOut;
			热交换器.LowInport = 压气机.OutPort;
			热交换器.HighInport1 = 涡轮.OutPort;
			热源.Inport = 热交换器.LowOutport;		
			冷凝器.Inport = 热交换器.HighOutport;
			冷凝器.BleedIn = 压气机.BleedToCon ;

		}
		public override void ReadData(string inputfileName)
		{
			#region 设计参数赋值
			double HighDP = 80;
			double LowDP = 20;
			double Wa = 12.61;
			//double DW1= 0.005;
			double P2 = 8130;
			double T2 = 313;
			double EffC = 0.65;
			double T4 = 673;
			double P4 = 16000;
			double EffT = 0.75;
			double EffEX = 0.93;

			double P3 = P4 + (2 * HighDP + 3 * LowDP);
			double P5 = P2 + (2 * HighDP + 3 * LowDP);
			double PRC = P3 / P2;
			double PRT = P4 / P5;
			#endregion 

			压气机.MapPath = "Compresoor.MAP";
			压气机.MapRds = 0.53;
			压气机.MechNds = 100;
			压气机.PRds = PRC;
			压气机.Wads = Wa;
			压气机.Effds = EffC;
			压气机.Tin = T2;
			压气机.Pin = P2;
			//压气机.WBleedOut = DW1;

			涡轮.MapPath = "MainTurbine.MAP";
			涡轮.MapR = 0.54;
			涡轮.CNTds = 1;
			涡轮.PRds = PRT;
			涡轮.Effds = EffT;
			涡轮.Tin = T4;
			涡轮.P5 = P5;
			涡轮.PreDP = 2 * HighDP + 3 * LowDP;


			热交换器.DP =HighDP;
			热交换器.DP2 = HighDP;
			热交换器.PreDP = LowDP;
			热交换器.PreDP2 = LowDP;
			热交换器.Eta =EffEX;

			热源.Tout = T4;
			热源.DP = HighDP;
			热源.PreDP = LowDP;
			

			冷凝器.DP = HighDP;
			冷凝器.PreDP = LowDP;
			冷凝器.T2 = T2;
			solver = new CSolver(this, 50, 3);  
		}
		public override void DesignPoint()
		{
			this.sw.WriteLine("************************************design point***********************************");
			boolDesign = true;
			this.AssembleEngine();
			this.ReadData("parameters are in engine source not file");
			this.ErrorInfo.Clear();//每次运行需要将错误列表清空，
			this.ResultInfo.Clear(); //将每次计算结果清空，否则达不到效果
			this.ComponetInfo.Clear(); //不清空则多次运行必出错		
			DoSequenceCal();
			AddList();
			Out();
			//this.sw.Close();

		}
		public void OffCal()
		{
			bool ISOK = false;
			this.sw1.WriteLine("T4,POWER,EFFICIENCY");
			//压气机.Tin = 336;
			//冷凝器.T2 = 336;
			double TXX = 930;
			热源.Tout = TXX;
			涡轮.Tin = TXX;
			this.涡轮.powerOut = 7000;	
			for (int i = 0; i < 1; i++)
			{
				for (int ijk = 0; ijk < 100; ijk++)
				{
					OffDesign(ref ISOK);
					if (ISOK)
					{
						if (Math.Abs(this.压气机.MechN - 100) < 0.1)
						{
							break;
						}
						this.涡轮.powerOut = this.涡轮.powerOut - 50 * (this.压气机.MechN - 100);
					}
					else
					{
						break;
					}

				}
				if (ISOK)
				{
					Out();
					this.sw1.WriteLine("{0},{1},{2}", this.热源.Tout, this.输出功, this.效率);
				}
				热源.Tout = 热源.Tout-10;
				涡轮.Tin = 涡轮.Tin-10;
				if (热源.Tout <800)
				{
					break;
				}
			}							
			this.sw.Close();
			this.sw1.Close();
		}
		public override void OffDesign(ref bool IsRun)
		{
			//this.主压气机.WAscaller = 1;
			//this.主压气机.PRscaller = 1;
			//this.主压气机.Effscaller = 1;
			//this.压缩涡轮.WGcscaller = 1;
			//this.压缩涡轮.PRscaller = 1;
			setInitialVars(IsRun);
			this.ErrorInfo.Clear();//每次运行需要将错误列表清空，
			this.ResultInfo.Clear(); //将每次计算结果清空，否则达不到效果
			boolDesign = false;
			try
			{
				double[] xx = new double[3];
				#region using lastest solution as initials
				xx[0] = InitalVars[0];
				xx[1] = InitalVars[1];
				xx[2] = InitalVars[2];
				#endregion

				this.ResultInfo.AppendFormat("迭代前：变量1={0:F3}  变量2={1:F3}  变量3={0:F3} ", xx[0], xx[1], xx[2]);
				this.ResultInfo.AppendLine();
				if (solver.Solve(xx))
				{
					#region setting initials for next
					InitalVars[0] = xx[0];
					InitalVars[1] = xx[1];
					InitalVars[2] = xx[2];
					#endregion
					this.ResultInfo.AppendFormat("迭代前：变量1={0:F3}  变量2={1:F3}  变量3={0:F3} ", xx[0], xx[1], xx[2]);

					IsRun = true;
				}
				else
				{
					this.ResultInfo.Append("迭代意外终止！！");
					IsRun = false;
				}
			}
			catch
			{
			    IsRun = false;
			}
		}
		void Out()
		{
	
			this.压比 = 压气机.PR;
			this.压气机功 = 压气机.Work;
			this.输出功 = this.涡轮.WorkProvide  - this.压气机功;
			this.吸收热 = 热源.SourceDH;
			this.效率 = this.输出功 / this.吸收热;
			PrintSections();
			string Performance = PrintSections();
			this.sw.WriteLine(Performance.ToString());
		}
		public override string Performance()
		{
			string aa = "aa";
			return aa;
		}
		public override void SingleRun(double[] X, double[] Y)
		{
			this.ErrorList.Clear();
			int I = 0;
			int xxxx = 0;
			this.压气机.MapR =X[0];
			this.涡轮.MapR=X[1];
			this.压气机.MechN = X[2];
			DoSequenceCal();
			Console.WriteLine("***********");
			foreach (object OB in this.ErrorList)
			{
				Y[I++] = Convert.ToDouble(OB);
				Console.Write("迭代变量={0}   误差={1}", X[xxxx], OB);
				Console.WriteLine();
				xxxx++;
			}
			Console.WriteLine();
		}
		public void setInitialVars(bool xxx)
		{
			this.InitalVars=new double [3];
			if (xxx)
			{
				this.InitalVars[0] = this.压气机.MapR;
				this.InitalVars[1] = this.涡轮.MapR;
				this.InitalVars[2] = this.压气机.MechN;
			}
			else
			{
				this.InitalVars[0] = 0.5;
				this.InitalVars[1] = 0.5;
				this.InitalVars[2] =100;

			}

		}
		void AddList()
		{
			this.GasPortList.Clear();
			this.GasPortList.Add("2", 压气机 .InPort );
			this.GasPortList.Add("3", 压气机.OutPort );
			this.GasPortList.Add("4", 涡轮.realInport);
			this.GasPortList.Add("5", 涡轮.OutPort);
			this.GasPortList.Add("HX1", 热交换器.realLowInport);
			this.GasPortList.Add("HX2", 热交换器 .LowOutport );
			this.GasPortList.Add("HX3", 热交换器.HighInport0);
			this.GasPortList.Add("HX4", 热交换器.HighOutport);
			this.GasPortList.Add("7", 冷凝器.realInport);
			this.GasPortList.Add("8", 冷凝器 .Outport );
		}	
		public string PrintSections()
		{
			StringBuilder MyStringBuilder = new StringBuilder("=-=-=-=-=-=-=-=-=-=- section parameters-=-=-=-=-=-=-=-=-=-=");
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat(" section,Tt,Pt,Ht,St,Wa,rho");
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat(" Unit,K,MPa,kJ/kg,kg-K,kg/s,kg/m3");
			MyStringBuilder.AppendLine();
			foreach (KeyValuePair<string, PortofGas> ValuePair in this.GasPortList)
			{
				string SR = ValuePair.Key;
				if (ValuePair.Key.Length == 1)
				{
					SR = "  " + SR;
				}
				else if (ValuePair.Key.Length == 2)
				{
					SR = " " + SR;
				}
				MyStringBuilder.Append(" " + SR);
				MyStringBuilder.Append(", ");
				MyStringBuilder.Append(ValuePair.Value.GasTt.ToString("#0.000"));
				MyStringBuilder.Append(",");
				MyStringBuilder.Append((ValuePair.Value.GasPt/1000).ToString("#00.000"));
				MyStringBuilder.Append(",");
				MyStringBuilder.Append(ValuePair.Value.GasHi .ToString("#000.000"));
				MyStringBuilder.Append(",");
				MyStringBuilder.Append(ValuePair.Value .GasSi .ToString("#0.000"));
				MyStringBuilder.Append(",");
				MyStringBuilder.Append(ValuePair.Value.GasFlow .ToString("#0.000"));
				MyStringBuilder.Append(",");
				MyStringBuilder.Append(ValuePair.Value.GasDens .ToString("#0.000"));
				MyStringBuilder.AppendLine();				
			}
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendLine("=-=-=-=-=-=-=-=-=-=- whole engine performance -=-=-=-=-=-=-=-=-=-=");
			MyStringBuilder.AppendFormat("Ng,{0}", this.压气机.MechN .ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("PR, {0}", this.压比.ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("Effc,{0}", this.压气机.Eff.ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("T4,{0}K", this.涡轮 .Tin .ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("LC,{0}kW", this.压气机功.ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("Efft,{0}", this.涡轮 .Effmap .ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("shaft Power,{0}kW", this.输出功.ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("net Power,{0}kW", (0.97* 0.94* this.输出功).ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("exchanger heat,{0}kW", this.吸收热.ToString("#0.000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("Mech efficiency,{0}",this.效率 .ToString("#0.0000"));
			MyStringBuilder.AppendLine();
			MyStringBuilder.AppendFormat("Net efficiency,{0}", (0.97 * 0.94 * this.效率).ToString("#0.0000"));
			MyStringBuilder.AppendLine();
			return (MyStringBuilder.ToString());

		}
	}

}
