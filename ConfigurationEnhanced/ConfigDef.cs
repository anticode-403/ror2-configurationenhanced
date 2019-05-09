namespace ConfigurationEnhanced
{
  public class ConfigDef
  {
    public string[] Section { get; }

    public string Key { get; }

    public string Description { get; }

    public bool ListValue { get; }
    
    public ConfigDef(string[] section, string key, string description, bool list = false)
    {
      Key = key;
      Section = section;
      Description = description;
      ListValue = list;
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
