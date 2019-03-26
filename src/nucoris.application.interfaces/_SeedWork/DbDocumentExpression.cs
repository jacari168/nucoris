using System;
using System.Linq.Expressions;

namespace nucoris.application.interfaces
{
    /// <summary>
    /// This class is a thin wrapper over a Linq expression.
    /// It's only goal is to simplify a bit the specification of "where" Linq conditions when querying the document database.
    /// Syntactic sugar, if you want...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbDocumentCondition<T>
    {
        public Expression<Func<DbDocument<T>, bool>> Expression { get; }

        public DbDocumentCondition(Expression<Func<DbDocument<T>, bool>> expression)
        {
            this.Expression = expression;
        }

        public static implicit operator Expression<Func<DbDocument<T>, bool>>(DbDocumentCondition<T> condition)
        {
            return condition.Expression;
        }
    }
}
