using BepInEx;
using BepInEx.Logging;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace ConfigurationEnhanced
{
  [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
  [BepInPlugin("com.anticode.ConfigurationEnhanced", "ConfigurationEnhanced", "1.0.0")]
  class Config : BaseUnityPlugin
  {
    public static Config instance;

    public void Awake()
    {
      if (instance == null)
      {
        instance = this;
      }
      else if (instance != this)
      {
        Destroy(this);
      }
    }

    /// <summary>
    /// Contains a list of all registered configuration files.
    /// </summary>
    public List<ConfigFile> configList = new List<ConfigFile>();
  }
}
