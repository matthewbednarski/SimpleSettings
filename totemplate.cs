
using System;
using EKR.Cmd.Utils;
using EKR.Simple.Settings;

namespace EKRPE.Giesse.Table.Sync
{
	class Program
	{
		private static SimpleSettingsReader _s;
		private static SimpleSettingsReader Setts
		{
			get{
				if(_s == null)
				{
					_s = new EKR.Simple.Settings.SimpleSettingsReader();
					
				}
				return _s;
			}
		}
		
		public static void Main(string[] args)
		{
			Args a = new Args(args);
			
		}
	}
}