using System;
using Newtonsoft.Json.Linq;
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
      return ConfigFile.Cache[Definition].ToObject<T>();
    }

    /// <summary>
    /// Your config wrap as a List type. If this setting is not meant to be a List, it will create a list with a single entry.
    /// </summary>
    /// <returns>A List of objects of type T</returns>
    public List<T> ListRead()
    {
      JToken value = ConfigFile.Cache[Definition];
      if (Definition.ListValue)
      {
        IList<JToken> results = value.Children().ToList();
        List<T> finalResult = new List<T>();
        foreach (JToken result in results)
        {
          T listItem = result.ToObject<T>();
          finalResult.Add(listItem);
        }
        return finalResult;
      }
      else
      {
        return new List<T> { value.ToObject<T>() };
      }
    }
    /// <summary>
    /// Set the value. Using this is not recommended, unless this value is invalid or has been set by the user.
    /// </summary>
    public void Write(T value)
    {
      ConfigFile.Cache[Definition] = JToken.FromObject(value);
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
