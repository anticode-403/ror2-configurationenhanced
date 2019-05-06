using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConfigurationEnhanced
{
  public class ConfigDef
  {
    public string[] Section { get; }

    public string Key { get; }
    
    public ConfigDef(string[] section, string key)
    {
      Key = key;
      Section = section;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      if (!(obj is ConfigDef other))  return false;

      return string.Equals(Key, other.Key) && string.Equals(Section, other.Section);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = Key != null ? Key.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (Section != null ? Section.GetHashCode() : 0);
        return hashCode;
      }
    }

    public static bool operator ==(ConfigDef left, ConfigDef right) => Equals(left, right);

    public static bool operator !=(ConfigDef left, ConfigDef right) => !Equals(left, right);
  }
}
