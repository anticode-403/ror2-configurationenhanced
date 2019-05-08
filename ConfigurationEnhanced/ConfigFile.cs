using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BepInEx;

namespace ConfigurationEnhanced
{
  /// <summary>
  /// A helper class designed to handle persistent data.
  /// </summary>
  public class ConfigFile
  {
    private static readonly Regex sanitizeKeyRegex = new Regex(@"[^a-z0-9\-\.]+", RegexOptions.IgnoreCase);

    protected internal Dictionary<ConfigDef, JToken> Cache => new Dictionary<ConfigDef, JToken>();

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
      JObject config = JObject.Parse(File.ReadAllText(Paths.ConfigPath));
      foreach (ConfigDef configDef in Cache.Keys)
      {
        JToken token = config;
        foreach (string section in configDef.Section)
        {
          token = token[section];
        }
        JToken result = token.SelectToken(configDef.Key);
        Cache[configDef] = result;
      }
      ConfigReloaded.Invoke(this, null);
    }

    /// <summary>
    /// Saves config file to disk. This will call reload.
    /// </summary>
    public void Save()
    {
      if (!Directory.Exists(Paths.ConfigPath)) Directory.CreateDirectory(Paths.ConfigPath);
      JsonSerializer jsonSerializer = JsonSerializer.Create();
      using (StreamWriter sw = new StreamWriter(File.Create(ConfigFilePath)))
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        dynamic fileData;
        fileData = new ExpandoObject();
        foreach (ConfigDef configDef in Cache.Keys)
        {
          fileData[configDef.Section[0]] = ModifySetting(fileData[configDef.Section[0]], configDef.Section.Skip(1).ToArray(), Cache[configDef]);
        }
        jsonSerializer.Serialize(writer, fileData);
      }
    }

    private dynamic ModifySetting(dynamic section, string[] path, JToken value)
    {
      if (path.Length == 1)
      {
        dynamic new_section = section;
        new_section[path[0]] = value;
        return new_section;
      }
      else
      {
        dynamic new_section = section;
        new_section[path[0]] = ModifySetting(new_section, path.Skip(1).ToArray(), value);
        return new_section;
      }
    }

    public ConfigWrapper<T> Wrap<T>(ConfigDef configDef, T defaultValue = default(T))
    {
      if (!Cache.ContainsKey(configDef))
      {
        Cache.Add(configDef, JToken.FromObject(defaultValue));
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
    public ConfigWrapper<T> Wrap<T>(string[] section, string key, string description, T defaultValue = default(T))
      => Wrap<T>(new ConfigDef(section, key, description), defaultValue);
  }
}
