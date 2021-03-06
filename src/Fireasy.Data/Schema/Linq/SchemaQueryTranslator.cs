﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Schema.Linq
{
    internal sealed class SchemaQueryTranslator : Common.Linq.Expressions.ExpressionVisitor
    {
        private Type metadataType;
        private string memberName;
        private List<MemberInfo> mbrRestrs = null;
        private readonly RestrictionDictionary dicRestr = new RestrictionDictionary();

        /// <summary>
        /// 对表达式进行解析，并返回限制值字典。
        /// </summary>
        /// <param name="expression">查询表达式。</param>
        /// <param name="dicRestrMbrs"></param>
        /// <returns></returns>
        public static RestrictionDictionary GetRestrictions<T>(Expression expression, Dictionary<Type, List<MemberInfo>> dicRestrMbrs)
        {
            if (expression == null)
            {
                return RestrictionDictionary.Empty;
            }

            var translator = new SchemaQueryTranslator { metadataType = typeof(T) };

            if (!dicRestrMbrs.TryGetValue(typeof(T), out List<MemberInfo> properties))
            {
                throw new SchemaQueryTranslateException(typeof(T));
            }

            translator.mbrRestrs = properties;
            expression = PartialEvaluator.Eval(expression);
            translator.Visit(expression);
            return translator.dicRestr;
        }

        /// <summary>
        /// 访问表达式树。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override Expression Visit(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)expression);
                case ExpressionType.Equal:
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
            }
            return base.Visit(expression);
        }

        /// <summary>
        /// 访问二元运算表达式。
        /// </summary>
        /// <param name="binaryExp"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression binaryExp)
        {
            memberName = string.Empty;

            if (binaryExp.Right is MemberExpression rmbr && rmbr.Member.DeclaringType == metadataType)
            {
                Visit(binaryExp.Right);
                Visit(binaryExp.Left);
            }
            else if (binaryExp.Left is MemberExpression lmbr && lmbr.Member.DeclaringType == metadataType)
            {
                Visit(binaryExp.Left);
                Visit(binaryExp.Right);
            }
            else
            {
                Visit(binaryExp.Left);
                Visit(binaryExp.Right);
            }

            return binaryExp;
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            //如果属性是架构元数据类的成员
            if (memberExp.Member.DeclaringType == metadataType)
            {
                if (!mbrRestrs.Contains(memberExp.Member))
                {
                    throw new SchemaQueryTranslateException(memberExp.Member, mbrRestrs);
                }

                memberName = memberExp.Member.Name;
                return memberExp;
            }

            //值或引用
            var exp = (Expression)memberExp;
            if (memberExp.Type.IsValueType)
            {
                exp = Expression.Convert(memberExp, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object>>(exp);
            var fn = lambda.Compile();

            //转换为常量表达式
            return Visit(Expression.Constant(fn(), memberExp.Type));
        }

        protected override Expression VisitConstant(ConstantExpression constExp)
        {
            if (!string.IsNullOrEmpty(memberName))
            {
                dicRestr[memberName] = constExp.Value;
            }

            return constExp;
        }
    }
}
