using System;
using Newtonsoft.Json;

namespace ConfigurationEnhanced
{
  public class ConfigWrapper<T>
  {
    public ConfigDef Definition { get; protected set; }

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
      JsonConvert.DeserializeObject<T>(Definition);
    }

    /// <summary>
    /// Set the value. Using this is not recommended, unless this value is invalid.
    /// </summary>
    public void Set()
    {

    }
  }
}
