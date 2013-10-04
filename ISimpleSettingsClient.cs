/*
 * Created by SharpDevelop.
 * User: ekr
 * Date: 29/08/2013
 * Time: 12:03
 * 
 */
using System;
using System.Collections.Generic;

namespace Simple.Settings
{
	/// <summary>
	/// Description of ISimpleSettingsClient.
	/// </summary>
	public interface ISimpleSettingsClient
	{
		SimpleSettingsReader SettingsReader{get;}
		IDictionary<String, String> Defaults {get; }
//		void SetDefaults();
		bool PrintDefaultIni(String out_path);
	}
}
