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
  [BepInPlugin("com.anticode.Sacrifice", "Sacrifice", "1.0.0")]
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
      FileSystemWatcher watcher = new FileSystemWatcher();
      watcher.Path = Paths.ConfigPath;

      watcher.Filter = "*.json";

      watcher.Changed += OnChange;
      watcher.Created += OnChange;
      watcher.Deleted += OnDeleted;

      watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Contains a list of all registered configuration files.
    /// </summary>
    public List<ConfigFile> configList = new List<ConfigFile>();

    private static void OnChange(object source, FileSystemEventArgs args)
    {
      foreach (ConfigFile configFile in instance.configList)
      {
        if (configFile.ConfigFilePath == args.FullPath)
        {
          configFile.Load();
        }
      }
    }

    private static void OnDeleted(object source, FileSystemEventArgs args)
    {
      foreach (ConfigFile configFile in instance.configList)
      {
        if (configFile.ConfigFilePath == args.FullPath)
        {
          configFile.Save();
        }
      }
    }
  }
}
