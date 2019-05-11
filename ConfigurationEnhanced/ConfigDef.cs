using System.Text.RegularExpressions;

namespace ConfigurationEnhanced
{
  public class ConfigDef
  {
    public string Section { get; }

    public string Key { get; }

    public string Description { get; }

    [System.Serializable]
    public class InvalidKeyException : System.Exception
    {
      public InvalidKeyException() { }
      public InvalidKeyException(string message) : base(message) { }
      public InvalidKeyException(string message, System.Exception inner) : base(message, inner) { }
      protected InvalidKeyException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public ConfigDef(string section, string key, string description = "")
    {
      if (!ConfigFile.sanitizeKeyRegex.IsMatch(key))
        throw new InvalidKeyException($"The key '{key}' is not valid. Please enter a valid key.");
      Key = Regex.Escape(key);
      Section = section;
      Description = description;
    }

    public override bool Equals(object obj)
    {
      if (obj is null) return false;
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
