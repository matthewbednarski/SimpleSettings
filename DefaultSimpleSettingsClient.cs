/*
 * Created by SharpDevelop.
 * User: ekr
 * Date: 29/08/2013
 * Time: 12:45
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace Simple.Settings
{
	/// <summary>
	/// Description of DefaultSimpleSettingsClient.
	/// </summary>
	public class DefaultSimpleSettingsClient: ISimpleSettingsClient
	{
		private static readonly string SETTING_NAME = "name";
		
		private Dictionary<string, string> _defaults;
		public IDictionary<string, string> Defaults {
			get {
				if(_defaults == null)
				{
					_defaults = new Dictionary<string, string>();
				}
				return _defaults;
			}
		}
		SimpleSettingsReader iniReader;
		public SimpleSettingsReader SettingsReader {
			get {
				if(iniReader == null)
				{
					iniReader = new SimpleSettingsReader();
				}
				return iniReader;
			}
		}
		public void SetDefaults()
		{
			//this.Defaults.Add(SETTING_NAME, "an example setting \"name\"");
			
			
		}
		public bool PrintDefaultIni(string out_path)
		{
			bool r = false;
			try{
				using(FileStream fs = new FileStream(out_path, FileMode.OpenOrCreate))
				{
					
				}
				SimpleSettingsReader sr = new SimpleSettingsReader();
				sr.SettingsINI = out_path;
				sr.AddSettings(this.Defaults);
				r = sr.SaveSettings();
				sr.Settings.Clear();
				
				sr = null;
			}catch(Exception ex)
			{
				
			}
			return r;
		}
		public DefaultSimpleSettingsClient()
		{
			SetDefaults();
		}
	}
}
