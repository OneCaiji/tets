using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace SCO2
{
	public enum MapType
	{
		Compressor, Turbine, Propeller
	};

	/// <summary>
	/// 查找特性图和加载特性图的类
	/// 20081031 王召广 
	/// 只需要实例化对象，仅仅调用 Find (string Name,double P,double A)就可以得出结果
	/// 
	/// 注意一个特性图的单元内部不要出现空格行
	/// 
	/// 
	/// </summary>
	[Serializable]
	public class CCompMap
	{
		/// <summary>
		/// 文件路径
		/// </summary>
		public string FilePath;
		/// <summary>
		/// 读入文件错误信息
		/// </summary>
		public string ErrorInfo = "";
		/// <summary>
		/// 文件单元表
		/// </summary>
		public Hashtable MLIST = new Hashtable();
		/// <summary>
		/// 插值计算中的警告信息
		/// </summary>
		public ArrayList MapCalError = new ArrayList();
		public CCompMap(string fileName)
		{
			FilePath = fileName;

		}
		/// <summary>
		/// 特性图的单元
		/// </summary>
		[Serializable]
		struct MapUnit
		{
			/// <summary>
			/// 表格名
			/// </summary>
			public string Name;
			/// <summary>
			/// 表格关键字
			/// </summary>
			public double key;
			/// <summary>
			/// 行数
			/// </summary>
			public int k1;
			/// <summary>
			/// 列数
			/// </summary>
			public int k2;
			/// <summary>
			/// 列坐标
			/// </summary>
			public double[] A;
			/// <summary>
			/// 行坐标
			/// </summary>
			public double[] P;
			/// <summary>
			/// 表格内容值
			/// </summary>
			public double[,] F;

		}

		/// <summary>
		/// 用于绘图的基本结构体
		/// </summary>

		public MapType mapType;

		public RawMapUnit ConvertData(ref bool getDate)
		{
			RawMapUnit RawMap = new RawMapUnit();
			MapUnit xMap, yMap, zMap;
			try
			{
				if (mapType == MapType.Turbine) CreatPRofTurbin();
				xMap = (MapUnit)this.MLIST["Mass"];
				yMap = (MapUnit)this.MLIST["Pressure"];
				zMap = (MapUnit)this.MLIST["Efficiency"];
				RawMap.nl = xMap.k1;
				RawMap.np = xMap.k2;
				RawMap.vl = new double[RawMap.nl];
				RawMap.x = new double[RawMap.nl, RawMap.np];
				RawMap.y = new double[RawMap.nl, RawMap.np];
				RawMap.z = new double[RawMap.nl, RawMap.np];


				RawMap.NMax = 0.0;
				RawMap.NMin = 100;
				RawMap.EffMax = 0.0;
				RawMap.EffMin = 1.0;


				for (int i = 0; i < RawMap.nl; i++)
				{
					RawMap.vl[i] = xMap.P[i];

					if (RawMap.vl[i] < RawMap.NMin)//找出等转速线最小值_郑
					{
						RawMap.NMin = RawMap.vl[i];
					}
					if (RawMap.vl[i] > RawMap.NMax)//找出等转速线最大值_郑
					{
						RawMap.NMax = RawMap.vl[i];
					}

				}

				for (int i = 0; i < RawMap.nl; i++)
					for (int j = 0; j < RawMap.np; j++)
					{
						RawMap.x[i, j] = xMap.F[i, j];
						RawMap.y[i, j] = yMap.F[i, j];
						RawMap.z[i, j] = zMap.F[i, j];


						if (RawMap.z[i, j] > RawMap.EffMax) //找出效率最大值_郑
						{
							RawMap.EffMax = RawMap.z[i, j];
						}
						if (RawMap.z[i, j] < RawMap.EffMin) //找出效率最小值_郑
						{
							RawMap.EffMin = RawMap.z[i, j];
						}



					}
				getDate = true;

			}
			catch
			{
				getDate = false;
			}
			return RawMap;
		}   //2015.2.15 绘图用转化函数

		/// <summary>
		/// 将图的KEY分解为矩阵的行列数
		/// 极易产生错误的小函数
		/// </summary>
		/// <param name="Key"></param>
		/// <param name="K1"></param>
		/// <param name="K2"></param>
		void CalKey(double Key, ref int K1, ref  int K2)
		{
			K1 = (int)Key - 1;
			double k22 = 1000 * (Key - K1 - 1);
			K2 = (int)(k22 - 0.5);

		}
		/// <summary>
		/// 加载文件；并且输入特性图的某一表格名称和P\A值，
		/// </summary>
		/// <param name="Name">名称</param>
		/// <param name="P"> 线特征</param>
		/// <param name="A"> 点特征值</param>
		/// <returns></returns>
		public double Find(string Name, double P, double A)
		{

			MapUnit map = (MapUnit)MLIST[Name];
			//return (Lagrange2D(map.P, map.A, map.F, map.k1, map.k2, ref P, ref A));
			return (Line2D(map.P, map.A, map.F, map.k1, map.k2, ref P, ref A));

		}

		/// <summary>
		/// 得到涡轮的压降
		/// </summary>
		public void CreatPRofTurbin()
		{
			double x0 = 0, x1 = 0;
			MapUnit map = new MapUnit();
			MapUnit map2 = (MapUnit)this.MLIST["Mass"];
			MapUnit mapMin = (MapUnit)this.MLIST["Min"];
			MapUnit mapMax = (MapUnit)this.MLIST["Max"];
			map.Name = "Pressure";
			map.k1 = map2.k1;
			map.k2 = map2.k2;
			map.key = map2.key;

			map.P = new double[map.k1];
			map.A = new double[map.k2];
			map.F = new double[map.k1, map.k2];

			for (int i = 0; i < map.k1; i++)
				map.P[i] = map2.P[i];
			for (int i = 0; i < map.k2; i++)
				map.A[i] = map2.A[i];

			//map.P = map2.P.;  //小心
			//map.F = map2.F.Clone();  //小心
			for (int i = 0; i < map.k1; i++)
				for (int j = 0; j < map.k2; j++)
				{
					x0 = mapMin.F[0, i];
					x1 = mapMax.F[0, i];
					map.F[i, j] = x0 + (x1 - x0) * map2.A[j];

				}
			this.MLIST["Pressure"] = map;
		}


		/// <summary>
		/// //一元三点拉格朗日插值,做成静态的为以后可调导叶的时候中
		/// //输入量：x-坐标数组；y-对应于坐标点的值；n-维数；t-插值坐标位置
		///返回值：对应于插值坐标位置的值
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="n"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		double Lagrange1D(double[] x, double[] y, int n, ref double t)
		{
			int i = 0;
			int j = 0;
			int k = 0;
			int m = 0;
			double z = 0.0;
			double s = 0.0;

			if (t < x[0])
			{
				t = x[0];
				MapCalError.Add("一元插值的下边界以下");
			}
			else if (t > x[n - 1])
			{
				t = x[n - 1];
				MapCalError.Add("一元插值的上边界以上");
			}

			if (n < 1) return z;
			if (n == 1) { z = y[0]; return z; }
			if (n == 2)
			{
				z = (y[0] * (t - x[1]) - y[1] * (t - x[0])) / (x[0] - x[1]);
				return z;
			}
			if (t <= x[1]) { k = 0; m = 2; }
			else if (t >= x[n - 2]) { k = n - 3; m = n - 1; }
			else
			{
				k = 1;
				m = n;
				while ((m - k) != 1)
				{
					i = (k + m) / 2;
					if (t < x[i - 1]) m = i;
					else k = i;
				}
				k = k - 1;
				if (Math.Abs(t - x[k]) < Math.Abs(t - x[m])) k = k - 1;
				else m = m + 1;
			}
			for (i = k; i <= m; i++)
			{
				s = 1.0;
				for (j = k; j <= m; j++)
					if (j != i) s = s * (t - x[j]) / (x[i] - x[j]);
				z = z + s * y[i];
			}
			return z;

		}



		double Line2D(double[] x, double[] y, double[,] z, int n, int m, ref double u, ref double v)
		{
			if (n < 2 || m < 2)
				return -1;               //异常
			int i;
			int N1 = 0, N2 = 0, M1 = 0, M2 = 0;

			if (u <= x[0])
			{
				N1 = 0; N2 = 1;
			}
			else if (u >= x[n - 1])
			{
				N1 = n - 2; N2 = n - 1;
			}
			else
			{
				for (i = 1; i < n; i++)
				{
					if (u < x[i])
					{
						N1 = i - 1;
						N2 = i;
						break;

					}

				}
			}
			//**************************选找到行标两个
			if (v <= y[0])
			{
				M1 = 0; M2 = 1;
			}
			else if (v >= y[m - 1])
			{
				M1 = m - 2; M2 = m - 1;
			}
			else
			{
				for (i = 1; i < m; i++)
				{
					if (v < y[i])
					{
						M1 = i - 1;
						M2 = i;
						break;

					}

				}
			}
			//**************************选找到列标两个
			double NLV = z[N1, M1] + (z[N1, M2] - z[N1, M1]) * (v - y[M1]) / (y[M2] - y[M1]);
			double NHV = z[N2, M1] + (z[N2, M2] - z[N2, M1]) * (v - y[M1]) / (y[M2] - y[M1]);
			double value = NLV + (NHV - NLV) * (u - x[N1]) / (x[N2] - x[N1]);
			return value;

		}

		/// <summary>
		///   //二元三点拉格朗日插值//输入量：x、y-坐标数组；z-对应于坐标点的值；n、m-维数；u、v-插值坐标位置
		///    返回值：对应于插值坐标位置的值
		///    调用函数：无
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="n"></param>
		/// <param name="m"></param>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>        
		public double Lagrange2D(double[] x, double[] y, double[,] z, int n, int m, ref double u, ref double v)
		{
			double[] b = new double[3];
			double h, w;
			int nn, mm, ip, iq, i, j, k, l;
			#region 寻找P的下标  nn,ip
			if (u < x[0])
			{
				u = x[0];
				MapCalError.Add("二元插值的P小于第一行！");

			}
			else if (u > x[n - 1])
			{
				u = x[n - 1];
				MapCalError.Add("二元插值的P大于最后一行");
			}

			nn = 3;
			w = 0.0;
			if (n <= 3) { ip = 0; nn = n; } //若提供的数据过少，输入值 王召广
			else if (u <= x[1]) ip = 0;
			else if (u >= x[n - 2]) ip = n - 3;
			else
			{
				i = 1;
				j = n;
				while (((i - j) != 1) && ((i - j) != -1))
				{
					l = (i + j) / 2;
					if (u < x[l - 1]) j = l;
					else i = l;
				}
				if (Math.Abs(u - x[i - 1]) < Math.Abs(u - x[j - 1])) ip = i - 2;
				else ip = i - 1;
				// ip 是Y第一个点的坐标下索引 王召广
			}
			#endregion
			# region 寻找A的下标 mm，iq
			if (v < y[0])
			{
				v = y[0];
				MapCalError.Add("二元插值的A小于第一列A1");
			}
			else if (v > y[m - 1])
			{
				v = y[m - 1];
				MapCalError.Add("二元插值的A大于最后一列A(n-1)");
			}

			mm = 3;
			if (m <= 3) { iq = 0; mm = m; }
			else if (v <= y[1]) iq = 0;
			else if (v >= y[m - 2]) iq = m - 3;
			else
			{
				i = 1;
				j = m;
				while (((i - j) != 1) && ((i - j) != -1))
				{
					l = (i + j) / 2;
					if (v < y[l - 1]) j = l;
					else i = l;
				}
				if (Math.Abs(v - y[i - 1]) < Math.Abs(v - y[j - 1])) iq = i - 2;
				else iq = i - 1;
				//iq是Z的第一个索引 王召广
			}
			#endregion
			//! 找到两个下标后，寻找三个点  王召广
			for (i = 0; i <= nn - 1; i++)
			{
				b[i] = 0.0;
				for (j = 0; j <= mm - 1; j++)
				{
					//k = m*(ip+i)+(iq+j);	//第一行（y1）下标  C++方式
					//h = z[k];
					h = z[ip + i, iq + j];

					for (k = 0; k <= mm - 1; k++)	//
						if (k != j)
							h = h * (v - y[iq + k]) / (y[iq + j] - y[iq + k]);
					b[i] = b[i] + h;
				}
			}
			for (i = 0; i <= nn - 1; i++)
			{
				h = b[i];
				for (j = 0; j <= nn - 1; j++)
					if (j != i)
						h = h * (u - x[ip + j]) / (x[ip + i] - x[ip + j]);
				w = w + h;
			}
			return w;
		}

		/// <summary>
		/// 读入文件
		/// </summary>
		/// <returns></returns>
		public bool Load()
		{

			MapUnit M;
			//int IM=0; 原本是想用此来读取个数gesh，后用hashtable
			string strName = "";
			bool getUnit = false;  //得到单元
			bool getKey = false; //得到第一个key


			using (StreamReader sr = new StreamReader(FilePath))
			{

				int iLine = 0;
				string first = "";
				string sLine = "";

				while (!sr.EndOfStream)  //读到最后
				{
					sLine = sr.ReadLine();
					iLine++;
					int I = 0, J = 0;

					if (sLine == " " || sLine == "")
					{ continue; }  //读到空格继续读                      

					string[] aLine = Regex.Split(sLine, @"\s+");//拆分单行
					ArrayList list = new ArrayList();
					foreach (string ob in aLine)
					{
						if (ob == "") continue;
						list.Add(ob);
					}                                          //list 中为无空格的每行对象                 

					if (getUnit == true && list.Count > 5) ErrorInfo += iLine.ToString() + "行多于5数据\n";
					if (list.Count > 0)
						first = list[0].ToString();           //取出第一word

					if (first == "99" || first == "Reynolds:")
						continue;

					if (first == "Mass" || first == "Efficiency" || first == "Pressure" || first == "Surge" || first == "Min" || first == "Max")
					{

						if (first == "Min" || first == "Max") mapType = MapType.Turbine;
						if (first == "Power" || first == "Static") mapType = MapType.Propeller;

						getUnit = true;
						getKey = false;
						strName = first;
						continue; //进行下次读取

					}
					//********************************************************************************

					if (getUnit == true)
					{
						#region 读取单个表格
						M = new MapUnit();
						M.Name = strName;
						M.key = Convert.ToDouble(first);
						CalKey(M.key, ref M.k1, ref M.k2);
						M.A = new double[M.k2];
						M.P = new double[M.k1];
						M.F = new double[M.k1, M.k2];

						while (I < M.k1)
						{

							if (getKey == true) //读取内容是F的每行，将行特征P记录，当第一次读列特征是不进入此部分
							{
								#region 仅读表内容存在
								//读取新的一行
								sLine = sr.ReadLine();
								iLine++;
								if (sLine == " " || sLine == "")
								{ continue; }  //读到空格继续读     

								aLine = Regex.Split(sLine, @"\s+");//拆分单行
								list.Clear();
								foreach (string ob in aLine)
								{
									if (ob == "") continue;
									list.Add(ob);
								}
								if (list.Count != 5) ErrorInfo += iLine.ToString() + "行出错\n";
								if (list.Count > 0)
									first = list[0].ToString();           //取出第一word

								M.P[I] = Convert.ToDouble(first);
								#endregion
							}

							for (int i = 1; i < list.Count; i++)
							{
								#region 特征行的其它变量
								if (getKey == false)                          //注意下标
									M.A[J] = Convert.ToDouble(list[i].ToString());
								else
									M.F[I, J] = Convert.ToDouble(list[i].ToString());
								J++;
								#endregion
							}
							while (J < M.k2)
							{
								#region 读含特征行的其它行
								sLine = sr.ReadLine();
								iLine++;
								if (sLine == " " || sLine == "")
								{ continue; }  //读到空格继续读 


								string[] bLine = Regex.Split(sLine, @"\s+");//拆分单行
								ArrayList blist = new ArrayList();
								foreach (string ob in bLine)
								{
									if (ob == "") continue;
									blist.Add(ob);
								}                                          //list 中为无空格的每行对象  

								for (int i = 0; i < blist.Count; i++)
								{
									if (getKey == false)
										M.A[J] = Convert.ToDouble(blist[i].ToString());
									else
										M.F[I, J] = Convert.ToDouble(blist[i].ToString());
									J++;
								}
								#endregion

							}
							if (getKey == false)
								getKey = true; //读取第一行的列特征量A后、要是真，以进入I循环，读行F
							else I++;
							J = 0;

						}
						//读完第一部分转到这里 A
						getUnit = false;
						#endregion
						MLIST[M.Name] = M;
					}

				}
				//Console.WriteLine("找到行数" + iLine.ToString());




			}
			Console.WriteLine(ErrorInfo);
			return true;

		}

		/// <summary>
		/// 读入简单格式的文件
		/// </summary>
		/// <returns></returns>
		public void LoadSample()
		{

			this.MLIST.Clear();
			MapUnit M;
			//int IM=0; 原本是想用此来读取个数gesh，后用hashtable
			string strName = "";
			bool getUnit = false;  //得到单元
			bool getA = false; //得到第一个key
			bool getP = false;
			using (StreamReader sr = new StreamReader(FilePath))
			{

				int iLine = 0;
				string first = "";
				string sLine = "";

				while (!sr.EndOfStream)  //读到最后
				{
					sLine = sr.ReadLine();
					iLine++;
					if (sLine == "" || sLine == " ")
					{ continue; }  //读到空格继续读 或没有内容的行   
					else if (sLine.Substring(0, 1) == "*")
					{ continue; }

					else
					{
						string[] aLine = Regex.Split(sLine, @"\s+");//拆分单行
						ArrayList list = new ArrayList();
						foreach (string ob in aLine)
						{
							if (ob == "") continue;
							list.Add(ob);
						}                                          //list 中为无空格的每行对象 


						if (list.Count > 0)
							first = list[0].ToString();           //取出第一word


						if (first == "Mass" || first == "Efficiency" || first == "Pressure" || first == "Surge" || first == "DHT")
						{
							getUnit = true;
							getA = false;
							getP = false;
							strName = first;
							continue; //进行下次读取

						}

						if (getUnit == true)
						{


							M.Name = strName;
							M.k1 = Convert.ToInt32(list[0].ToString());
							M.k2 = Convert.ToInt32(list[1].ToString());
							M.A = new double[M.k2];
							M.P = new double[M.k1];
							M.F = new double[M.k1, M.k2];
							M.key = M.k1 + 1 + (M.k2 + 1) / 1000.0;
							int I = 0;//用于给每行读入数据做个计数

							while (getUnit)
							{

								sLine = sr.ReadLine();
								iLine++;
								if (sLine == "" || sLine == " ")
								{ continue; }  //读到空格继续读    
								else if (sLine.Substring(0, 1) == "*")
								{ continue; }  //读到*继续读 
								else
								{
									#region 开始读入单元
									//aLine = " ";
									aLine = Regex.Split(sLine, @"\s+");//拆分单行
									list.Clear();
									foreach (string ob in aLine)
									{
										if (ob == "") continue;
										list.Add(ob);
									}                                          //list 中为无空格的每行对象 

									if (getP == false)
									{
										for (int i = 0; i < list.Count; i++)
										{
											M.P[i] = Convert.ToDouble(list[i]);
										}
										getP = true;

									}
									else if (getA == false)
									{

										for (int i = 0; i < list.Count; i++)
										{
											M.A[i] = Convert.ToDouble(list[i]);
										}
										getA = true;

									}
									else
									{
										for (int i = 0; i < list.Count; i++)
										{
											M.F[I, i] = Convert.ToDouble(list[i]);
										}
										I++;
									}
								}
									#endregion
								if (I == M.k1)
								{
									getUnit = false;
									MLIST[M.Name] = M;
								}
							} //while (getUnit)

						}// if (getUnit == true)
					}//外循环中没有注释和空格行
				} //外循环

			}//using
		}//函数


		public void EffNocrToBeta(double effective, double n, ref double Beta1, ref double Beta2)
		{
			double BetaEffMax = 0;
			double BetaEffMin1 = 0;   //效率曲线起点
			double BetaEffMin2 = 1;   //效率曲线终点
			double EffMax = 0;
			Beta1 = 0;
			Beta2 = 1;
			double nocr = n;         //为输入值，为原始的、已有的nocr
			double Eff = effective;
			double delteff;//迭代值与目标值的差
			//粗略找出effective最大处的beta值；效率最大值；曲线起始点的效率值
			double CutNumber = 100;
			double betah = (BetaEffMin2 - BetaEffMin1) / CutNumber;
			double effmin1 = Find("Efficiency", nocr, BetaEffMin1);
			double effmin2 = Find("Efficiency", nocr, BetaEffMin2);
			for (int k = 0; k < CutNumber; k++)
			{
				double nonuse = Find("Efficiency", nocr, betah * k);

				if (EffMax < nonuse)
				{
					BetaEffMax = BetaEffMax + betah;
					EffMax = nonuse;

				}
			}
			if (BetaEffMax >= 1)
			{
				BetaEffMax = 1;
			}

			////粗略找出effective最大处的beta值；效率最大值；曲线起始点的效率值
			if (EffMax < Eff) //取得效率值大于最大值或小于最小值
			{
				Beta1 = 0;
				Beta2 = 1;
			}
			////有极致，找出效率曲线下降段合适的Beta
			if (effmin1 < Eff)
			{
				double Betaeffmax1 = BetaEffMax;
				double ThisCounter = 0;
				do
				{
					Beta1 = (BetaEffMin1 + Betaeffmax1) / 2;
					double eff1 = Find("Efficiency", nocr, Beta1);
					delteff = eff1 - Eff;
					ThisCounter++;
					if (delteff < 0)
					{
						BetaEffMin1 = Beta1;
					}
					else
					{
						Betaeffmax1 = Beta1;
					}
					if (ThisCounter > 100)
					{
						Beta1 = 0;
						break;
					}
				}
				while (Math.Abs(delteff) > 0.001);
				//return Beta1;
			}
			else
			{
				Beta1 = 0;
			}
			////有极致，找出效率曲线下降段的Beta
			////有极致，找出效率曲线上升段的Beta
			if (effmin2 < Eff)
			{
				double Betaeffmax2 = BetaEffMax;
				double ThisCounter = 0;
				do
				{
					Beta2 = (BetaEffMin2 + Betaeffmax2) / 2;
					double eff2 = Find("Efficiency", nocr, Beta2);
					delteff = eff2 - Eff;
					ThisCounter++;
					if (delteff < 0)
					{
						BetaEffMin2 = Beta2;
					}
					else
					{
						Betaeffmax2 = Beta2;
					}
					if (ThisCounter > 100)
					{
						Beta2 = 1;
						break;
					}
				}
				while (Math.Abs(delteff) > 0.001);
				//return Beta2;
			}
			else
			{
				Beta2 = 1;
			}
			////有极致，找出效率曲线上升段的Beta
		}



		public Mydraw1Character EffFindNocr(double Nmin, double Nmax, double Effect)
		{
			Mydraw1Character Point;       //返回数组
			double Ncor = Nmin;
			double Eff = Effect;
			int Counter1 = 0;
			int Counter2 = 0;
			int LineNumber = 100;
			double[] npr1 = new double[LineNumber];
			double[] npr2 = new double[LineNumber];
			double[] wa1 = new double[LineNumber];
			double[] wa2 = new double[LineNumber];
			double[] eff = new double[LineNumber];
			double Beta1 = 0;
			double Beta2 = 1;
			double DhNocr = (Nmax - Nmin) / LineNumber;
			for (int k = 0; k < LineNumber; k++)
			{
				Ncor = Nmin + DhNocr * k;
				if (Ncor >= Nmax)
				{
					break;
				}
				EffNocrToBeta(Eff, Ncor, ref  Beta1, ref Beta2);
				//EffNocrToBeta(Eff, 0.86, ref  Beta1, ref Beta2);          
				if (Beta1 != 0)
				{
					npr1[Counter1] = Find("Pressure", Ncor, Beta1);
					wa1[Counter1] = Find("Mass", Ncor, Beta1);
					Counter1 = Counter1 + 1;
				}
				if (Beta2 != 1)
				{
					npr2[Counter2] = Find("Pressure", Ncor, Beta2);
					wa2[Counter2] = Find("Mass", Ncor, Beta2);
					Counter2 = Counter2 + 1;
				}
			}

			Point.eff = eff;
			Point.npr1 = npr1;
			Point.npr2 = npr2;
			Point.wa1 = wa1;
			Point.wa2 = wa2;
			return Point;
		}
		public Mydraw2Character EffFindNocr2(double[] Ncor, double Efficiency)
		{
			Mydraw2Character MyPoint;
			double Beta1 = 0;
			double Beta2 = 1;
			int LengthofNcor;
			LengthofNcor = Ncor.Length;
			double[] x1 = new double[LengthofNcor];
			double[] x2 = new double[LengthofNcor];
			double[] y1 = new double[LengthofNcor];
			double[] y2 = new double[LengthofNcor];
			for (int i = 0; i < LengthofNcor; i++)
			{
				EffNocrToBeta(Efficiency, Ncor[i], ref  Beta1, ref Beta2);
				if (Beta1 != 0)
				{
					y1[i] = Find("Pressure", Ncor[i], Beta1);
					x1[i] = Find("Mass", Ncor[i], Beta1);
				}
				if (Beta2 != 1)
				{
					y2[i] = Find("Pressure", Ncor[i], Beta2);
					x2[i] = Find("Mass", Ncor[i], Beta2);
				}
			}
			MyPoint.eff = Efficiency;
			MyPoint.Ncor1 = Ncor;
			MyPoint.wa1 = x1;
			MyPoint.wa2 = x2;
			MyPoint.npr1 = y1;
			MyPoint.npr2 = y2;
			return MyPoint;
		}
	} //类

	public struct RawMapUnit                            // 2015.2.15 为了绘图需要添加！！！！！！！！！！！！！！！！！
	{
		public int nl;
		public int np;
		public double[] vl;
		public double[,] x;
		public double[,] y;
		public double[,] z;
		public double NMax;
		public double NMin;
		public double EffMax;
		public double EffMin;
	}

	public struct Mydraw1Character //对应case1的结构体
	{
		public double[] eff;

		public double[] npr1;
		public double[] wa1;//第一条线

		public double[] npr2;
		public double[] wa2;//第二条线
	}
	public struct Mydraw2Character //对应case1的结构体
	{
		public double eff;
		public double[] Ncor1;
		public double[] npr1;
		public double[] wa1;//第一条线

		public double[] npr2;
		public double[] wa2;//第二条线
	}

}
