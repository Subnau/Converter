using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ConverterLib
{
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     create expression by property name
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="propertyName">
        ///     <example>Urer.Role.Name</example>
        /// </param>
        /// <returns></returns>
        public static Expression<Func<TModel, dynamic>> CreateExpression<TModel>(this string propertyName)
        {
            Type currentType = typeof(TModel);
            ParameterExpression parameter = Expression.Parameter(currentType, "x");
            Expression expression = parameter;

            int i = 0;
            List<string> propertyChain = propertyName.Split('.').ToList();
            do
            {
                System.Reflection.PropertyInfo propertyInfo = currentType.GetProperty(propertyChain[i]);
                currentType = propertyInfo.PropertyType;
                i++;
                if (propertyChain.Count == i)
                {
                    currentType = typeof(object);
                }
                expression = Expression.Convert(Expression.PropertyOrField(expression, propertyInfo.Name), currentType);
            } while (propertyChain.Count > i);

            return Expression.Lambda<Func<TModel, dynamic>>(expression, parameter);
        }
    }

    class Program
    {
        public static void PrintPropertyAndObject<T>(Expression<Func<T>> e)
        {
            MemberExpression member = (MemberExpression)e.Body;
            Expression strExpr = member.Expression;
            if (strExpr.Type == typeof(String))
            {
                String str = Expression.Lambda<Func<String>>(strExpr).Compile()();
                Console.WriteLine("String: {0}", str);
            }
            string propertyName = member.Member.Name;
            T value = e.Compile()();
            Console.WriteLine("{0} : {1}", propertyName, value);
        }

        public enum MyEnum
        {
            Crow,
            Black
        }

        public class SubClass
        {
            public MyEnum GH { get; set; }
            public string Descr { get; set; }
        }

        public class BooClass
        {
            public string Name { get; set; }

            public SubClass Sc { get; set; }


            public List<SubClass> Phones { get; set; }

            public List<string> Strings { get; set; }
        }


        static Expression GetExpression(string expr, ParameterExpression param, Type t)
        {
            //            var param = Expression.Parameter(typeof(T), "x");
            Expression expression = param;
            foreach (var property in expr.Split('.'))
            {
                if (property.Contains("["))
                {
                    string regExpr = @"\[([^\]]*)\]";
                    string indexedPropertyName = Regex.Replace(property, regExpr, "");
                    int index = Convert.ToInt32(Regex.Match(property, regExpr).Groups[1].Value);

                    PropertyInfo propertyInfo = t.GetProperty(indexedPropertyName).PropertyType.GetProperty("Item");
                    expression = Expression.Property(param, indexedPropertyName);

                    expression =
                            Expression.MakeIndex(
                                    expression,
                                    propertyInfo,
                                    new[] { Expression.Constant(index) });

                }
                else
                {
                    expression = Expression.Property(expression, property);
                }
            }

            return expression;

        }

        //static Expression GetExpression<T>(string expr, ParameterExpression param)
        //{
        //    //            var param = Expression.Parameter(typeof(T), "x");
        //    Expression expression = param;
        //    foreach (var property in expr.Split('.'))
        //    {
        //        if (property.Contains("["))
        //        {
        //            string regExpr = @"\[([^\]]*)\]";
        //            string indexedPropertyName = Regex.Replace(property, regExpr, "");
        //            int index = Convert.ToInt32(Regex.Match(property, regExpr).Groups[1].Value);

        //            PropertyInfo propertyInfo = typeof(T).GetProperty(indexedPropertyName).PropertyType.GetProperty("Item");
        //            expression = Expression.Property(param, indexedPropertyName);

        //            expression =
        //                    Expression.MakeIndex(
        //                            expression,
        //                            propertyInfo,
        //                            new[] { Expression.Constant(index) });

        //        }
        //        else
        //        {
        //            expression = Expression.Property(expression, property);
        //        }
        //    }

        //    return expression;

        //}

        static void SetProperty(object source, object dest, string sourcePropName, string destPropName)
        {
            var param = Expression.Parameter(dest.GetType(), "x");
            Expression left = GetExpression(destPropName, param, dest.GetType());

            left = Expression.Assign(left, Expression.Constant(GetPropertyValue(source, sourcePropName)));

            //            var l = Expression.Lambda<Action<TDest>>(left, param);

            var l = Expression.Lambda(left, param);
            var o = l.Compile().DynamicInvoke(dest);

            //Expression right = GetExpression<TDest>(sourcePropName);

        }

        static object GetPropertyValue(object o, string expr)
        {
            var param = Expression.Parameter(o.GetType(), "x");
            Type t = o.GetType();
            //var l = Expression.Lambda<Func<object, object>>(GetExpression(expr, param, o.GetType()), param);
            //return l.Compile()(o);
            var l = Expression.Lambda(GetExpression(expr, param, o.GetType()), param);

            return l.Compile().DynamicInvoke(o);
        }

        //static object GetPropertyValue(object o, string expr)
        //{
        //    var param = Expression.Parameter(o.GetType(), "x");
        //    Type t = o.GetType();
        //    var l = Expression.Lambda<Func<t, object>>(GetExpression<T>(expr, param), param);

        //    return l.Compile()(o);
        //}
        static void a()
        {

        }

        private delegate void delea();

        static void Main(string[] args)
        {

            Expression<Action<Dictionary<string, object>>> f = (dic) => dic.Add("ss", "val");//b.Phones.Add(new SubClass() { Descr = "ss" });

            SrcUser srcUser = new SrcUser();
            DestUser destUser;
            srcUser.FirstName = "FirstName";
            srcUser.LastName = "LastName";
            srcUser.AdditionalProperties = new Dictionary<string, object>();
            srcUser.AdditionalProperties["Company Name"] = "Company1";
            srcUser.Phones = new List<SrcUserPhone>()
            {
                new SrcUserPhone() { Number = "Business-123", Type = SrcUserPhoneType.Business },
                new SrcUserPhone() { Number = "Mobile-123", Type = SrcUserPhoneType.Mobile },
                new SrcUserPhone() { Number = "Home-123", Type = SrcUserPhoneType.Home }
            };


            srcUser.Emails = new List<SrcUserEmail>()
            {
                new SrcUserEmail(){EmailAddress = "mail_primary",IsPrimary = true},
                new SrcUserEmail(){EmailAddress = "mail_first_not_primary",IsPrimary = false},
                new SrcUserEmail(){EmailAddress = "mail_second_not_primary",IsPrimary = false}
            };
            Converter c = new Converter(null);
            destUser = c.Convert(srcUser);
            srcUser = c.Convert(destUser);

            BooClass boo = new BooClass();
            BooClass boo1 = new BooClass() { Name = "bo1" };
            boo.Phones = new List<SubClass>();
            boo.Strings = new List<string>();
            //boo.Phones[0] = "ss";
            boo.Phones.Add(new SubClass() { Descr = "1", GH = MyEnum.Crow });
            boo.Phones.Add(new SubClass() { Descr = "2", GH = MyEnum.Black });
            boo.Phones.Add(new SubClass() { Descr = "1", GH = MyEnum.Crow });
            boo.Name = "glue";
            boo.Sc = new SubClass() { Descr = "BlaBla" };
            boo.Sc.GH = MyEnum.Black;

            //MemberExpression mem=Expression.MakeMemberAccess();
            //Expression exprProp = Expression.Property(Expression.Constant(boo), "Sc");

            //string s = Expression.Lambda<Func<string>>(exprProp).Compile()();
            //Expression<Func<BooClass, object>> firstNameExp =  e => e.Name;



            //Expression exprProp = Expression.Property(Expression.Constant(boo), "Phones");

            //string s = Expression.Lambda<Func<string>>(exprProp).Compile()();
            //var param = Expression.Parameter(typeof(BooClass), "x");
            //var b = Expression.PropertyOrField(param, "Phones");


            //            SetProperty(boo, boo1, "Sc.Descr", "Name");
            ////

            //            var param = Expression.Parameter(typeof(BooClass), "x");
            //            Expression expression;

            //Expression.Lambda<Func<string>>(x=>"ss").Compile()();

            //Expression<Func<SubClass, bool>> f1 = (subone) => subone.Descr == "1";

            //.Lambda #Lambda1<System.Func`2[ConverterLib.Program+SubClass,System.Boolean]>(ConverterLib.Program+SubClass $subone) {
            //    $subone.Descr == "1"
            //}

            string operLeft = "GH";
            string operRight = "Crow";
            //var param = Expression.Parameter(typeof(BooClass), "x");

            // expression = Expression.Equal(GetExpression<SubClass>(operLeft, param), Expression.Constant(operRight));

            //var res =Expression.Lambda( expression .Compile()(boo.Phones[0]);

            // expression = Expression.Property(param, "Name");
            Expression expression;

            IEnumerable<object> list = GetPropertyValue(boo, "Phones") as IEnumerable<object>;
            Type subclassType = typeof(BooClass).GetProperty("Phones").PropertyType.GetProperty("Item").PropertyType;

            foreach (var listItem in list)
            {
                var param = Expression.Parameter(subclassType, "x");
                expression = GetExpression(operLeft, param, subclassType);
                expression = Expression.Call(expression, subclassType.GetMethod("ToString"));
                expression = Expression.Equal(expression, Expression.Constant(operRight));
                var res = Expression.Lambda(expression, param).Compile(); //.Compile()((object)boo.Phones[0]);
                var res1 = res.DynamicInvoke(listItem);

            }

            expression = Expression.New(subclassType);

            var l = Expression.Lambda(expression).Compile();
            var newobj = l.DynamicInvoke();
            var param2 = Expression.Parameter(newobj.GetType(), "x");

            expression = GetExpression("Descr", param2, newobj.GetType());

            expression = Expression.Assign(expression, Expression.Constant("blavla"));

            var l1 = Expression.Lambda(expression, param2);
            var o = l1.Compile().DynamicInvoke(newobj);

            var oo = (MyEnum)Enum.Parse(typeof(MyEnum), "Crow");



            SubClass cls = new SubClass();
            Expression<Action<List<string>>> fff = (x) => x.Add("cool");
            var parseMethod = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string) });
            var proptype = newobj.GetType().GetProperty("GH").PropertyType;
            //expression = Expression.Call(
            //    parseMethod, Expression.Constant(typeof(MyEnum)),
            //    Expression.Constant("Crow"));
            expression = GetExpression("GH", param2, newobj.GetType());
            //expression = Expression.Assign(expression, Expression.Convert(Expression.Constant(Enum.Parse(typeof(MyEnum), "Black")), typeof(MyEnum)));
            expression = Expression.Assign(expression, Expression.Convert(
                Expression.Call(
                    parseMethod, Expression.Constant(newobj.GetType().GetProperty("GH").PropertyType), Expression.Constant("Black")), typeof(MyEnum)));
            //expression = Expression.Assign(expression, Expression.Constant(MyEnum.Black));
            //newobj.GetType().GetProperty("GH").PropertyType
            l1 = Expression.Lambda(expression, param2);
            o = l1.Compile().DynamicInvoke(newobj);

            param2 = Expression.Parameter(boo.Phones.GetType(), "x");




            var meth = boo.Phones.GetType().GetMethod("Add", new Type[] { newobj.GetType() });


            expression = param2;
            expression = Expression.Call(Expression.Constant(boo.Phones), meth, Expression.Constant(newobj));

            l1 = Expression.Lambda(expression, param2);
            o = l1.Compile().DynamicInvoke(boo.Phones);

            //expression = Expression .Bind( newobj.GetType().GetProperty("Descr"), Expression.Constant("dd"));

            //var param1 = Expression.Parameter(subclassType, "x");
            //Expression left = GetExpression("Descr", param1, subclassType);

            //left = Expression.Assign(left, Expression.Constant(GetPropertyValue<TSource>(source, sourcePropName)));

            ////            var l = Expression.Lambda<Action<TDest>>(left, param);

            //var l = Expression.Lambda<Action<TDest>>(left, param);
            //l.Compile()(dest);






            //expression=Expression.MemberInit(expression,new MemberBinding[]{Expression.Bind()})
            //var res = Expression.Lambda<Func<object, bool>>(expression, param).Compile();

            //g = Func<object,bool>};
            //var bres = res(boo.Phones[0]);
            //.Lambda #Lambda1<System.Action`1[ConverterLib.Program+BooClass]>(ConverterLib.Program+BooClass $b) {
            //    .Call ($b.Phones).Add(.New ConverterLib.Program+SubClass(){
            //            Descr = "ss"
            //        })
            //}

            //PropertyInfo propertyInfo = typeof(BooClass).GetProperty("Phones").PropertyType.GetProperty("Item");

            //var s = Expression.Lambda<Func<BooClass, object>>(expression, param);

            //string s1 = s.Compile()(boo).ToString();
            //////boo.Name.CreateExpression<>()

            //string ss = GetPropertyValue(boo, "Sc.Descr").ToString();

            //Console.WriteLine(Regex.Replace("ddeded[0]", @"\[([^\]]*)\]", ""));
            //var r = Regex.Match("ddeded[0]", @"\[([^\]]*)\]");
            //Console.WriteLine(r.Groups[1].Value);

            Console.ReadLine();
        }
    }
}
