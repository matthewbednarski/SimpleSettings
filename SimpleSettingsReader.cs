/*
 * Matthew Carl Bednarski <matthew.bednarski@ekr.it>
 * 05/06/2012 - 16.47
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace EKR.Simple.Settings
{
	/// <summary>
	/// Description of SimpleSettingsReader.
	/// </summary>
	public class SimpleSettingsReader
	{
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
										Settings[kvp.Key] += ";" + kvp.Value;
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
			
			//TODO DONE reopen file
			//TODO DONE load text lines
			List<String> lines = new List<string>();
			using(StreamReader sr = File.OpenText(this.SettingsINI))
			{
				while(sr.Peek() >= 0)
				{
					lines.Add(sr.ReadLine());
				}
			}
			
			//TODO DONE find lines with settings already existing
			var settingLines = lines.Where(xx => xx.IsSettingLine())
				.Select(xx =>
				        {
				        	SettingRow sr =xx.ParseIniLine();
				        	sr.Row = lines.IndexOf(xx);
				        	return sr;
				        });
			//TODO DONE update already existing settings
			foreach(SettingRow sr in settingLines)
			{
				if(this.Settings.ContainsKey(sr.Setting))
				{
					if(!this[sr.Setting].Equals(sr.Value))
					{
						sr.Value = this[sr.Setting];
						//TODO DONE update List<string> lines with new values;
						lines[sr.Row] = sr.CreateIniLine();
					}
				}
			}
			
			
			//TODO DONE add settings previously not existing
			var settingKeys = settingLines.Select(xx => xx.Setting);
			foreach(KeyValuePair<string, string> setting in this.Settings)
			{
				if(!settingKeys.Contains(setting.Key))
				{
					lines.Add(setting.CreateIniLine());
				}
			}
			
			//TODO DONE re-write file
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
//		public static string[] GetPair(string piece)
//		{
//			string[] r = null;
//			var kvp = ParseIniLine(piece);
//
//			if(!String.IsNullOrEmpty(kvp.Key))
//			{
//				string k = kvp.Key;
//				string v = kvp.Value;
//				r = new string[]{k, v};
//			}
//			return r;
//		}
//		private static KeyValuePair<string, string> ParseIniLine(string line)
//		{
//			KeyValuePair<string, string> kvp = new KeyValuePair<string, string>();
//
//			string[] pieces = line.Split(new string[]{":\t"}, 2, StringSplitOptions.RemoveEmptyEntries);
//			if(pieces.Length == 2)
//			{
//				kvp = new KeyValuePair<string, string>(pieces[0], cleanUpValue(pieces[1]));
//			}
//			return kvp;
//		}
//		private static string cleanUpValue(string toClean)
//		{
//			char[] toRemove = new char[]{'"', '\'', '\t', '\r', '\n', ' '};
//			toClean = toClean.Trim();
//			toClean = toClean.TrimStart(toRemove);
//			toClean = toClean.TrimEnd(toRemove);
//			toClean = toClean.TrimStart(toRemove);
//			toClean = toClean.TrimEnd(toRemove);
//			return toClean;
//		}
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
				if(line.Contains(":\t"))
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
