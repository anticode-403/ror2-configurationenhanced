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

    internal static ConfigFile CoreConfig { get; } = new ConfigFile(Paths.BepInExConfigPath, true);

    protected internal Dictionary<ConfigDef, JToken> Cache => new Dictionary<ConfigDef, JToken>();

    public IReadOnlyCollection<ConfigDef> ConfigDefs => Cache.Keys.ToList().AsReadOnly();

    /// <summary>
    /// An event that is fired every time this config is reloaded.
    /// </summary>
    public event EventHandler ConfigReloaded;

    public string ConfigFilePath { get; }
    
    public ConfigFile(string configPath, bool saveOnInit)
    {
      ConfigFilePath = configPath;
      if (File.Exists(ConfigFilePath))
      {
        Reload();
      }
      else if (saveOnInit)
      {
        Save();
      }
    }

    public void Reload()
    {
      if (!Directory.Exists(Paths.ConfigPath)) Directory.CreateDirectory(Paths.ConfigPath);
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
    }

    public void Save()
    {
      if (!Directory.Exists(Paths.ConfigPath)) Directory.CreateDirectory(Paths.ConfigPath);
      JsonSerializer jsonSerializer = JsonSerializer.Create();
      using (StreamWriter sw = new StreamWriter(Paths.ConfigPath))
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
        Cache.Add(configDef, null);
      }
      return new ConfigWrapper<T>(this, configDef);
    }

    public ConfigWrapper<T> Wrap<T>(string[] section, string key, T defaultValue = default(T))
      => Wrap<T>(new ConfigDef(section, key));
  }
}
