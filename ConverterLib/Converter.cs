using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using System.Xml.Serialization;
using SharedUserClasses;

namespace ConverterLib
{
    public class Converter
    {
        private readonly List<Rule> _rules;

        public Converter(List<Rule> rules)
        {
            _rules = rules;
        }

        public Converter(string xmlDoc)
        {
            var xml = new XmlSerializer(typeof(List<Rule>));
            _rules = xml.Deserialize(new StringReader(xmlDoc)) as List<Rule>;
        }

        private static Expression getExpression(string expr, ParameterExpression param)
        {
            Expression expression = param;
            foreach (var property in expr.Split('.'))
                expression = Expression.Property(expression, property);
            return expression;
        }

        private static object getPropertyValue(object o, string expr)
        {
            var param = Expression.Parameter(o.GetType(), "x");
            var l = Expression.Lambda(getExpression(expr, param), param);
            return l.Compile().DynamicInvoke(o);
        }

        private static object getValue(object o, SimpleBinding b)
        {
            return getPropertyValue(o, b.PropName);
        }

        private static object getValue(object o, ListBinding b)
        {
            var list = getPropertyValue(o, b.ListName) as IEnumerable<object>;
            if (list == null)
                return null;

            var filteredList = new List<object>();
            Type itemType = o.GetType().GetProperty(b.ListName).PropertyType.GetProperty("Item").PropertyType;
            foreach (var listItem in list)
            {
                bool isCanAdd = true;
                if (b.ListFilterOrInits != null && b.ListFilterOrInits.Count > 0)
                {
                    foreach (var listFilter in b.ListFilterOrInits)
                    {
                        var param = Expression.Parameter(itemType, "x");
                        Expression expression = getExpression(listFilter.PropName, param);
                        expression = Expression.Call(expression, itemType.GetMethod("ToString"));
                        if (listFilter.Condition == "equal")
                            expression = Expression.Equal(expression, Expression.Constant(listFilter.PropValue));
                        var res = (bool)Expression.Lambda(expression, param).Compile().DynamicInvoke(listItem);
                        if (!res)
                        {
                            isCanAdd = false;
                            break;
                        }
                    }
                }
                if (isCanAdd)
                    filteredList.Add(listItem);

            }
            if (filteredList.Count > b.ItemIndex)
                return getPropertyValue(filteredList[b.ItemIndex], b.PropName);
            return null;
        }

        private static object getValue(object o, DicBinding b)
        {
            Type propType = o.GetType().GetProperty(b.PropName).PropertyType;
            object dic = getPropertyValue(o, b.PropName);
            if (dic == null)
                return null;
            Expression express =
                Expression.Call(Expression.Constant(dic),
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    propType.GetMethod("get_Item"), Expression.Constant(b.Key));
            var l = Expression.Lambda(express);
            object res = null;
            try
            {
                res = l.Compile().DynamicInvoke();
            }
            catch (TargetInvocationException)
            {
            }

            return res;
        }

        private static void setPropValue(object dest, string propName, object val)
        {
            var param = Expression.Parameter(dest.GetType(), "x");
            var proptype = dest.GetType().GetProperty(propName).PropertyType;
            Expression left = null;
            if (proptype.BaseType != null && proptype.BaseType.Name == "Enum")
            {
                var parseMethod = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string) });
                left = Expression.Assign(getExpression(propName, param), Expression.Convert(
                    Expression.Call(
                        parseMethod, Expression.Constant(proptype),
                        Expression.Constant(val)), proptype));
            }
            if (proptype.Name == "Boolean")
            {
                left = Expression.Assign(getExpression(propName, param), Expression.Call(typeof(Boolean).GetMethod("Parse"), Expression.Constant(val)));
            }

            if (left == null)
                left = Expression.Assign(getExpression(propName, param), Expression.Constant(val));

            var l = Expression.Lambda(left, param);
            l.Compile().DynamicInvoke(dest);
        }

        private static void setValue(object dest, SimpleBinding b, object val)
        {
            setPropValue(dest, b.PropName, val);
        }

        private static void setValue(object dest, DicBinding b, object val)
        {
            Type propType = dest.GetType().GetProperty(b.PropName).PropertyType;
            object dic = getPropertyValue(dest, b.PropName);
            if (dic == null)
                return;
            Expression express =
                Expression.Call(Expression.Constant(dic),
                    propType.GetMethod("Add"), Expression.Constant(b.Key), Expression.Constant(val));
            var l = Expression.Lambda(express);
            try
            {
                l.Compile().DynamicInvoke();
            }
            catch (ArgumentException)
            {
            }
        }

        private static void setValue(object dest, ListBinding b, object val)
        {
            object list = getPropertyValue(dest, b.ListName);
            if (list == null)
                return;
            Type itemType = dest.GetType().GetProperty(b.ListName).PropertyType.GetProperty("Item").PropertyType;

            var newobj = Expression.Lambda(Expression.New(itemType)).Compile().DynamicInvoke();

            setPropValue(newobj, b.PropName, val);

            if (b.ListFilterOrInits != null && b.ListFilterOrInits.Count > 0)
            {
                foreach (var listInit in b.ListFilterOrInits)
                {
                    setPropValue(newobj, listInit.PropName, listInit.PropValue);
                }
            }

            var addMethod = dest.GetType().GetProperty(b.ListName).PropertyType.GetMethod("Add", new[] { itemType });

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            var l1 = Expression.Lambda(Expression.Call(Expression.Constant(list), addMethod, Expression.Constant(newobj)));
            l1.Compile().DynamicInvoke();
        }


        private void convert(object src, object dest, bool isChangedRulesDirection)
        {

            foreach (var rule in _rules)
            {
                PropBinding sourcePropBinding;
                PropBinding destPropBinding;
                object val = null;
                if (isChangedRulesDirection)
                {
                    sourcePropBinding = rule.Dest;
                    destPropBinding = rule.Source;
                }
                else
                {
                    sourcePropBinding = rule.Source;
                    destPropBinding = rule.Dest;
                }

                if (sourcePropBinding is SimpleBinding)
                {
                    val = getValue(src, (SimpleBinding)sourcePropBinding);
                }
                if (sourcePropBinding is ListBinding)
                {
                    val = getValue(src, (ListBinding)sourcePropBinding);
                }
                if (sourcePropBinding is DicBinding)
                {
                    val = getValue(src, (DicBinding)sourcePropBinding);
                }
                if (val != null)
                {
                    if (destPropBinding is SimpleBinding)
                    {
                        setValue(dest, (SimpleBinding)destPropBinding, val);
                    }
                    if (destPropBinding is DicBinding)
                    {
                        setValue(dest, (DicBinding)destPropBinding, val);
                    }
                    if (destPropBinding is ListBinding)
                    {
                        setValue(dest, (ListBinding)destPropBinding, val);
                    }
                }
            }
        }

        public DestUser Convert(SrcUser srcUser)
        {
            var destUser = new DestUser { AdditionalProperties = new Dictionary<string, object>() };
            convert(srcUser, destUser, false);
            return destUser;
        }

        public SrcUser Convert(DestUser destUser)
        {
            var srcUser = new SrcUser
            {
                AdditionalProperties = new Dictionary<string, object>(),
                Phones = new List<SrcUserPhone>(),
                Emails = new List<SrcUserEmail>()
            };
            convert(destUser, srcUser, true);
            return srcUser;
        }
    }
}
