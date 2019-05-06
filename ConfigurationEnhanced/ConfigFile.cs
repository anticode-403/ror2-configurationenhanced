using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigurationEnhanced
{
  /// <summary>
  /// A helper class designed to handle persistent data.
  /// </summary>
  public class ConfigFile
  {
    private static readonly Regex sanitizeKeyRegex = new Regex(@"[^a-z0-9\-\.]+", RegexOptions.IgnoreCase);

    internal static ConfigFile CoreConfig { get; } = new ConfigFile(BepInEx.Paths.BepInExConfigPath, true);

    protected Dictionary<ConfigDef, string> Cache => new Dictionary<ConfigDef, string>();

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

    private object _ioLock = new object();

    public void Reload()
    {
      
    }

    public void Save()
    {

    }
  }
}
