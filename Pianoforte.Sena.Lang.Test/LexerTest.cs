using System;
using Xunit;
using System.IO;
using System.Text;
using System.Linq;

namespace Pianoforte.Sena.Lang.Test
{
  public class LexerTest
  {
    // { input, kind, text, line, column }[]
    public static object[][] lexData = {
      new object[] { "", new[] { new Token(TokenKind.EndOfFile, "", new TokenPosition("", 1, 1))} },
      new object[] { @"""abc"" ""\\\""""" + "\n", new[] {
        new Token(TokenKind.StringLiteral, "abc", new TokenPosition("", 1, 1)),
        new Token(TokenKind.StringLiteral, @"\""", new TokenPosition("", 1, 7)),
        new Token(TokenKind.EndOfLine, "\n", new TokenPosition("", 1, 13)),
        new Token(TokenKind.EndOfFile, "", new TokenPosition("", 2, 1)),
      }},
      new object[] { "none true false value\n", new[] {
        new Token(Keywords.None.TokenKind, Keywords.None.Text, new TokenPosition("", 1, 1)),
        new Token(Keywords.True.TokenKind, Keywords.True.Text, new TokenPosition("", 1, 6)),
        new Token(Keywords.False.TokenKind, Keywords.False.Text, new TokenPosition("", 1, 11)),
        new Token(TokenKind.Identifier, "value", new TokenPosition("", 1, 17)),
        new Token(TokenKind.EndOfLine, "\n", new TokenPosition("", 1, 22)),
        new Token(TokenKind.EndOfFile, "", new TokenPosition("", 2, 1)),
      }},
      new object[] { string.Join("",
        "=", "+", "-", "*", "/",
        "+=", "-=", "*=", "/=",
        "==", "!=", "<", ">", "<=", ">=",
        "(", ")", "[", "]",
        "\n"), new[] {
          new Token(TokenKind.OpAssignment, "=", new TokenPosition("", 1, 1)),
          new Token(TokenKind.OpPlus, "+", new TokenPosition("", 1, 2)),
          new Token(TokenKind.OpMinus, "-", new TokenPosition("", 1, 3)),
          new Token(TokenKind.OpMultiplication, "*", new TokenPosition("", 1, 4)),
          new Token(TokenKind.OpDivision, "/", new TokenPosition("", 1, 5)),
          new Token(TokenKind.OpPlusAssignment, "+=", new TokenPosition("", 1, 6)),
          new Token(TokenKind.OpMinusAssignment, "-=", new TokenPosition("", 1, 8)),
          new Token(TokenKind.OpMultiplicationAssignment, "*=", new TokenPosition("", 1, 10)),
          new Token(TokenKind.OpDivisionAssignment, "/=", new TokenPosition("", 1, 12)),
          new Token(TokenKind.OpEqual, "==", new TokenPosition("", 1, 14)),
          new Token(TokenKind.OpNotEqual, "!=", new TokenPosition("", 1, 16)),
          new Token(TokenKind.OpLessThan, "<", new TokenPosition("", 1, 18)),
          new Token(TokenKind.OpGreaterThan, ">", new TokenPosition("", 1, 19)),
          new Token(TokenKind.OpLessThanOrEqual, "<=", new TokenPosition("", 1,20)),
          new Token(TokenKind.OpGreaterThanOrEqual, ">=", new TokenPosition("", 1, 22)),
          new Token(TokenKind.ParenLeft, "(", new TokenPosition("", 1, 24)),
          new Token(TokenKind.ParenRight, ")", new TokenPosition("", 1, 25)),
          new Token(TokenKind.SquareBracketLeft, "[", new TokenPosition("", 1, 26)),
          new Token(TokenKind.SquareBracketRight, "]", new TokenPosition("", 1, 27)),
          new Token(TokenKind.EndOfLine, "\n", new TokenPosition("", 1, 28)),
          new Token(TokenKind.EndOfFile, "", new TokenPosition("", 2, 1)),
      }},
    };

    [Theory]
    [MemberData(nameof(lexData))]
    public void TestLex(string input, Token[] expectedTokens)
    {
      var lexer = new Lexer("", new MemoryStream(Encoding.UTF8.GetBytes(input)));
      var tokenList = Enumerable.Zip(expectedTokens, lexer.Lex(), (a, b) => new { expected = a, actual = b });
      foreach (var t in tokenList)
      {
        Assert.Equal(t.expected, t.actual);
      }
    }
  }
}