﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
namespace SqlSugar
{
    public class ConstantExpressionResolve : BaseResolve
    {
        public ConstantExpressionResolve(ExpressionParameter parameter) : base(parameter)
        {
            var expression = base.Expression as ConstantExpression;
            var isLeft = parameter.IsLeft;
            object value = expression.Value;
            var baseParameter = parameter.BaseParameter;
            baseParameter.ChildExpression = expression;
            var isSetTempData = baseParameter.CommonTempData.IsValuable() && baseParameter.CommonTempData.Equals(CommonTempDataType.Result);
            switch (parameter.Context.ResolveType)
            {
                case ResolveExpressType.SelectSingle:
                case ResolveExpressType.Update:
                case ResolveExpressType.SelectMultiple:
                    baseParameter.CommonTempData = value;
                    break;
                case ResolveExpressType.WhereSingle:
                case ResolveExpressType.WhereMultiple:
                    if (isSetTempData)
                    {
                        baseParameter.CommonTempData = value;
                    }
                    else
                    {
                        var parentIsBinary = parameter.BaseParameter.CurrentExpression is BinaryExpression;
                        var parentIsRoot = parameter.BaseParameter.CurrentExpression is LambdaExpression;
                        if (parentIsRoot && value != null && value.GetType() == PubConst.BoolType)
                        {
                            this.Context.Result.Append(value.ObjToBool() ? this.Context.DbMehtods.True() :this.Context.DbMehtods.False());
                            break;
                        }
                        if (parentIsBinary && value != null && value.GetType() == PubConst.BoolType && parameter.BaseExpression != null)
                        {
                            var isLogicOperator =
                               parameter.BaseExpression.NodeType == ExpressionType.And ||
                               parameter.BaseExpression.NodeType == ExpressionType.AndAlso ||
                               parameter.BaseExpression.NodeType == ExpressionType.Or ||
                               parameter.BaseExpression.NodeType == ExpressionType.OrElse;
                            if (isLogicOperator)
                            {
                                AppendMember(parameter, isLeft, (value.ObjToBool() ? this.Context.DbMehtods.True(): this.Context.DbMehtods.False()));
                                break;
                            }
                        }
                        if (value == null && parentIsBinary)
                        {
                            parameter.BaseParameter.ValueIsNull = true;
                            value = "NULL";
                        }
                        AppendValue(parameter, isLeft, value);
                    }
                    break;
                case ResolveExpressType.FieldSingle:
                case ResolveExpressType.FieldMultiple:
                default:
                    break;
            }
        }
    }
}
