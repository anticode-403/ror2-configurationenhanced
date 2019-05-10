﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ConfigurationEnhanced
{
  static class ConfigWriter
  {
    public static string ToJSON(this object item)
    {
      StringBuilder stringBuilder = new StringBuilder();
      AppendValue(stringBuilder, item);
      return stringBuilder.ToString();
    }

    static void AppendValue(StringBuilder stringBuilder, object item)
    {
      if (item == null)
      {
        stringBuilder.Append("null");
        return;
      }

      Type type = item.GetType();
      if (type == typeof(string))
      {
        stringBuilder.Append('"');
        string str = (string)item;
        for (int i = 0; i < str.Length; ++i)
        {
          if (str[i] < ' ' || str[i] == '"' || str[i] == '\\')
          {
            stringBuilder.Append('\\');
            int j = "\"\\\n\r\t\b\f".IndexOf(str[i]);
            if (j >= 0)
              stringBuilder.Append("\"\\nrtbf"[j]);
            else
              stringBuilder.AppendFormat("u{0:X$}", (UInt32)str[i]);
          }
          else
            stringBuilder.Append(str[i]);
        }
        stringBuilder.Append('"');
      }
      else if (type == typeof(byte) || type == typeof(int))
      {
        stringBuilder.Append(item.ToString());
      }
      else if (type == typeof(float))
      {
        stringBuilder.Append(((float)item).ToString(System.Globalization.CultureInfo.InvariantCulture));
      }
      else if (type == typeof(double))
      {
        stringBuilder.Append(((double)item).ToString(System.Globalization.CultureInfo.InvariantCulture));
      }
      else if (type == typeof(bool))
      {
        stringBuilder.Append(((bool)item) ? "true" : "false");
      }
      else if (type.IsEnum)
      {
        stringBuilder.Append('"');
        stringBuilder.Append(item.ToString());
        stringBuilder.Append('"');
      }
      else if (item is IList)
      {
        stringBuilder.Append('[');
        bool isFirst = true;
        IList list = item as IList;
        for (int i = 0; i < list.Count; i++)
        {
          if (isFirst)
            isFirst = false;
          else
            stringBuilder.Append(',');
          AppendValue(stringBuilder, list[i]);
        }
        stringBuilder.Append(']');
      }
      else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
      {
        Type keyType = type.GetGenericArguments()[0];
        if (keyType != typeof(string))
        {
          stringBuilder.Append("{}");
          return;
        }

        stringBuilder.Append('{');
        IDictionary dict = item as IDictionary;
        bool isFirst = true;
        foreach (object key in dict.Keys)
        {
          if (isFirst)
            isFirst = false;
          else
            stringBuilder.Append(',');
          stringBuilder.Append('\"');
          stringBuilder.Append((string)key);
          stringBuilder.Append("\":");
          AppendValue(stringBuilder, dict[key]);
        }
        stringBuilder.Append('}');
      }
      else
      {
        stringBuilder.Append('{');

        bool isFirst = true;
        BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        FieldInfo[] fieldInfos = type.GetFields(bindFlags);
        for (int i = 0; i < fieldInfos.Length; i++)
        {
          if (fieldInfos[i].IsDefined(typeof(IgnoreDataMemberAttribute), true))
            continue;
          object value = fieldInfos[i].GetValue(item);
          if (value != null)
          {
            if (isFirst)
              isFirst = false;
            else
              stringBuilder.Append(',');
            stringBuilder.Append('\"');
            stringBuilder.Append(GetMemberName(fieldInfos[i]));
            stringBuilder.Append("\":");
            AppendValue(stringBuilder, value);
          }
        }
        PropertyInfo[] propertyInfo = type.GetProperties(bindFlags);
        for (int i = 0; i < propertyInfo.Length; i++)
        {
          if (!propertyInfo[i].CanRead || propertyInfo[i].IsDefined(typeof(IgnoreDataMemberAttribute), true))
            continue;
          object value = propertyInfo[i].GetValue(item, null);
          if (value != null)
          {
            if (isFirst)
              isFirst = false;
            else
              stringBuilder.Append(',');
            stringBuilder.Append('\"');
            stringBuilder.Append(GetMemberName(propertyInfo[i]));
            stringBuilder.Append("\":");
            AppendValue(stringBuilder, value);
          }
        }
        stringBuilder.Append('}');
      }
    }

    static string GetMemberName(MemberInfo member)
    {
      if (member.IsDefined(typeof(DataMemberAttribute), true))
      {
        DataMemberAttribute dataMemberAttribute = 
          (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
        if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
          return dataMemberAttribute.Name;
      }

      return member.Name;
    }
  }
}
