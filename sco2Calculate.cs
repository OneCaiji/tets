using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SCO2
{
	/// <summary>
	/// 计算SCO2物性参数
	/// </summary>
	public class sco2Calculate
	{
		/// <summary>
		/// 焓
		/// </summary>
		public double enthalpy;
		/// <summary>
		/// 熵
		/// </summary>
		public double entropy;
		/// <summary>
		/// 密度
		/// </summary>
		public double density;
		double Cp, SoundSpeed;

		public const Int32 refpropfluidpathlength = (refpropcharlength + 1) * compsize; //+1 for '|' between file names
		public const Int32 refpropcharlength = 255;
		public const Int32 filepathlength = 255;
		public const Int32 lengthofreference = 3;
		public const Int32 errormessagelength = 255;
		public const Int32 compsize = 20;
		const string FluidsDirectory = "d:/setup/refprop/fluids/";
		const string MixturesDirectory = "d:/setup/refprop/mixtures/";
		const string refpropDLL_path = "d:/setup/refprop/Refprop.dll";

		[DllImport(refpropDLL_path, EntryPoint = "SETUPdll", SetLastError = true)]
		//ok public static extern void SETUPdll(ref Int32 NumberOfComponents, char[] HFILES, char[] HFMIX, char[] HRF, [In, Out] ref Int32 ierr, [MarshalAs(UnmanagedType.LPArray, SizeConst=errormessagelength)] [In, Out] char[] HERR, Int32 l1, Int32 l2, Int32 l3, Int32 l4);
#if (setup_use_string)
		public static extern void SETUPdll(ref Int32 NumberOfComponents, string HFILES, string HFMIX, string HRF, [In, Out] ref Int32 ierr,  [In, Out] char[] HERR, Int32 l1, Int32 l2, Int32 l3, Int32 l4);
#else
		public static extern void SETUPdll(ref Int32 NumberOfComponents, char[] HFILES, char[] HFMIX, char[] HRF, [In, Out] ref Int32 ierr, [In, Out] char[] HERR, Int32 l1, Int32 l2, Int32 l3, Int32 l4);
#endif

		[DllImport(refpropDLL_path, EntryPoint = "SATPdll", SetLastError = true)]
		//ok public static extern void SATPdll(ref double PkPa, double[] X, ref Int32 KPH, [In, Out] ref double TK, [In, Out] ref double RHOF, [In, Out] ref double RHOG, [Out] double[] XLIQ, [Out] double[] XVAP, [In, Out] ref Int32 ierr, [MarshalAs(UnmanagedType.LPArray, SizeConst=errormessagelength)] [In, Out] char[] HERR, Int32 LengthHERR);
		public static extern void SATPdll(ref double PkPa, double[] X, ref Int32 KPH, [In, Out] ref double TK, [In, Out] ref double RHOF, [In, Out] ref double RHOG, [Out] double[] XLIQ, [Out] double[] XVAP, [In, Out] ref Int32 ierr, [In, Out] char[] HERR, Int32 LengthHERR);

		[DllImport(refpropDLL_path, EntryPoint = "TPFLSHdll", SetLastError = true)]
		public static extern void TPFLSHdll(ref double TK, ref double PkPa, double[] X, [In, Out]ref double d, [In, Out]ref double dl, [In, Out]ref double dv, [Out]double[] XLIQ, [Out] double[] XVAP, [In, Out] ref double q, [In, Out] ref double e, [In, Out] ref double h, [In, Out] ref double s, [In, Out] ref double cv, [In, Out] ref double cp, [In, Out] ref double w, [In, Out] ref Int32 ierr, [In, Out] char[] HERR, Int32 LengthHERR);

		double[] x; //[compsize]={0};
		Int32 nc, ierr;
		char[] HFILES_ch, HRF_ch, HERR_ch, HFMIX_ch;
		string HFILES_st, HRF_st, HFMIX_st;  //HERR_st unable to return HERR as string

		public sco2Calculate()
		{
			enthalpy = 0;
			entropy = 0;
			density = 0;
			Cp = 0;
			SoundSpeed = 0;
		}

		/// <summary>
		/// 单纯计算物性参数
		/// </summary>
		/// <param name="T">温度</param>
		/// <param name="P">压力</param>
		public void TPflash(double T, double P)
		{
			// Now use the functions.

			// Refprop variables that need to be defined
			//
			// nc = Number of components in the mixture
			// x[NumberOfComponentsInMixtures] = Mole fraction of each component
			// ierr =  An integer flag defining an error
			// HFILES[] = a character array defining the fluids in a mixture
			// HRF[] = a character array denoting the reference state
			// HERR[] = a character array for storing a string - Error message
			// HFMIX[] a character array defining the path to the mixture file

			x = new double[compsize];
			HFILES_ch = new char[refpropfluidpathlength + 1];
			HRF_ch = new char[lengthofreference + 1];
			HERR_ch = new char[errormessagelength + 1];
			HFMIX_ch = new char[refpropcharlength + 1];

			nc = 1;
			x[0] = 1.0;

			// Note, the directory will likely be different on other machines
			HFILES_st = "D:\\setup\\Refprop\\Fluids\\CO2.fld";
			HFMIX_st = "D:\\setup\\Refprop\\Fluids\\HMX.BNC";
			HRF_st = "DEF";
			//overwritten by dll HERR_st="Ok";

			// Call SETUPdll to initialize global variables in Refprop

#if (setup_use_string)
			SETUPdll(ref nc, HFILES_st, HFMIX_st, HRF_st, ref ierr, HERR_ch,
				refpropfluidpathlength,refpropcharlength,
				lengthofreference,errormessagelength);
#else
			convert_string_to_charArray(HFILES_st, HFILES_ch);
			convert_string_to_charArray(HFMIX_st, HFMIX_ch);
			convert_string_to_charArray(HRF_st, HRF_ch);
			//convert_string_to_charArray(HERR_st, HERR_ch);
			SETUPdll(ref nc, HFILES_ch, HFMIX_ch, HRF_ch, ref ierr, HERR_ch,
				refpropfluidpathlength, refpropcharlength,
				lengthofreference, errormessagelength);
#endif

			if (ierr != 0)
			{
				CheckStatus(ierr, HERR_ch, "SETUPdll");
			}

			// Create variables that are to be
			// (a) Used to call the SATPdll function
			// (b) Returned by the SATPdll function

			double[] XLIQ = new double[compsize]; //={0.0};
			double[] XVAP = new double[compsize]; //={0.0};
			// Some values...


			double d = 0, dl = 0, dv = 0, q = 0, e = 0, h = 0, s = 0, cv = 0, cp = 0, w = 0;

			TPFLSHdll(ref T, ref P, x, ref d, ref dl, ref dv, XLIQ, XVAP, ref q, ref e, ref h, ref s, ref cv, ref cp, ref w, ref ierr, HERR_ch, errormessagelength);

			this.density = d * 44.01;
			this.enthalpy = h / 44.01;
			this.entropy = s / 44.01;
			this.Cp = cp / 44.01;
			this.SoundSpeed = w;

		}
		#region 隐藏
		void convert_string_to_charArray(string s, char[] c)
		{
			int i;
			if (s.Length + 1 > c.Length)
			{
				//MessageBox.Show(String.Format("string Length {0} +1 > char Length {1} ", s.Length, c.Length));
				//Application.Exit();
			}
			for (i = 0; i < s.Length; i++)
			{
				c[i] = s[i];
			}
			c[i] = '\0';
		}

		public bool CheckStatus(Int32 ierr, char[] err_msg, String RoutineName)
		{
			string text;
			bool fatal;
			text = new string(err_msg);
			fatal = true;
			return fatal;
		}
		#endregion			
		/// <summary>
		/// 由静参数和速度求解滞止参数
		/// </summary>
		/// <param name="Ts">静温</param>
		/// <param name="Ps">总温</param>
		/// <param name="V">速度</param>
		public void TFromSV(double Ts,double Ps,double V)
		{
		}		
		/// <summary>
		/// 由压力和焓值求解T
		/// </summary>
		/// <param name="P">压力</param>
		/// <param name="H">焓</param>
		/// <returns>温度</returns>
		public double TFromH(double P, double H)
		{
			double Tmax = 2000, Tmin = 100, DH = 0, Ttry = 0;
			int ijk = 0;
			do
			{
				Ttry = (Tmax + Tmin) / 2;
				DH = DeltH(Ttry, P, H);

				if (Math.Abs(DH) < H * 1E-7)
				{
					break;
				}
				if (DH > 0) Tmax = Ttry;
				if (DH < 0) Tmin = Ttry;

				ijk++;
			}
			while (ijk < 100);
			return Ttry;
		}	
		/// <summary>
		/// 从压力和熵求解T
		/// </summary>
		/// <param name="P">压力</param>
		/// <param name="S">熵</param>
		/// <returns>温度</returns>
		public double TFromS(double P, double S)
		{
			double Tmax = 1000, Tmin = 316, DS = 0, Ttry = 0;
			int ijk = 0;
			do
			{
				Ttry = (Tmax + Tmin) / 2;
				DS = DeltS(Ttry, P, S);

				if (Math.Abs(DS) < S * 1E-7)
				{
					break;
				}
				if (DS > 0) Tmax = Ttry;
				if (DS < 0) Tmin = Ttry;

				ijk++;
			}
			while (ijk < 100);
			return Ttry;
		}
		/// <summary>
		///S(T,P)与S的差
		/// </summary>
		double DeltS(double T, double P, double S)
		{
			double DS = 0;
			TPflash(T, P);
			DS = this.entropy - S;
			return DS;
		}
		/// <summary>
		///H(T,P)与H的差
		/// </summary>
		double DeltH(double T, double P, double H)
		{
			double DH = 0;
			TPflash(T, P);
			DH = this.enthalpy - H;
			return DH;
		}
		/// <summary>
		/// 返回焓值
		/// </summary>
		/// <param name="T"></param>
		/// <param name="P"></param>
		/// <returns>H</returns>
		public double ReturnH(double T, double P)
		{
			double H = 0;
			TPflash(T, P);
			H = this.enthalpy;
			return H;
		}
		/// <summary>
		/// 返回熵
		/// </summary>
		/// <param name="T"></param>
		/// <param name="P"></param>
		/// <returns>S</returns>
		public double ReturnS(double T, double P)
		{
			double S = 0;
			TPflash(T, P);
			S = this.entropy;
			return S;
		}
		/// <summary>
		/// 返回密度
		/// </summary>
		/// <param name="T"></param>
		/// <param name="P"></param>
		/// <returns>S</returns>
		public double ReturnD(double T, double P)
		{
			double d= 0;
			TPflash(T, P);
			d = this.density ;
			return d;
		}
		



	}
}
