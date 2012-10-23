/*
 * Matthew Carl Bednarski <matthew.bednarski@ekr.it>
 * 05/06/2012 - 16.47
 */

using System;
using System.Collections.Generic;
using System.IO;

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
							if(!line.StartsWith("#") && !line.StartsWith("["))
							{
								KeyValuePair<string, string> kvp = ParseIniLine(line);
								if(!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
								{
									if(!Settings.ContainsKey(kvp.Key))
									{
										Settings.Add(kvp.Key, kvp.Value);
									}else{
										Settings.Remove(kvp.Key);
										Settings.Add(kvp.Key, kvp.Value);
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
		
		public static string[] GetPair(string piece)
		{
			string[] r = null;
			var kvp = ParseIniLine(piece);
			
			if(!String.IsNullOrEmpty(kvp.Key))
			{
				string k = kvp.Key;
				string v = kvp.Value;
				r = new string[]{k, v};
			}
			return r;
		}
		private static KeyValuePair<string, string> ParseIniLine(string line)
		{
			KeyValuePair<string, string> kvp = new KeyValuePair<string, string>();
			
			string[] pieces = line.Split(new string[]{":\t"}, StringSplitOptions.RemoveEmptyEntries);
			if(pieces.Length == 2)
			{
				kvp = new KeyValuePair<string, string>(pieces[0], cleanUpValue(pieces[1]));
			}
			return kvp;
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
}
