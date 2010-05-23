using System;
using System.Linq.Expressions;

namespace Metsys.Little
{
   public interface ITypeConfiguration<T>
   {
      ITypeConfiguration<T> Ignore(Expression<Func<T, object>> expression);
   }

   internal class TypeConfiguration<T> : ITypeConfiguration<T>
   {
      private readonly LittleConfiguration _configuration;

      internal TypeConfiguration(LittleConfiguration configuration)
      {
         _configuration = configuration;
      }

      public ITypeConfiguration<T> Ignore(Expression<Func<T, object>> expression)
      {
         var member = expression.GetMemberExpression();
         _configuration.AddIgnore<T>(member.GetName());
         return this;
      }
   }
}