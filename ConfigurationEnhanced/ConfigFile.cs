using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using BepInEx;

namespace ConfigurationEnhanced
{
  /// <summary>
  /// A helper class designed to handle persistent data.
  /// </summary>
  public class ConfigFile
  {
    protected internal static readonly Regex sanitizeKeyRegex = new Regex(@"[^.']*", RegexOptions.IgnoreCase);

    protected internal Dictionary<ConfigDef, string> Cache => new Dictionary<ConfigDef, string>();

    public IReadOnlyCollection<ConfigDef> ConfigDefs => Cache.Keys.ToList().AsReadOnly();

    /// <summary>
    /// An event that is fired every time this config is reloaded.
    /// </summary>
    public event EventHandler ConfigReloaded;

    public string ConfigFilePath { get; }
    
    /// <summary>
    /// Creating this object makes a ConfigFile you can use anywhere in your mod. To make a setting, do ConfigFile.Wrap();
    /// </summary>
    /// <param name="configPath">The name of your file, without a file extension</param>
    /// <param name="saveOnInit"></param>
    public ConfigFile(string fileName, bool saveOnInit)
    {
      ConfigFilePath = Utility.CombinePaths(Paths.ConfigPath, fileName + ".json");
      if (File.Exists(ConfigFilePath))
      {
        Load();
      }
      else if (saveOnInit)
      {
        Save();
      }
    }

    /// <summary>
    /// Reloads config file from disk.
    /// </summary>
    public void Load()
    {
      if (!Directory.Exists(Paths.ConfigPath))
        Directory.CreateDirectory(Paths.ConfigPath);
      string line = "";
      foreach (string rawLine in File.ReadAllLines(ConfigFilePath))
      {
        line += rawLine.Trim();
      }
      Dictionary<string, string> dict = ConfigParser.FromJSON<Dictionary<string, string>>(line);
      foreach (string key in dict.Keys)
      {
        string[] splitKey = key.Split('.');
        string newKey = splitKey.Last();
        string section = "";
        bool isFirst = true;
        foreach (string subKey in splitKey)
        {
          if (isFirst)
          {
            isFirst = false;
            section += subKey;
          }
          else
          {
            section += $".{subKey}";
          }
        }
        ConfigDef configDef = new ConfigDef(section, newKey);
        Cache[configDef] = dict[key];
      }
      ConfigReloaded.Invoke(this, null);
    }

    /// <summary>
    /// Saves config file to disk. This will call reload.
    /// </summary>
    public void Save()
    {
      if (!Directory.Exists(Paths.ConfigPath))
        Directory.CreateDirectory(Paths.ConfigPath);
      Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
      foreach (ConfigDef configDef in Cache.Keys)
      {
        if (dict[configDef.Section] == null)
        {
          dict[configDef.Section] = new Dictionary<string, string>();
        }
        dict[configDef.Section].Add(configDef.Key, Cache[configDef]);
      }
      string json = ConfigWriter.ToJSON(dict);
      using (StreamWriter writer = new StreamWriter(File.Create(ConfigFilePath), System.Text.Encoding.UTF8))
      {
        writer.WriteLine(json);
      }
    }

    public ConfigWrapper<T> Wrap<T>(ConfigDef configDef, T defaultValue = default)
    {
      if (!Cache.ContainsKey(configDef))
      {
        Cache.Add(configDef, ConfigWriter.ToJSON(defaultValue));
        Save();
      }
      return new ConfigWrapper<T>(this, configDef);
    }

    public ConfigWrapper<T> Wrap<T>(ConfigDef configDef, List<T> defaultValue = default)
    {
      if (!Cache.ContainsKey(configDef))
      {
        Cache.Add(configDef, ConfigWriter.ToJSON(defaultValue));
        Save();
      }
      return new ConfigWrapper<T>(this, configDef);
    }

    /// <summary>
    /// Create a wrap for ConfigurationEnhanced to create a setting out of.
    /// </summary>
    /// <typeparam name="T">Type of the config value.</typeparam>
    /// <param name="section">An array of sections to drill through. foo.bar.baz: value would be ["foo", "bar"]</param>
    /// <param name="key">Key of the value. This key should be unique to this section.</param>
    /// <param name="description">Description of your setting. This displays in the in-game settings menu.</param>
    /// <param name="defaultValue">The default value of this setting if none is set.</param>
    /// <returns></returns>
    public ConfigWrapper<T> Wrap<T>(string section, string key, string description, T defaultValue = default)
      => Wrap(new ConfigDef(section, key, description), defaultValue);
  }
}
