using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationEnhanced
{
  public class ConfigWrapper<T>
  {
    /// <summary>
    /// The definition object for this setting. Read Only.
    /// </summary>
    public ConfigDef Definition { get; protected set; }

    /// <summary>
    /// The file this setting is being modified in. Read Only.
    /// </summary>
    public ConfigFile ConfigFile { get; protected set; }

    /// <summary>
    /// When this setting is changed by any means, this is called. Other settings do not call this.
    /// </summary>
    public EventHandler SettingsChanged;

    /// <summary>
    /// Read finds the setting and returns the result in real time.
    /// You should NOT be performing this action in-script.
    /// </summary>
    /// <returns>The value of this configuration setting</returns>
    public T Read()
    {
      ConfigFile.Load();
      return ConfigParser.FromJSON<T>(ConfigFile.Cache[Definition]);
    }
    /// <summary>
    /// Set the value. Using this is not recommended, unless this value is invalid or has been set by the user.
    /// </summary>
    public void Write(T value)
    {
      ConfigFile.Cache[Definition] = ConfigWriter.ToJSON(value);
      ConfigFile.Save();
      SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public ConfigWrapper(ConfigFile configFile, ConfigDef configDef)
    {
      ConfigFile = configFile;
      Definition = configDef;
    }
  }
}
