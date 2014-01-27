﻿/**
 *   Copyright 2013
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using BlueBoxSharp.Data.Entity;
using BlueBoxSharp.Data.Expressions;
using BlueBoxSharp.Helpers;

namespace BlueBoxSharp.Data.Converters
{
    internal class MemberAccessConverter : BaseConverter<MemberExpression>
    {
        internal MemberAccessConverter() : base(ExpressionType.MemberAccess) { }

        public override Expression Convert(ExpressionConverter converter, MemberExpression expression)
        {
            if (expression.Expression != null)
            {
                Expression instance = converter.Convert(expression.Expression);
                EntityExpression entity = GetEntityFromExpression(instance);

                // If expression is an IEntity try to find JoinColumn and create it (or get it if Join already exists)
                if (typeof(IEntity).IsAssignableFrom(expression.Type) && entity != null)
                {
                    JoinColumnAttribute joinAttr = expression.Member.GetCustomAttributes(true).OfType<JoinColumnAttribute>().FirstOrDefault();
                    if (joinAttr != null)
                        return entity.Join(converter, expression.Member, expression.Type);
                }

                if (instance is UnionQueryExpression)
                {
                    UnionQueryExpression qExp = (UnionQueryExpression)instance;
                    ProjectionItem item;

                    if (qExp.Projection.TryFindMember(expression.Member, out item))
                    {
                        if (qExp.IsDefaultProjection)
                            return item.Expression;
                        else
                            return new AliasedExpression(expression, item.Alias);
                    }

                    throw new InvalidOperationException("Member doesn't exists");
                }
                else if (instance is QueryExpression)
                {
                    QueryExpression qExp = (QueryExpression)instance;
                    Expression member;

                    if (qExp.Projection.TryFindMember(expression.Member, out member))
                        return member;

                    throw new InvalidOperationException("Member doesn't exists");
                }
                else if (instance is JoinExpression)
                {
                    JoinExpression jExp = (JoinExpression)instance;
                    return Expression.MakeMemberAccess(new EntityRefExpression(jExp.Entity), expression.Member);
                }
                else if (instance is ConstantExpression)
                {
                    Expression objectMember = Expression.Convert(expression, typeof(object));
                    Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    object getter = getterLambda.Compile()();

                    if (getter != null && typeof(IInternalQuery).IsAssignableFrom(getter.GetType()))
                    {
                        IInternalQuery set = (IInternalQuery)getter;
                        return converter.CreateQuery(set.Context, getter.GetType().GetGenericArguments()[0]);
                    }
                    else if (getter != null && getter.GetType() == expression.Type)
                        return Expression.Constant(getter);

                    return Expression.Convert(Expression.Constant(getter), expression.Type);
                }
                else if (instance is GroupByProjectionExpression)
                {
                    GroupByProjectionExpression eExp = (GroupByProjectionExpression)instance;
                    Expression member;

                    eExp.TryFindMember(expression.Member, out member);
                    return member;
                }

                return Expression.MakeMemberAccess(instance, expression.Member);
            }
            else if (expression.Expression == null)
            {
                object value = null;

                if (expression.Member.MemberType == MemberTypes.Property)
                    value = ((PropertyInfo)expression.Member).GetValue(null, null);
                else if(expression.Member.MemberType == MemberTypes.Method)
                    value = ((MethodInfo)expression.Member).Invoke(null, null);
                else if (expression.Member.MemberType == MemberTypes.Field)
                    value = ((FieldInfo)expression.Member).GetValue(null);

                if (value == null || value.GetType() == expression.Type)
                    return Expression.Constant(value);

                return Expression.Convert(Expression.Constant(value), expression.Type);
            }

            throw new InvalidOperationException("No Expression found");
        }

        private EntityExpression GetEntityFromExpression(Expression instance)
        {
            if (instance is UnionQueryExpression || instance is QueryExpression)
                return (EntityExpression)((QueryExpression)instance).From;
            else if (instance is EntityRefExpression)
                return ((EntityRefExpression)instance).Entity;
            else if (instance is JoinExpression)
                return ((JoinExpression)instance).Entity;
            else
                return null;
        }
    }
}
