using System;
using System.Collections;

namespace Metsys.Little
{
   internal static class Helper
   {
      public static readonly DateTime Epoch = new DateTime(1970, 1, 1);

      public static bool IsAmbiguous(Type type)
      {
         return (type.IsAbstract || type.IsInterface) && !typeof (IEnumerable).IsAssignableFrom(type);
      }
   }
}