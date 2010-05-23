using System;
using System.Collections.Generic;

namespace Metsys.Little
{
   public class LittleConfiguration
   {
      private readonly IDictionary<Type, HashSet<string>> _ignored = new Dictionary<Type, HashSet<string>>();

      private static LittleConfiguration _instance;
      internal static LittleConfiguration Instance
      {
         get
         {
            if (_instance == null) { _instance = new LittleConfiguration(); }
            return _instance;
         }
      }

      private LittleConfiguration(){}

      public static void ForType<T>(Action<ITypeConfiguration<T>> action)
      {
         action(new TypeConfiguration<T>(Instance));
      }

      public void AddIgnore<T>(string name)
      {
         var type = typeof(T);
         if (!_ignored.ContainsKey(type))
         {
            _ignored[type] = new HashSet<string>();
         }
         _ignored[type].Add(name);
      }
      public bool IsIgnored(Type type, string name)
      {
         HashSet<string> list;
         return _ignored.TryGetValue(type, out list) && list.Contains(name);
      }
   }
}
