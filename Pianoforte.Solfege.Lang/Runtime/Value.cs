﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Pianoforte.Solfege.Lang.Runtime
{
  public enum ValueType
  {
    None,
    Bool,
    Number,
    String,
    Object,
    Array,
    Function,
  }

  public readonly struct Value : IEquatable<Value>
  {
    public ValueType Type { get; }

    private void AssertType(ValueType t)
    {
      if (Type != t)
      {
        throw new InternalAssertionException("Type mismatch");
      }
    }

    #region Properties

    private readonly bool _bool;
    public bool Bool
    {
      get
      {
        AssertType(ValueType.Bool);
        return _bool;
      }
    }

    private readonly decimal _number;
    public decimal Number
    {
      get
      {
        AssertType(ValueType.Number);
        return _number;
      }
    }

    public int Integer
    {
      get => decimal.ToInt32(Number);
    }

    private readonly string _string;
    public string String
    {
      get
      {
        AssertType(ValueType.String);
        return _string;
      }
    }

    private readonly Object _object;
    public Object Object
    {
      get
      {
        AssertType(ValueType.Object);
        return _object;
      }
    }

    private readonly Array _array;
    public Array Array
    {
      get
      {
        AssertType(ValueType.Array);
        return _array;
      }
    }
    private readonly Function _function;
    public Function Function
    {
      get
      {
        AssertType(ValueType.Function);
        return _function;
      }
    }

    #endregion

    public bool IsInteger
    {
      get => Type == ValueType.Number && decimal.Floor(Number) == Number;
    }

    #region Constructors
    public Value(ValueType type) : this(type, false, 0, "", null, null, null)
    {
    }

    public Value(ValueType type, bool b, decimal num, string str, Object obj, Array arr, Function func)
    {
      Type = type;
      _bool = b;
      _number = num;
      _string = str;
      _object = obj;
      _array = arr;
      _function = func;
    }
    #endregion

    #region FromXXX

    public static Value FromToken(Token token)
      =>
      MakeString(token.Text).ConvertType(token.Kind switch
      {
        TokenKind.NoneLiteral => ValueType.None,
        TokenKind.TrueLiteral => ValueType.Bool,
        TokenKind.FalseLiteral => ValueType.Bool,
        TokenKind.NumberLiteral => ValueType.Number,
        TokenKind.StringLiteral => ValueType.String,
        _ => throw new InternalAssertionException("Cannot convert RuntimeValue"),
      }
      );


    #endregion

    #region MakeXXX

    public static Value MakeDefault(ValueType type)
      => type switch
      {
        ValueType.None => MakeNone(),
        ValueType.Bool => MakeBool(false),
        ValueType.Number => MakeNumber(0),
        ValueType.String => MakeString(""),
        _ => throw new InternalAssertionException(string.Format("Cannot generate default of {0}", type)),
      };

    public static Value MakeNone()
      => new Value(ValueType.None);

    public static Value MakeBool(bool v)
      => new Value(ValueType.Bool, v, 0, "", null, null, null);

    public static Value MakeNumber(decimal v)
      => new Value(ValueType.Number, false, v, "", null, null, null);

    public static Value MakeInteger(int v) => MakeNumber((decimal)v);

    public static Value MakeString(string v)
      => new Value(ValueType.String, false, 0, v, null, null, null);

    public static Value MakeObject(Object v)
      => new Value(ValueType.Object, false, 0, "", v, null, null);
    public static Value MakeArray(Array v)
      => new Value(ValueType.Array, false, 0, "", null, v, null);


    public static Value MakeFunction(LambdaExpression lambda)
      => MakeFunction(new Function(lambda));
    public static Value MakeFunction(Func<FunctionArgs, Value> func, string name, params string[] argNames)
      => MakeFunction(new Function(func, name, argNames));

    public static Value MakeFunction(Function v)
      => new Value(ValueType.Function, false, 0, "", null, null, v);
    #endregion

    #region Converts

    public override string ToString() => Type switch
    {
      ValueType.None => Keywords.None.Text,
      ValueType.Bool => Bool ? Keywords.True.Text : Keywords.False.Text,
      ValueType.Number => Number.ToString(),
      ValueType.String => String,
      ValueType.Object => Object.ToString(),
      ValueType.Array => Array.ToString(),
      ValueType.Function => Function.ToString(),
       _ => throw new InternalAssertionException("Error")
    };

    public bool ToBool() => Type switch
    {
      ValueType.None => false,
      ValueType.Bool => Bool,
      ValueType.Number => Number != 0,
      ValueType.String => String.Length > 0,
      ValueType.Object => Object != null,
      ValueType.Array => Array.Length > 0,
      ValueType.Function => true,
       _ => throw new InternalAssertionException("Error")
    };

    public Value ConvertType(ValueType type)
    {
      if (Type == type)
      {
        return this;
      }
      var s = ToString();
      if (type == ValueType.None)
      {
        return MakeNone();
      }
      else if (type == ValueType.Bool && (s == Keywords.True.Text || s == Keywords.False.Text))
      {
        return MakeBool(s == Keywords.True.Text);
      }
      else if (type == ValueType.Number)
      {
        if (decimal.TryParse(s, out var n))
        {
          return MakeNumber(n);
        }
      }
      else if (type == ValueType.String)
      {
        return MakeString(s);
      }
      throw new RuntimeException(string.Format(Properties.Resources.CannotConvertType, s, type));
    }
    #endregion

    public Value DeepCopy()
      => Type switch
      {
        ValueType.Array => MakeArray(Array.DeepCopy()),
        ValueType.Object => MakeObject(Object.DeepCopy()),
        ValueType.Function => MakeFunction(Function), // TODO: It isn't deepcopy.
        ValueType.None => MakeNone(),
        ValueType.Bool => MakeBool(Bool),
        ValueType.Number => MakeNumber(Number),
        ValueType.String => MakeString(String),
      };

    #region Equals
    public override bool Equals(object obj)
    {
      return obj is Value value && Equals(value);
    }

    public bool Equals(Value other)
      => Type == other.Type && Type switch
      {
        ValueType.None => true,
        ValueType.Bool => Bool == other.Bool,
        ValueType.Number => Number == other.Number,
        ValueType.String => String == other.String,
        ValueType.Array => Array == other.Array,
        ValueType.Object =>
             EqualityComparer<Object>.Default.Equals(Object, other.Object),
        ValueType.Function =>
             EqualityComparer<Function>.Default.Equals(Function, other.Function),
       _ => throw new InternalAssertionException("Error")
      };


    public override int GetHashCode()
     => Type switch
     {
       ValueType.None => 0,
       ValueType.Bool => Bool.GetHashCode(),
       ValueType.Number => Number.GetHashCode(),
       ValueType.String => String.GetHashCode(),
       ValueType.Array => Array.GetHashCode(),
       ValueType.Object => Object.GetHashCode(),
       ValueType.Function => Function.GetHashCode(),
       _ => throw new InternalAssertionException("Error")
     } + Type.GetHashCode();


    public static bool operator ==(Value lhs, Value rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Value lhs, Value rhs)
    {
      return !(lhs == rhs);
    }

    #endregion
  }
}
