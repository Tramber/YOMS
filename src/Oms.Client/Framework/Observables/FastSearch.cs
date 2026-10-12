using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oms.Client.Framework.Observables
{
    /// <summary>Row filter made of predicates that must all match.</summary>
    public class FastSearch<T>
    {
        private readonly List<Predicate<T>> _predicates = new List<Predicate<T>>(); 

        /// <summary>True when the item matches every predicate, or when there are none.</summary>
        public bool Predicate(T item)
        {
            return _predicates.Count == 0 || _predicates.All(p => Matches(p, item));
        }

        // A null on the way down a nested path (Security.IsinCode with no Security) is a non-match, not an error.
        private static bool Matches(Predicate<T> predicate, T item)
        {
            try
            {
                return predicate(item);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public void AddPredicate(Expression<Predicate<T>> predicateExpression)
        {
            _predicates.Add(predicateExpression.Compile());
        }

        /// <summary>Adds a predicate written as <c>Property = "value"</c>; nested paths such as <c>Security.IsinCode</c> are allowed. Throws when the text is not in that form or the value cannot be converted to the property type.</summary>
        public void AddPredicate(string expressionString)
        {
            var param = expressionString.Split(' ');
            var tpe = Expression.Parameter(typeof(T));
            Type propType;
            var left = GetPropertyInfo<T>(CleanPropertyName(param[0].Trim()), tpe, out propType);
            var right = Expression.Convert(Expression.Constant(param[2].Trim('"')), propType);
            var innerExpr = Expression.Lambda<Predicate<T>>(Expression.Equal(left, right), tpe);
            _predicates.Add(innerExpr.Compile());
        }

        private readonly Dictionary<string, string> _propertyMappings = new Dictionary<string, string>
        {
            {"IsinCode", "Security.IsinCode"}
        };

        private string CleanPropertyName(string propertyName)
        {
            string result;
            return _propertyMappings.TryGetValue(propertyName, out result) ? result : propertyName;
        }

        private Expression GetPropertyInfo<T>(string propertyName, Expression variable, out Type propertyType)
        {
            Expression result = variable;
            propertyType = typeof (T);
            foreach (var p in propertyName.Split('.'))
            {
                var prop = propertyType.GetProperty(p);
                result = Expression.Property(result, prop);
                propertyType = prop.PropertyType;
            }
            return result;
        }

        public void Clear()
        {
            _predicates.Clear();
        }
    }



    ////public static class ExpressionExtensions
    ////{
    ////    public static Expression<Predicate<T>> StringToPredicate<T>(string propName, string opr, string value, Expression<Predicate<T>> expr = null)
    ////    {
    ////        Expression<Func<T, bool>> func = null;
    ////        try
    ////        {
    ////            var prop = GetProperty<T>(propName);
    ////            ParameterExpression tpe = Expression.Parameter(typeof(T));
    ////            Expression left = Expression.Property(tpe, prop);
    ////            Expression right = Expression.Convert(ToExprConstant(prop, value), prop.PropertyType);
    ////            Expression<Func<T, bool>> innerExpr = Expression.Lambda<Func<T, bool>>(ApplyFilter(opr, left, right), tpe);
    ////            if (expr != null)
    ////                innerExpr = innerExpr.And(expr);
    ////            func = innerExpr;
    ////        }
    ////        catch { }
    ////        return func;
    ////    }
    ////    private static Expression ToExprConstant(PropertyInfo prop, string value)
    ////    {
    ////        object val = null;
    ////        switch (prop.PropertyTypeName())
    ////        {
    ////            case "System.Guid":
    ////                val = value.ToGuid();
    ////                break;
    ////            default:
    ////                val = Convert.ChangeType(value, Type.GetType(prop.PropertyTypeName()));
    ////                break;
    ////        }
    ////        return Expression.Constant(val);
    ////    }
    ////    private static BinaryExpression ApplyFilter(string opr, Expression left, Expression right)
    ////    {
    ////        BinaryExpression InnerLambda = null;
    ////        switch (opr)
    ////        {
    ////            case "==":
    ////            case "=":
    ////                InnerLambda = Expression.Equal(left, right);
    ////                break;
    ////            case "<":
    ////                InnerLambda = Expression.LessThan(left, right);
    ////                break;
    ////            case ">":
    ////                InnerLambda = Expression.GreaterThan(left, right);
    ////                break;
    ////            case ">=":
    ////                InnerLambda = Expression.GreaterThanOrEqual(left, right);
    ////                break;
    ////            case "<=":
    ////                InnerLambda = Expression.LessThanOrEqual(left, right);
    ////                break;
    ////            case "!=":
    ////                InnerLambda = Expression.NotEqual(left, right);
    ////                break;
    ////            case "&&":
    ////                InnerLambda = Expression.And(left, right);
    ////                break;
    ////            case "||":
    ////                InnerLambda = Expression.Or(left, right);
    ////                break;
    ////        }
    ////        return InnerLambda;
    ////    }
    ////    public static Expression<Func<T, TResult>> And<T, TResult>(this Expression<Func<T, TResult>> expr1, Expression<Func<T, TResult>> expr2)
    ////    {
    ////        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
    ////        return Expression.Lambda<Func<T, TResult>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    ////    }
    ////    public static Func<T, TResult> ExpressionToFunc<T, TResult>(this Expression<Func<T, TResult>> expr)
    ////    {
    ////        return expr.Compile();
    ////    }
    ////}
}
