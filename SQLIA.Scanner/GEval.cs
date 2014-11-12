/*
 * Created by SharpDevelop.
 * User: Feeling
 * Date: 2012/1/3
 * Time: 0:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.Units;

namespace antiSQLInjection
{
    /// <summary>
    /// Description of GEval.
    /// </summary>
    public class GEval
    {
        private evalVisitor ev = null;
        private GContext context = null;
        private Hashtable valueMap = new Hashtable();
        private Stack exprs = new Stack();
        public GEval(GContext context)
        {
            this.context = context;
        }

        /**
         * Evaluate a expression.
         * @param expr, condition need to be evaluated.
         * @param context, not used in current version
         * @return
         */

        public Boolean eavl(TLz_Node pnode, Boolean pIsLeafNode)
        {

            TLzCustomExpression expr = (TLzCustomExpression)pnode;
            ev = new evalVisitor(context, valueMap, exprs);
            return ev.eavl(expr);
        }

        public Object getValue()
        {
            if (ev != null)
                return ev.getValue();
            return null;
        }

    }

    class evalVisitor
    {

        public evalVisitor(GContext context, Hashtable valueMap, Stack exprs)
        {
            this.exprs = exprs;
            this.context = context;
            this.valueMap = valueMap;
        }

        public Object getValue()
        {
            return valueMap[exprs.Pop()];
        }

        private Stack exprs = null;
        private GContext context = null;
        private Hashtable valueMap = null;

        public Boolean eavl(TLzCustomExpression expr)
        {

            switch ((expr.oper))
            {
                case TLzOpType.Expr_Attr:
                    valueMap.Add(expr, new UnknownValue());
                    break;
                case TLzOpType.Expr_Const:
                    TLz_Const constExpr = (TLz_Const)expr.lexpr;
                    if (constExpr.valtype == TNodeTag.T_Null)
                    {
                        valueMap.Add(expr, null);
                    }
                    else
                    {
                        try
                        {
                            valueMap.Add(expr, this.eval_constant(constExpr));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    break;
                case TLzOpType.Expr_Unary:
                    if (expr.opname.TokenType == TTokenType.ttMinus)
                    {
                        try
                        {
                            long l = Coercion.coerceLong(valueMap[exprs.Pop()]);
                            valueMap.Add(expr, -l);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    else if (expr.opname.TokenType == TTokenType.ttPlus)
                    {
                        try
                        {
                            long l = Coercion.coerceLong(valueMap[exprs.Pop()]);
                            valueMap.Add(expr, l);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    break;
                case TLzOpType.Expr_Comparison:
                    try
                    {
                        valueMap.Add(expr, eval_simple_comparison_conditions(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_OR:
                    try
                    {
                        valueMap.Add(expr, eval_logical_conditions_or(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_AND:
                    try
                    {
                        valueMap.Add(expr, eval_logical_conditions_and(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_NOT:
                    try
                    {
                        valueMap.Add(expr, eval_logical_conditions_not(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_XOR:
                    try
                    {
                        valueMap.Add(expr, eval_unknown_two_operand(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_IsNull:
                case TLzOpType.Expr_IsNotNull:
                    try
                    {
                        valueMap.Add(expr, eval_isnull(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_Between:
                case TLzOpType.Expr_BetweenTo:
                    try
                    {
                        valueMap.Add(expr, eval_unknown_two_operand(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_isoftype:
                case TLzOpType.Expr_isoftype_list:
                    try
                    {
                        valueMap.Add(expr, eval_unknown_one_operand(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_Leftjoin:
                case TLzOpType.Expr_Rightjoin:
                    try
                    {
                        valueMap.Add(expr, eval_unknown_two_operand(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_ArrayAccess:
                    valueMap.Add(expr, new UnknownValue());
                    break;
                case TLzOpType.Expr_GroupingSets:
                   try
                    {
                        valueMap.Add(expr, eval_group_comparison_conditions(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                   break;
                case TLzOpType.Expr_YearToMonth:
                case TLzOpType.Expr_DayToSecond:
                case TLzOpType.Expr_DatetimeAtLocal:
                   try
                   {
                       valueMap.Add(expr, eval_unknown_one_operand(expr));
                   }
                   catch (Exception e)
                   {
                       Console.WriteLine("Something wrong {0}", e);
                   }
                   break;
                case TLzOpType.Expr_AtTimeZone:
                   try
                   {
                       valueMap.Add(expr, eval_unknown_two_operand(expr));
                   }
                   catch (Exception e)
                   {
                       Console.WriteLine("Something wrong {0}", e);
                   }
                   break;
                case TLzOpType.Expr_Arithmetic:
                    if (expr.opname.TokenType.Equals(TTokenType.ttPlus))
                    {
                        try
                        {
                            valueMap.Add(expr, eval_add(expr));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    else if (expr.opname.TokenType.Equals(TTokenType.ttMinus))
                    {
                        try
                        {
                            valueMap.Add(expr, eval_subtract(expr));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    else if (expr.opname.TokenType.Equals(TTokenType.ttMulti))
                    {
                        try
                        {
                            valueMap.Add(expr, this.eval_mul(expr));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    else if (expr.opname.TokenType.Equals(TTokenType.ttDiv))
                    {
                        try
                        {
                            valueMap.Add(expr, this.eval_divide(expr));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    else if (expr.opname.SourceCode.Trim().Equals("%"))
                    {
                        try
                        {
                            valueMap.Add(expr, this.eval_mod(expr));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Something wrong {0}", e);
                        }
                    }
                    break;
                case TLzOpType.Expr_Parenthesis:
                    valueMap.Add(expr, valueMap[exprs.Pop()]);
                    break;
                case TLzOpType.Expr_Like:
                case TLzOpType.Expr_NotLike:
                    try
                    {
                        valueMap.Add(expr, this.eval_like(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_Assign:
                    try
                    {
                        this.eval_assignment(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_ConcatenationOP:
                    try
                    {
                        valueMap.Add(expr, this.eval_concatenate(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_FuncCall:
                case TLzOpType.Expr_Cursor:
                case TLzOpType.Expr_subquery:
                case TLzOpType.Expr_Case:
                    valueMap.Add(expr, new UnknownValue());
                    break;
                case TLzOpType.Expr_Exists:
                    try
                    {
                        valueMap.Add(expr, this.eval_exists_condition(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_BitWise:
                    try
                    {
                        valueMap.Add(expr, eval_unknown_two_operand(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                case TLzOpType.Expr_In:
                    try
                    {
                        valueMap.Add(expr, eval_in_conditions(expr));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something wrong {0}", e);
                    }
                    break;
                default:
                    break;
            }  //switch

            exprs.Push(expr);
            return true;
        }

        Object eval_constant(TLz_Const expr)
        {
            Object ret = null;
            if (expr.valtype == TNodeTag.T_Integer)
            {
                ret = Coercion.coerceInteger(expr.StartToken.AsText);
            }
            else if (expr.valtype == TNodeTag.T_Float)
            {
                ret = Coercion.coerceDouble(expr.StartToken.AsText);
            }
            else if (expr.valtype == TNodeTag.T_String)
            {
                String s = expr.StartToken.AsText.Substring(1, expr.StartToken.AsText.Length - 2);
                ret = s;
            }
            return ret;
        }

        Object eval_logical_conditions_and(TLzCustomExpression expr)
        {
            Object right = valueMap[(TLzCustomExpression)exprs.Pop()];
            Object left = valueMap[(TLzCustomExpression)exprs.Pop()];

            if (right is UnknownValue)
            {
                if (left is UnknownValue)
                {
                    return new UnknownValue();
                }
                else
                {
                    Boolean leftValue = Coercion.coerceBoolean(left);
                    if (!leftValue)
                    {
                        return false;
                    }
                    else
                    {
                        return new UnknownValue();
                    }
                }
            }
            else if (left is UnknownValue)
            {
                Boolean rightValue = Coercion.coerceBoolean(right);
                if (!rightValue)
                {
                    return false;
                }
                else
                {
                    return new UnknownValue();
                }
            }
            else
            {
                Boolean leftValue = Coercion.coerceBoolean(left);
                Boolean rightValue = Coercion.coerceBoolean(right);

                return (leftValue && rightValue) ? true
                        : false;
            }

        }

        Object eval_in_conditions(TLzCustomExpression expr)
        {
            exprs.Pop();
            exprs.Pop();

            return new UnknownValue();
        }

        Object eval_group_comparison_conditions(TLzCustomExpression expr)
        {
            exprs.Pop();
            exprs.Pop();

            return new UnknownValue();
        }

        Object eval_logical_conditions_not(TLzCustomExpression expr)
        {
            Object left = valueMap[(TLzCustomExpression)exprs.Pop()];

            if (left is UnknownValue)
            {
                return new UnknownValue();
            }
            Boolean b = Coercion.coerceBoolean(left);

            return b ? false : true;
        }

        Object eval_logical_conditions_or(TLzCustomExpression expr)
        {
            Object right = valueMap[(TLzCustomExpression)exprs.Pop()];
            Object left = valueMap[(TLzCustomExpression)exprs.Pop()];

            if (right is UnknownValue)
            {
                if (left is UnknownValue)
                {
                    return new UnknownValue();
                }
                else
                {
                    Boolean leftValue = Coercion.coerceBoolean(left);
                    if (leftValue)
                    {
                        return true;
                    }
                    else
                    {
                        return new UnknownValue();
                    }
                }
            }
            else if (left is UnknownValue)
            {
                Boolean rightValue = Coercion.coerceBoolean(right);
                if (rightValue)
                {
                    return true;
                }
                else
                {
                    return new UnknownValue();
                }
            }
            else
            {
                Boolean leftValue = Coercion.coerceBoolean(left);
                Boolean rightValue = Coercion.coerceBoolean(right);

                return (leftValue || rightValue) ? true
                        : false;
            }
        }

        //Object eval_between(TLzCustomExpression expr)
        //{
        //    Object right = valueMap[(TLzCustomExpression)exprs.Pop()];
        //    Object left = valueMap[(TLzCustomExpression)exprs.Pop()];

        //    GEval v = new GEval();
        //    Object between = v.value(expr.getBetweenOperand(), null);


        //    if ((between is UnknownValue)
        //            || (left is UnknownValue)
        //            || (right is UnknownValue)
        //    )
        //    {
        //        return new UnknownValue();
        //    }
        //    else
        //    {
        //        Long betweenValue = Coercion.coerceLong(between);
        //        Long rightValue = Coercion.coerceLong(right);
        //        Long leftValue = Coercion.coerceLong(left);
        //        return (betweenValue >= leftValue && betweenValue <= rightValue) ? Boolean.TRUE : Boolean.FALSE;
        //    }

        //}

        Object eval_isnull(TLzCustomExpression expr)
        {
            Object left = valueMap[(TLzCustomExpression)exprs.Pop()];
            if (left is UnknownValue)
            {
                return new UnknownValue();
            }
            else
            {
                if (left == null)
                {
                    if (expr.oper == TLzOpType.Expr_IsNotNull)
                    {
                        return false; //null is not null, return false
                    }
                    else
                    {
                        return true; //null is null, return true
                    }
                }
                else if (left is Boolean &&  ((Boolean)left) == false)
                {
                    if (expr.oper == TLzOpType.Expr_IsNotNull)
                    {
                        return true; //not null is not null, return false
                    }
                    else
                    {
                        return false; //not null is null, return false
                    }
                }
                else
                {
                    return new UnknownValue();
                }
            }
        }

        Object eval_simple_comparison_conditions(TLzCustomExpression expr)
        {
            Object right = valueMap[(TLzCustomExpression)exprs.Peek()];
            Object left = valueMap[(TLzCustomExpression)exprs.Peek()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                exprs.Pop();
                exprs.Pop();
                return new UnknownValue();
            }

            if (expr.opname.TokenCode == (int)'=')
            {
                return eval_equal(expr);
            }
            else if (expr.opname.TokenCode == (int)'>')
            {
                return eval_gt(expr);
            }
            else if (expr.opname.TokenCode == (int)'<')
            {
                return eval_lt(expr);
            }
            else if (expr.opname.TokenType.Equals(TTokenType.ttMultiCharOperator))
            {
                if (expr.opname.AsText.Trim().Equals("<="))
                {
                    return eval_le(expr);
                }
                else if (expr.opname.AsText.Trim().Equals(">="))
                {
                    return eval_ge(expr);
                }
                else if (expr.opname.AsText.Trim().Equals("<>") || expr.opname.AsText.Trim().Equals("!="))
                {
                    return eval_notequal(expr);
                }
            }

            exprs.Pop();
            exprs.Pop();
            return new UnknownValue();
        }


        Object eval_notequal(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == null && right == null)
            {
                /*
                 * first, the possibility that both *are* null
                 */

                return false;
            }
            else if (left == null || right == null)
            {
                /*
                 * otherwise, both aren't, so it's clear L != R
                 */
                return true;
            }
            else if (left.GetType().Equals(right.GetType()))
            {
                return (left.Equals(right)) ? false : true;
            }
            else if (left is float
              || left is double
              || right is float
              || right is double)
            {
                return (Coercion.coerceDouble(left).Equals(Coercion.coerceDouble(right))) ? false : true;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                return (Coercion.coerceLong(left).Equals(Coercion.coerceLong(right))) ? false : true;
            }
            else if (left is Boolean || right is Boolean)
            {
                return (Coercion.coerceBoolean(left).Equals(Coercion.coerceBoolean(right))) ? false : true;
            }
            else if (left is String || right is String)
            {
                return (left.ToString().Equals(right.ToString())) ? false : true;
            }

            return (left.Equals(right)) ? false : true;

        }

        Object eval_ge(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == right)
            {
                return true;
            }
            else if ((left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left)
                  || Coercion.isFloatingPoint(right))
            {
                double leftDouble = Coercion.coerceDouble(left);
                double rightDouble = Coercion.coerceDouble(right);

                return leftDouble >= rightDouble ? true : false;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                long leftLong = Coercion.coerceLong(left);
                long rightLong = Coercion.coerceLong(right);

                return leftLong >= rightLong ? true : false;
            }
            else if (left is String || right is String)
            {
                String leftString = left.ToString();
                String rightString = right.ToString();

                return leftString.CompareTo(rightString) >= 0 ? true
                        : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) >= 0 ? true
                        : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) <= 0 ? true
                        : false;
            }

            throw new Exception("Invalid comparison : GE ");
        }

        Object eval_le(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == right)
            {
                return true;
            }
            else if ((left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left)
                  || Coercion.isFloatingPoint(right))
            {
                double leftDouble = Coercion.coerceDouble(left);
                double rightDouble = Coercion.coerceDouble(right);

                return leftDouble <= rightDouble ? true : false;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                long leftLong = Coercion.coerceLong(left);
                long rightLong = Coercion.coerceLong(right);

                return leftLong <= rightLong ? true : false;
            }
            else if (left is String || right is String)
            {
                String leftString = left.ToString();
                String rightString = right.ToString();

                return leftString.CompareTo(rightString) <= 0 ? true
                        : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) <= 0 ? true
                        : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) >= 0 ? true
                        : false;
            }

            throw new Exception("Invalid comparison : LE ");
        }

        Object eval_lt(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if ((left == right) || (left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left)
                    || Coercion.isFloatingPoint(right))
            {
                double leftDouble = Coercion.coerceDouble(left);
                double rightDouble = Coercion.coerceDouble(right);

                return leftDouble < rightDouble ? true : false;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                long leftLong = Coercion.coerceLong(left);
                long rightLong = Coercion.coerceLong(right);

                return leftLong < rightLong ? true : false;
            }
            else if (left is String || right is String)
            {
                String leftString = left.ToString();
                String rightString = right.ToString();

                return leftString.CompareTo(rightString) < 0 ? true
                        : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) < 0 ? true
                        : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) > 0 ? true
                        : false;
            }

            throw new Exception("Invalid comparison : LT ");
        }

        Object eval_gt(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if ((left == right) || (left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left)
                    || Coercion.isFloatingPoint(right))
            {
                double leftDouble = Coercion.coerceDouble(left);
                double rightDouble = Coercion.coerceDouble(right);

                return leftDouble > rightDouble ? true : false;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                long leftLong = Coercion.coerceLong(left);
                long rightLong = Coercion.coerceLong(right);

                return leftLong > rightLong ? true : false;
            }
            else if (left is String || right is String)
            {
                String leftString = left.ToString();
                String rightString = right.ToString();

                return leftString.CompareTo(rightString) > 0 ? true
                        : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) > 0 ? true
                        : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) < 0 ? true
                        : false;
            }

            throw new Exception("Invalid comparison : GT ");

        }

        Object eval_equal(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == null && right == null)
            {
                /*
                 * if both are null L == R
                 */
                return true;
            }
            else if (left == null || right == null)
            {
                /*
                 * we know both aren't null, therefore L != R
                 */
                return false;
            }
            else if (left.GetType().Equals(right.GetType()))
            {
                return left.Equals(right) ? true : false;
            }
            else if (left is float || left is double
                  || right is float || right is double)
            {
                Double l = Coercion.coerceDouble(left);
                Double r = Coercion.coerceDouble(right);

                return l.Equals(r) ? true : false;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                return Coercion.coerceLong(left).Equals(Coercion.coerceLong(right)) ? true
                        : false;
            }
            else if (left is Boolean || right is Boolean)
            {
                return Coercion.coerceBoolean(left).Equals(
                        Coercion.coerceBoolean(right)) ? true
                        : false;
            }
            else if (left is String || right is String)
            {
                return left.ToString().Equals(right.ToString()) ? true
                        : false;
            }

            return left.Equals(right) ? true : false;
        }

        Object eval_add(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            /*
             *  the spec says 'and'
             */
            if (left == null && right == null)
            {
                return 0L;
            }

            /*
             *  if anything is float, double or string with ( "." | "E" | "e")
             *  coerce all to doubles and do it
             */
            if (left is float || left is double
                || right is float || right is double
                || (left is String
                      && (((String)left).IndexOf(".") != -1
                              || ((String)left).IndexOf("e") != -1
                              || ((String)left).IndexOf("E") != -1)
                   )
                || (right is String
                      && (((String)right).IndexOf(".") != -1
                              || ((String)right).IndexOf("e") != -1
                              || ((String)right).IndexOf("E") != -1)
                   )
                )
            {

                /*
                 * in the event that either is null and not both, then just make the
                 * null a 0
                 */

                try
                {
                    double l = Coercion.coerceDouble(left);
                    double r = Coercion.coerceDouble(right);
                    return l + r;
                }
                catch (Exception nfe)
                {
                    /*
                     * Well, use strings!
                     */
                    return left.ToString() + right.ToString();
                }
            }

            /*
             * attempt to use Longs
             */
            try
            {
                long l = Coercion.coerceLong(left);
                long r = Coercion.coerceLong(right);
                return l + r;
            }
            catch (Exception nfe)
            {
                /*
                 * Well, use strings!
                 */
                return left.ToString() + right.ToString();
            }
        }

        Object eval_subtract(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            /*
             * the spec says 'and', I think 'or'
             */
            if (left == null && right == null)
            {
                return (byte)0;
            }

            /*
             * if anything is float, double or string with ( "." | "E" | "e") coerce
             * all to doubles and do it
             */
            if (left is float
                || left is double
                || right is float
                || right is double
                || (left is String
                    && (((String)left).IndexOf(".") != -1
                        || ((String)left).IndexOf("e") != -1
                        || ((String)left).IndexOf("E") != -1))
                || (right is String
                    && (((String)right).IndexOf(".") != -1
                        || ((String)right).IndexOf("e") != -1
                        || ((String)right).IndexOf("E") != -1)))
            {
                double l = Coercion.coerceDouble(left);
                double r = Coercion.coerceDouble(right);

                return l - r; ;
            }

            /*
             * otherwise to longs with thee!
             */

            return Coercion.coerceLong(left) - Coercion.coerceLong(right);
        }

        Object eval_mod(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if (left == null && right == null)
            {
                return (byte)0;
            }

            /*
             * if anything is float, double or string with ( "." | "E" | "e") coerce
             * all to doubles and do it
             */
            if (left is float
                || left is double
                || right is float
                || right is double
                || (left is String
                    && (((String)left).IndexOf(".") != -1
                        || ((String)left).IndexOf("e") != -1
                        || ((String)left).IndexOf("E") != -1))
                || (right is String
                    && (((String)right).IndexOf(".") != -1
                        || ((String)right).IndexOf("e") != -1
                        || ((String)right).IndexOf("E") != -1)))
            {
                double l = Coercion.coerceDouble(left);
                double r = Coercion.coerceDouble(right);

                /*
                 * catch div/0
                 */
                if (r == 0.0)
                {
                    return 0.0;
                }

                return l % r;
            }

            /*
             * otherwise to longs with thee!
             */
            {
                long l = Coercion.coerceLong(left);
                long r = Coercion.coerceLong(right);

                /*
                 * catch the div/0
                 */
                if (r == 0)
                {
                    return 0L;
                }

                return l % r;
            }
        }

        Object eval_divide(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if (left == null && right == null)
            {
                return (byte)0;
            }

            Double l = Coercion.coerceDouble(left);
            Double r = Coercion.coerceDouble(right);

            /*
             * catch div/0
             */
            if (r == 0.0)
            {
                return 0.0D;
            }

            return l / r;

        }

        Object eval_mul(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];


            /*
             * the spec says 'and', I think 'or'
             */
            if (left == null && right == null)
            {
                return (byte)0;
            }

            /*
             * if anything is float, double or string with ( "." | "E" | "e") coerce
             * all to doubles and do it
             */
            if (left is float
                || left is double
                || right is float
                || right is double
                || (left is String
                    && (((String)left).IndexOf(".") != -1
                        || ((String)left).IndexOf("e") != -1
                        || ((String)left).IndexOf("E") != -1))
                || (right is String
                    && (((String)right).IndexOf(".") != -1
                        || ((String)right).IndexOf("e") != -1
                        || ((String)right).IndexOf("E") != -1)))
            {
                double l = Coercion.coerceDouble(left);
                double r = Coercion.coerceDouble(right);

                return l * r;
            }

            /*
             * otherwise to longs with thee!
             */
            {
                long l = Coercion.coerceLong(left);
                long r = Coercion.coerceLong(right);

                return l * r;
            }
        }


        Object eval_concatenate(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            return left.ToString() + right.ToString();
        }

        Object eval_unknown_one_operand(TLzCustomExpression expr)
        {
            exprs.Pop();
            return new UnknownValue();
        }

        Object eval_unknown_two_operand(TLzCustomExpression expr)
        {
            exprs.Pop();
            exprs.Pop();
            return new UnknownValue();
        }

        void eval_assignment(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            valueMap.Add(exprs.Pop(), right);
        }

        Object eval_exists_condition(TLzCustomExpression expr)
        {
            // check condition in subquery
            if (expr.rexpr != null && expr.rexpr is TSelectSqlStatement) {
                if (((TSelectSqlStatement)(expr.rexpr)).WhereClause != null)
                {
                    GEval e = new GEval(null);
                    ((TSelectSqlStatement)(expr.rexpr)).WhereClause.PostOrderTraverse(e.eavl);
                    return e.getValue();
                }
                else return true;
            }
            return new UnknownValue();
        }

        Object eval_like(TLzCustomExpression expr)
        {
            Object right = valueMap[exprs.Pop()];
            Object left = valueMap[exprs.Pop()];

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (right.ToString().StartsWith("%"))
            {
                if (right.ToString().EndsWith("%"))
                {
                    // 'abc' like '%abc%'
                    String c = right.ToString().Substring(1, right.ToString().Length - 2);
                    if (left.ToString().ToLower().Contains(c.ToLower()))
                    {
                        if (expr.opname.AsText.ToLower().Equals("not"))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (expr.opname.AsText.ToLower().Equals("not"))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // 'abc' like '%abc'
                    String c = right.ToString().Substring(1, right.ToString().Length - 1);
                    if (left.ToString().ToLower().EndsWith(c.ToLower()))
                    {
                        if (expr.opname.AsText.ToLower().Equals("not"))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (expr.opname.AsText.ToLower().Equals("not"))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else if (right.ToString().EndsWith("%"))
            {
                //'abc' like 'abc%'
                String c = right.ToString().Substring(0, right.ToString().Length - 1);
                if (left.ToString().ToLower().StartsWith(c.ToLower()))
                {
                    if (expr.opname.AsText.ToLower().Equals("not"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (expr.opname.AsText.ToLower().Equals("not"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // 'abc' like 'abc'
                if (right.ToString().ToLower().Equals(left.ToString().ToLower()))
                {
                    if (expr.opname.AsText.ToLower().Equals("not"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (expr.opname.AsText.ToLower().Equals("not"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        class Coercion
        {

            public static int coerceInteger(Object val)
            {
                if (val == null)
                {
                    return 0;
                }
                else if (val is String)
                {
                    if ("".Equals(val))
                    {
                        return 0;
                    }

                }
                return Convert.ToInt32(val);
            }

            public static double coerceDouble(Object val)
            {
                if (val == null)
                {
                    return 0;
                }
                else if (val is String)
                {
                    if ("".Equals(val))
                    {
                        return 0;
                    }

                }
                return Convert.ToDouble(val);
            }

            public static long coerceLong(Object val)
            {
                if (val == null)
                {
                    return 0L;
                }
                else if (val is String)
                {
                    if ("".Equals(val))
                    {
                        return 0L;
                    }
                }
                return Convert.ToInt64(val);

            }

            public static bool coerceBoolean(Object val)
            {
                if (val == null)
                {
                    return false;
                }
                else if (val is Boolean)
                {
                    return (Boolean)val;
                }
                else if (val is String)
                {
                    return Convert.ToBoolean((String)val);
                }
                return false;
            }

            public static Boolean isFloatingPoint(Object o)
            {
                return o is float || o is double;
            }

            public static Boolean isNumberable(Object o)
            {
                return o is int
                    || o is long
                    || o is byte
                    || o is short
                    || o is char;
            }
        }
    }

}
