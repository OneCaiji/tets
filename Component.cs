using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SCO2
{
	/// <summary>
	/// 发动机部件根类
	/// 包含发动机名称，端口数，部件顺序，所含机械和气路哈希表
	/// </summary>
	public abstract class Component
	{
		/// <summary>
		/// // 部件所在发动机 
		/// </summary>
		public Engine Engine;

		/// <summary>
		/// // 部件所含端口数
		/// </summary>
		public uint PortNumber;
		/// <summary>
		/// // 部件序列
		/// </summary>
		public uint ComponentNo;
		/// <summary>
		/// // 关键字气路端口映射
		/// </summary>
		public Hashtable KeyToGasFlux;
		/// <summary>
		/// // 关键字机械端口映射
		/// </summary>
		public Hashtable KeyToMechPort;

		public Component()
		{
			KeyToGasFlux = new Hashtable();
			KeyToMechPort = new Hashtable();

		}

		public abstract void Run(int I);

	}
}
