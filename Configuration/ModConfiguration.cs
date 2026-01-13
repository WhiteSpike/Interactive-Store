using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace InteractiveStore.Configuration
{
	public class ModConfiguration
	{
		public ConfigEntry<string> ItemStoreCommandList { get; set; }
		public ConfigEntry<bool> ItemStoreCommandCaseSensitive { get; set; }
		public ConfigEntry<string> DecorationStoreCommandList { get; set; }
		public ConfigEntry<bool> DecorationStoreCommandCaseSensitive { get; set; }
		public ModConfiguration(ConfigFile config)
		{
			string sectionName = "Initialization";
			ItemStoreCommandList = config.Bind(sectionName, "Command Prompts for Item Store", "istore", "List of commands separated by a comma (') to access the interactive item store");
			ItemStoreCommandCaseSensitive = config.Bind(sectionName, "Item Store Prompt Case Sensitivity", false, "Wether the letter case of the command prompts should be relevant or not to bring up the application");
			DecorationStoreCommandList = config.Bind(sectionName, "Command Prompts for Decoration/Ship Upgrade Store", "idecor", "List of commands separated by a comma (') to access the interactive decoration/upgrade store");
			DecorationStoreCommandCaseSensitive = config.Bind(sectionName, "Decoration Store Prompt Case Sensitivity", false, "Wether the letter case of the command prompts should be relevant or not to bring up the application");
		}
	}
}
