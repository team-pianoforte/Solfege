﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pianoforte.Sena.Lang.Runtime
{
  public static class Operations
  {
    public static Value Add(Value lhs, Value rhs)
    {
      if (lhs.Type == ValueType.String || rhs.Type == ValueType.String)
      {
        return Value.MakeString(lhs.ToString() + rhs.ToString());
      }
      if (lhs.Type == ValueType.Number && rhs.Type == ValueType.Number)
      {
        return Value.MakeNumber(lhs.Number + rhs.Number);
      }
      if (lhs.Type == ValueType.Array && rhs.Type == ValueType.Array)
      {
        return Value.MakeArray(lhs.Array.Concat(rhs.Array));
      }
      throw new RuntimeException(string.Format(Properties.Resources.InvalidAddition, lhs, rhs));
    }

    public static Value Subtract(Value lhs, Value rhs)
    {
      if (lhs.Type == ValueType.Number && rhs.Type == ValueType.Number)
      {
        return Value.MakeNumber(lhs.Number - rhs.Number);
      }
      throw new RuntimeException(string.Format(Properties.Resources.InvalidSubtraction, lhs, rhs));
    }

    public static Value Multiple(Value lhs, Value rhs)
    {
      if (rhs.Type == ValueType.Number)
      {
        switch (lhs.Type)
        {
          case ValueType.Number:
            return Value.MakeNumber(lhs.Number * rhs.Number);
        }
      }
      throw new RuntimeException(string.Format(Properties.Resources.InvalidMultiplication, lhs, rhs));
    }
    public static Value Devide(Value lhs, Value rhs)
    {
      if (rhs.Type == ValueType.Number)
      {
        if (rhs.Number == 0)
        {
          throw new RuntimeException(Properties.Resources.DevideByZero);
        }
        switch (lhs.Type)
        {
          case ValueType.Number:
            return Value.MakeNumber(lhs.Number / rhs.Number);
        }
      }
      throw new RuntimeException(string.Format(Properties.Resources.InvalidDivision, lhs, rhs));
    }

    public static Value Length(Value v)
      => Value.MakeNumber(
        v.Type switch
        {
          ValueType.String => v.String.Length,
          ValueType.Array => v.Array.Length,
          _ => throw new RuntimeException(string.Format(Properties.Resources.InvalidLength, v)),
        }
      );

    public static Value Slice(Value v, Value start, Value end)
    {
      if (!(v.Type == ValueType.String || v.Type == ValueType.Array))
      {
      }
      if (!(start.Type == ValueType.Number && end.Type == ValueType.Number))
      {
        throw new RuntimeException(Properties.Resources.SliceByNonNumber);
      }

      var i = Math.Clamp(
        start.Integer < 0
          ? Length(v).Integer + start.Integer
          : start.Integer,
        0, Length(v).Integer - 1);
      var j = Math.Clamp(
        end.Integer < 0
          ? Length(v).Integer + end.Integer + 1  // End はそれ自体を含まないので、-1 のときのインデックスは Length - 1 ではなく Length になる
          : end.Integer,
        0, Length(v).Integer);
      return v.Type switch
      {
        ValueType.String => Value.MakeString(v.String.Substring(i, Math.Max(0, j - i))),
        ValueType.Array => Value.MakeArray(v.Array.Span(i, j)),
        _ => throw new RuntimeException(string.Format(Properties.Resources.InvalidSlice, v)),
      };
    }
    public static Value Repeat(Value v, Value n)
    {
      if (!(v.Type == ValueType.String || v.Type == ValueType.Array))
      {
        throw new RuntimeException(string.Format(Properties.Resources.InvalidRepeat, v));
      }
      if (n.Type != ValueType.Number)
      {
        throw new RuntimeException(Properties.Resources.RepeatByNonNunmber);
      }
      if (n.Number == 0)
      {
        return v.Type == ValueType.String ? Value.MakeString("") : Value.MakeArray(new Array());
      }
      if (n.Number < 0)
      {
        return Repeat(Reverse(v), Value.MakeNumber(-n.Number));
      }

      var res = Enumerable.Range(0, (n.Integer) - 1).Aggregate(v, (sum, x) => Add(sum, v));
      if (!n.IsInteger)
      {
        res = Add(
          res,
          Slice(
            v,
            Value.MakeNumber(0),
            Multiple(Length(v), Subtract(n, Value.MakeNumber(n.Integer))))
          );
      }
      return res;
    }

    public static Value Reverse(Value v)
    {
      return v.Type switch
      {
        ValueType.String
          => Value.MakeString(string.Join("", v.String.ToCharArray().Reverse())),
        ValueType.Array
          => Value.MakeArray(new Array(v.Array.Reverse())),
        _ => throw new RuntimeException(string.Format(Properties.Resources.InvalidReverse, v)),
      };
    }


    public static Value MemberAccess(Value receiver, string name)
    {
      if (receiver.Type != ValueType.Object)
      {
        throw new RuntimeException(Properties.Resources.NonObjectMemberAccess);
      }
      return receiver.Object.Member(name);
    }

    public static Value FunctionCall(Value f, params Value[] args)
    {
      if (f.Type != ValueType.Function)
      {
        throw new RuntimeException(Properties.Resources.NonFunctionCalling);
      }
      return f.Function.Call(args);
    }

    public static Value InitArray(params Value[] items)
    {
      return Value.MakeArray(new Array(items));
    }

    private static void AssertTypeOfArrayAndIndex(Value v, Value index)
    {
      if (v.Type != ValueType.Array)
      {
        throw new RuntimeException(Properties.Resources.NonArrayIndexAccess);
      }
      if (!index.IsInteger)
      {
        throw new RuntimeException(Properties.Resources.NonIntArrayIndex);
      }
    }

    public static Value ArrayItem(Value v, Value index)
    {
      AssertTypeOfArrayAndIndex(v, index);
      return v.Array.Item((int)index.Number);
    }

    public static void SetArrayItem(Value v, Value index, Value e)
    {
      AssertTypeOfArrayAndIndex(v, index);
      v.Array.SetItem((int)index.Number, e);
    }
  }
}
