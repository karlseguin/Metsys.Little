namespace Metsys.Little
{
   public interface IGlobalConfiguration
   {
      IGlobalConfiguration DateTime(DateTimeMode mode);
   }

   public class GlobalConfiguration : IGlobalConfiguration
   {
      internal DateTimeMode DateTimeMode{ get;set;}

      internal GlobalConfiguration()
      {
         DateTimeMode = DateTimeMode.SecondPrecision;
      }

      public IGlobalConfiguration DateTime(DateTimeMode mode)
      {
         DateTimeMode = mode;
         return this;
      }
   }
}