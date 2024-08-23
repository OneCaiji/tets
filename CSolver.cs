using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCO2
{
	public class CSolver
	{
		/// <summary>
		/// 方程数目
		/// </summary>
		private int funNum;
		/// <summary>
		/// 所要解决的发动机
		/// </summary>
		Engine Eng;
		/// <summary>
		/// 最大迭代次数
		/// </summary>
		public int MaxTimes;
		/// <summary>
		/// 给定的小偏移量
		/// </summary>
		public double Detah = 0.025;
		public double InteSpd = 1.05;
		/// <summary>
		/// 错误信息
		/// </summary>
		public string ErrorMsg;
		/// <summary>
		/// 收敛判据
		/// </summary>
		public double Eps = 2e-5;
		Random random;

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="forEng">The engine object</param>
		/// <param name="Max">Maximum interative cycles</param>
		/// <param name="Num">the number of errors </param>
		public CSolver(Engine forEng, int Max, int Num)
		{
			this.Eng = forEng;
			this.funNum = Num;
			this.MaxTimes = Max;
			random = new Random();
			this.ErrorMsg = "";
			this.Eng.Solver = this;

		}
		public void Test(double[] b)
		{
			double[] d = new double[1];
			d = b;
			double[,] a = { { 1, 3, 0, 0 }, { 2, 9, 15, 0 }, { 0, 3, 17, 8 }, { 0, 0, 3, 16 } };

			double[] c = new double[5];
			Console.WriteLine(Gauss(a, d, 4, c));

		}

		/// <summary>
		/// 传入初值X，返回ture（迭代存在结果）或者FALSE （不存在结果）
		/// </summary>
		/// <param name="X"></param>
		/// <returns></returns>
		public bool Solve(double[] X)
		{
			// TODO: Add your command handler code here
			int i = 0, j = 0;  //最终迭代次数
			int k = 0;
			double MaxError, z, beta, d, EPS;
			double[] h = new double[this.funNum];  //每个变量的小扰动数组
			double[] y = new double[this.funNum];
			double[] b = new double[this.funNum];//初值
			double[] Deta = new double[this.funNum];
			double[,] a = new double[this.funNum, this.funNum];
			double[] Z = new double[this.funNum];

			for (i = 0; i < this.funNum; i++)
			{
				h[i] = Detah;
				Deta[i] = 0.0;
			}
			EPS = this.Eps;
			MaxError = 1.0 + EPS;
			while (MaxError >= EPS)
			{
				this.Eng.SingleRun(X, Z);	//在此调用发动机非线性方程组
				for (i = 0; i < this.funNum; i++)
				{
					b[i] = Z[i]; 	//b[ ] 误差初值
				}
				//********************
				MaxError = 0.0;
				for (i = 0; i < this.funNum; i++)
				{
					z = Math.Abs(b[i]);
					if (z > MaxError) MaxError = z;  //最大的误差为am

				}

				Console.WriteLine("第{0}次迭代，最大误差：{1}", k.ToString(), MaxError.ToString("E3"));
				//***********************
				if (MaxError >= EPS)
				{
					k = k + 1;
					Console.WriteLine("*****************        " + k.ToString() + "        *****************");
					if (k >= this.MaxTimes)
					{
						this.Eng.ResultInfo.AppendFormat("迭代次数过多，出问题了。共迭代了{0}次！", k);
						return false;
					}
					for (j = 0; j < this.funNum; j++)//Z[ ]是误差列表、n方程个数  
					{
						z = X[j];
						X[j] = X[j] + h[j] * z;  	    //X[ ]是变量列表			王召广
						this.Eng.SingleRun(X, Z);	//在此调用发动机非线性方程组
						for (i = 0; i < this.funNum; i++)
						{
							a[i, j] = (Z[i] - b[i]) / (h[j] * z);
						}
						X[j] = z; //将扰动值恢复
					}
					if (Gauss(a, b, this.funNum, y) == false)
					{
						ErrorMsg = ErrorMsg.Insert(ErrorMsg.Length, " 无法求解线性方程组");
						return false;
					}
					for (i = 0; i < this.funNum; i++)
					{ //计算下次迭代的X
						d = Deta[i] + y[i];
						//X[i] = X[i] - y[i] / InteSpd * random.Next(11, 19) / 20.0;
						//X[i] = X[i] - y[i] / InteSpd;
						if (Math.Abs(Deta[i]) > 1.0e-4 && Math.Abs(d) <= 1.0e-4) //如果这次相对应上次改变量，改变的太大
						{
							X[i] = X[i] - random.Next(11, 15) / 15 * y[i];         //随机的改小甚至不变
						}

						else
						{
							X[i] = X[i] - y[i] / InteSpd;
						}

						Deta[i] = y[i];
					}
					//*************
					beta = 1.0;
					for (i = 0; i < this.funNum; i++) beta = beta - y[i];
					if (Math.Abs(beta) + 1.0 == 1.0)
					{//迭代出现错误
						ErrorMsg = ErrorMsg.Insert(ErrorMsg.Length, " No convergence in Solver!");
						return false;
					}
					//**************	
				}
			}
			i = k;
			this.Eng.ResultInfo.AppendFormat("迭代收敛，共迭代了{0}次！\n", i);
			return true;
		}
		/// <summary>
		/// 解线性方程组的高斯消元法！形式aX=b，n为维数
		/// </summary>
		/// <param name="a">系数矩阵</param>
		/// <param name="b">结构矩阵</param>
		/// <param name="n">维数</param>
		/// <param name="x">结果，使用数组避免ref</param>
		/// <returns></returns>
		bool Gauss(double[,] a, double[] b, int n, double[] x)
		{
			int l, k, i, j, IS = 0, p;
			double d, t;
			int[] js = new int[10];

			l = 1;
			for (k = 0; k <= n - 2; k++)
			{
				d = 0.0;
				for (i = k; i <= n - 1; i++)
					for (j = k; j <= n - 1; j++)
					{
						t = Math.Abs(a[i, j]);
						if (t > d) { d = t; js[k] = j; IS = i; }
					}
				if (d + 1.0 == 1.0) l = 0;
				else
				{
					if (js[k] != k)
						for (i = 0; i <= n - 1; i++)
						{
							//p = i*n+k;
							//q = i*n+js[k];
							t = a[i, k]; a[i, k] = a[i, js[k]]; a[i, js[k]] = t;
						}
					if (IS != k)
					{
						for (j = k; j <= n - 1; j++)
						{
							//p = k*n+j;
							//q = IS*n +j;
							t = a[k, j]; a[k, j] = a[IS, j]; a[IS, j] = t;
						}
						t = b[k]; b[k] = b[IS]; b[IS] = t;
					}
				}
				if (l == 0)
				{
					return false;
				}
				d = a[k, k];
				for (j = k + 1; j <= n - 1; j++)
				{
					//p = k*n+j;
					a[k, j] = a[k, j] / d;
				}
				b[k] = b[k] / d;
				for (i = k + 1; i <= n - 1; i++)
				{
					for (j = k + 1; j <= n - 1; j++)
					{
						p = i * n + j;
						//a[p] = a[p]-a[i*n+k]*a[k*n+j];
						a[i, j] = a[i, j] - a[i, k] * a[k, j];
					}
					//b[i] = b[i]-a[i*n+k]*b[k];
					b[i] = b[i] - a[i, k] * b[k];
				}
			}
			//d = a[(n-1)*n+n-1];
			d = a[n - 1, n - 1];
			if (Math.Abs(d) + 1.0 == 1.0)
			{
				return false;
			}
			x[n - 1] = b[n - 1] / d;
			for (i = n - 2; i >= 0; i--)
			{
				t = 0.0;
				for (j = i + 1; j <= n - 1; j++)
					//t = t+a[i*n+j]*x[j];
					t = t + a[i, j] * x[j];
				x[i] = b[i] - t;
			}
			js[n - 1] = n - 1;
			for (k = n - 1; k >= 0; k--)
				if (js[k] != k)
				{
					t = x[k]; x[k] = x[js[k]]; x[js[k]] = t;
				}

			return true;
		}
	}
}
