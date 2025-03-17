using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Parser
{
    public enum TokenType
    {
        Unknown = 0,
        Number = 1,            
        Identifier = 3,        
        Keyword = 4,          
        Assignment = 5,       
        Space = 6,            
        Semicolon = 7         
            
    }
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }

        public Token(TokenType type, string value, int start, int end)
        {
            Type = type;
            Value = value;
            StartPos = start;
            EndPos = end;
        }

        public override string ToString()
        {
            return $"{(int)Type} - {Type.ToString().ToLower()} - {Value} - с {StartPos + 1} по {EndPos + 1} символ";
        }
    }


    public class TextParser
    {

       
        private static readonly HashSet<string> Keywords = new HashSet<string> { "const", "double", "float" };

        public List<Token> Parse(string input)
        {
            List<Token> tokens = new List<Token>();
            int pos = 0;

            while (pos < input.Length)
            {
                char currentChar = input[pos];

                if (char.IsWhiteSpace(currentChar))
                {
                    int start = pos;
                    while (pos < input.Length && char.IsWhiteSpace(input[pos]))
                        pos++;
                    tokens.Add(new Token(TokenType.Space, " ", start, start));
                    continue;
                }

                if (char.IsLetter(currentChar))
                {
                    int start = pos;
                    while (pos < input.Length && (char.IsLetterOrDigit(input[pos]) || input[pos] == '_'))
                        pos++;
                    string word = input.Substring(start, pos - start);

                    if (Keywords.Contains(word))
                        tokens.Add(new Token(TokenType.Keyword, word, start, pos - 1));
                    else
                        tokens.Add(new Token(TokenType.Identifier, word, start, pos - 1));
                    continue;
                }

                if (char.IsDigit(currentChar) || currentChar == '+' || currentChar == '-' || currentChar == '.')
                {
                    int start = pos;
                    bool isValidNumber = false;
                    bool hasDot = false;
                    bool hasExp = false;
                    bool hasDigitsBeforeDot = false;
                    bool hasDigitsAfterDot = false;
                    bool hasDigitsAfterExp = false;

                    if (currentChar == '+' || currentChar == '-')
                    {
                        pos++;
                        if (pos >= input.Length || !char.IsDigit(input[pos]))
                        {
                            tokens.Add(new Token(TokenType.Unknown, input.Substring(start, pos - start), start, pos - 1));
                            continue;
                        }
                    }

                    if (pos < input.Length && char.IsDigit(input[pos]))
                    {
                        hasDigitsBeforeDot = true;
                        while (pos < input.Length && char.IsDigit(input[pos]))
                            pos++;
                    }

                    if (pos < input.Length && input[pos] == '.' && !hasExp)
                    {
                        hasDot = true;
                        pos++;
                        if (pos < input.Length && char.IsDigit(input[pos]))
                        {
                            hasDigitsAfterDot = true;
                            while (pos < input.Length && char.IsDigit(input[pos]))
                                pos++;
                        }
                    }

                    if (pos < input.Length && (input[pos] == 'e' || input[pos] == 'E') && hasDot)
                    {
                        hasExp = true;
                        pos++;
                        if (pos < input.Length && (input[pos] == '+' || input[pos] == '-'))
                            pos++;
                        if (pos < input.Length && char.IsDigit(input[pos]))
                        {
                            hasDigitsAfterExp = true;
                            while (pos < input.Length && char.IsDigit(input[pos]))
                                pos++;
                        }
                    }

                    string numberStr = input.Substring(start, pos - start);

                    if (hasDot && hasDigitsBeforeDot && hasDigitsAfterDot)
                        isValidNumber = true;
                    if (hasExp && hasDigitsAfterExp)
                        isValidNumber = true;

                    if (isValidNumber)
                        tokens.Add(new Token(TokenType.Number, numberStr, start, pos - 1));
                    else
                        tokens.Add(new Token(TokenType.Unknown, numberStr, start, pos - 1));
                    continue;
                }

                if (currentChar == '=')
                {
                    tokens.Add(new Token(TokenType.Assignment, "=", pos, pos));
                    pos++;
                    continue;
                }

                if (currentChar == ';')
                {
                    tokens.Add(new Token(TokenType.Semicolon, ";", pos, pos));
                    pos++;
                    continue;
                }

                tokens.Add(new Token(TokenType.Unknown, currentChar.ToString(), pos, pos));
                pos++;
            }

            return tokens;
        }

        public List<List<Token>> ParseMultipleLines(string[] lines)
        {
            List<List<Token>> result = new List<List<Token>>();

            foreach (string line in lines)
            {
                List<Token> tokens = Parse(line);
                result.Add(tokens);
            }

            return result;
        }
    }
}
