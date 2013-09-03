/*
 * Matthew Carl Bednarski <matthew.bednarski@ekr.it>
 * 05/06/2012 - 16.47
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace Simple.Settings
{
	/// <summary>
	/// Description of SimpleSettingsReader.
	/// </summary>
	public class SimpleSettingsReader
	{
		public const String MULTI_DELIM = "|__;__|";
		private String _settingsINI;
		public String SettingsINI
		{
			get{
				if(String.IsNullOrEmpty(_settingsINI))
				{
					string dir = this.GetType().Assembly.Location;
					dir = Path.GetDirectoryName(dir);
					_settingsINI = Path.Combine(dir, "settings.ini");
				}
				return _settingsINI;
			}
			set{
				if(!String.IsNullOrEmpty(value) && File.Exists(value))
				{
					this.Settings.Clear();
					_settingsINI = Path.GetFullPath(value);
				}
				else {
					Console.WriteLine(value + " not exists..");
				}
			}
		}
		private bool inited;
		public bool HasSettings
		{
			get{
				inited = this.Settings != null && this.Settings.Count > 0;
				if(!inited)
				{
					this.LoadSettings();
				}
				return this.Settings != null && this.Settings.Count > 0;
			}
		}
		private Dictionary<string, string> _settings;
		public Dictionary<string, string> Settings
		{
			get{
				if(_settings == null)
				{
					_settings = new Dictionary<string, string>();
				}
				return _settings;
			}
		}
		public String this [string index]
		{
			get{
				if(Settings.ContainsKey(index))
				{
					return Settings[index];
				}else{
					return "";
				}
			}
		}
		public List<String> this[string index, string delim]
		{
			get{
				String t = this[index];
				if(!String.IsNullOrEmpty(t))
				{
					List<String> r = new List<string>();
					if(t.Contains(delim))
					{
						r.AddRange(t.Split(new string[]{ delim }, StringSplitOptions.RemoveEmptyEntries));
					}else{
						r.Add(t);
					}
					return r;
				}else{
					return new List<String>();
				}
			}
		}
		public SimpleSettingsReader()
		{
			this.LoadSettings();
		}
		public void LoadSettings()
		{
			try{
				if(File.Exists(SettingsINI))
				{
					this._settings = new Dictionary<string, string>();
					string contents = "";
					FileInfo fi = new FileInfo(this.SettingsINI);
					using(StreamReader sr = fi.OpenText())
					{
						contents = sr.ReadToEnd();
					}
					if(!string.IsNullOrEmpty(contents))
					{
						string[] lines = contents.Split(new string[]{System.Environment.NewLine}, StringSplitOptions.None);
						foreach(string line in lines)
						{
							if(line.IsSettingLine())
							{
								SettingRow sr = line.ParseIniLine();
								KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(sr.Setting, sr.Value);
								if(!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
								{
									if(!Settings.ContainsKey(kvp.Key))
									{
										Settings.Add(kvp.Key, kvp.Value);
									}else{
										Settings[kvp.Key] += MULTI_DELIM + kvp.Value;
									}
								}
							}
						}
					}
				}
			}catch(Exception ex)
			{
				
			}
		}
		public bool SaveSettings()
		{
			List<String> lines = new List<string>();
			using(StreamReader sr = File.OpenText(this.SettingsINI))
			{
				while(sr.Peek() >= 0)
				{
					lines.Add(sr.ReadLine());
				}
			}
			IList<SettingRow> settingLines = new List<SettingRow>();
			foreach(string line in lines)
			{
				if(line.IsSettingLine())
				{
					SettingRow sr =line.ParseIniLine();
					sr.Row = lines.IndexOf(line);
					settingLines.Add( sr );
				}
			}
//			var settingLines = lines.Where(xx => xx.IsSettingLine())
//				.Select(xx =>
//				        {
//				        	SettingRow sr =xx.ParseIniLine();
//				        	sr.Row = lines.IndexOf(xx);
//				        	return sr;
//				        });
			foreach(SettingRow sr in settingLines)
			{
				if(this.Settings.ContainsKey(sr.Setting))
				{
					if(!this[sr.Setting].Equals(sr.Value))
					{
						sr.Value = this[sr.Setting];
						lines[sr.Row] = sr.CreateIniLine();
					}
				}
			}
			
			IList<string> settingKeys = new List<string>();
			foreach(SettingRow sr in settingLines)
			{
				settingKeys.Add(sr.Setting);
			}
//			var settingKeys = settingLines.Select(xx => xx.Setting);
			foreach(KeyValuePair<string, string> setting in this.Settings)
			{
				if(!settingKeys.Contains(setting.Key))
				{
					lines.Add(setting.CreateIniLine());
				}
			}
			
			bool r = false;
			using(FileStream fs = File.Create(this.SettingsINI))
			{
				using(StreamWriter sw = new StreamWriter(fs))
				{
					foreach(String line in lines)
					{
						sw.WriteLine(line);
					}
					r = true;
				}
			}
			return r;
		}
		public bool GetBooleanSetting(string key)
		{
			return this.GetBooleanSetting(key, false);
		}
		public bool GetBooleanSetting(string key, bool default_value)
		{
			bool r = default_value;
			if(!string.IsNullOrEmpty(this[key]))
			{
				string val = this[key];
				r = Convert.ToBoolean(val);
				if(r == false)
				{
					if(val.ToLower().Trim().Equals("1"))
					{
						r = true;
					}else if(val.ToLower().Trim().Equals("t"))
					{
						r = true;
					}
				}
					
			}
			return r;
		}
		public int GetNumericSetting(string key)
		{
			return this.GetNumericSetting(key, -1);
		}
		public int GetNumericSetting(string key, int default_value)
		{
			int r = default_value;
			if(!string.IsNullOrEmpty(this[key]))
			{
				string val = this[key];
				int tmp = Convert.ToInt32(val);
				if(tmp != 0)
				{
					r  = tmp;
				}
			}
			return r;
		}
		
		public void AddSettings(IDictionary<string, string> settings)
		{
			foreach(KeyValuePair<string, string> setting in settings)
			{
				if(this.Settings.ContainsKey(setting.Key))
				{
					this.Settings[setting.Key] = setting.Value;
				}else{
					this.Settings.Add(setting.Key, setting.Value);
				}
			}
		}
		public static List<string> ListSettingParser(string toParse)
		{
			List<string> r = new List<string>();
			
			string listItemBegin = "{";
			string listItemEnd = "}";
			
			int pieces = CountOccurences(toParse, listItemBegin);
			
			string restOfString = toParse;
			for(int i = 0; i < pieces; i++)
			{
				restOfString = GetContents(listItemBegin, listItemEnd, restOfString, r);
			}
			
			return r;
		}
		private static string GetContents(string ctStart, string ctEnd, string toParse, List<string> items)
		{
			string tmp = toParse;
			string ctContents = "";
			int start = toParse.IndexOf(ctStart) + ctStart.Length;
			if(toParse.Length > start)
			{
				string tmp2 = toParse.Substring(start);
				int end = tmp2.IndexOf(ctEnd);
				if(end > -1)
				{
					tmp = tmp2.Substring(0, end);
					items.Add(tmp);
					tmp = tmp2.Substring(end + ctEnd.Length);
				}
			}
			return tmp;
		}
		private static int CountOccurences(string haystack, string needle)
		{
			return (haystack.Length - haystack.Replace(needle,"").Length) / needle.Length;
		}
		
	}

	class SettingRow
	{
		public int Row {get; set;}
		public String Setting {get; set;}

		public String Value
		{
			get;
			set;
		}
		public String Text
		{
			get{
				return this.Setting + ":\t" + this.Value;
			}
		}
	}
	static class SettingsExtensions
	{
		public static bool IsSettingLine(this string line)
		{
			bool r = false;
			if(line != null && !line.StartsWith("#") && !line.StartsWith("[") && !String.IsNullOrEmpty(line.Trim()))
			{
				if(line.Contains(":"))
				{
					r = true;
				}
			}
			return r;
		}
		public static string CreateIniLine(this KeyValuePair<string, string> kvp)
		{
			return kvp.Key + ":\t" + kvp.Value;
		}
		public static string CreateIniLine(this SettingRow setting)
		{
			return setting.Setting + ":\t" + setting.Value;
		}
		public static SettingRow ParseIniLine(this string line)
		{
			SettingRow sr = new SettingRow();
			
			string[] pieces = line.Split(new string[]{":"}, 2, StringSplitOptions.RemoveEmptyEntries);
			if(pieces.Length == 2)
			{
				sr.Setting = pieces[0];
				sr.Value = cleanUpValue(pieces[1]);
			}
			return sr;
		}
		private static string cleanUpValue(string toClean)
		{
			char[] toRemove = new char[]{'"', '\'', '\t', '\r', '\n', ' '};
			toClean = toClean.Trim();
			toClean = toClean.TrimStart(toRemove);
			toClean = toClean.TrimEnd(toRemove);
			toClean = toClean.TrimStart(toRemove);
			toClean = toClean.TrimEnd(toRemove);
			return toClean;
		}
	}
}
