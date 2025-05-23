﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Compiler.Parser
{
    public enum TokenType
    {
        Unknown = 0,
        Integer = 1,
        Identifier = 3,
        Keyword = 4,
        Assignment = 5,
        Space = 6,
        Semicolon = 7,
        Real = 8,
        Plus = 9,
        Minus = 10,
        Dot = 11,
        Null,

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
        private List<string> errors = new List<string>();
        private int tokenIndex;
        private List<Token> tokens;
        private string inputstring;

        public static bool isKeyword(string token)
        {
            return Keywords.Contains(token);
        }
        private bool IsLetter(char c)
        {
            if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z') return true;
            return false;
        }

        public List<Token> Parse(string input)
        {
            inputstring = input;
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

                if (IsLetter(currentChar))
                {
                    int start = pos;
                    while (pos < input.Length && (char.IsLetter(input[pos])))
                        pos++;
                    string word = input.Substring(start, pos - start);

                    if (Keywords.Contains(word))
                        tokens.Add(new Token(TokenType.Keyword, word, start, pos - 1));
                    else
                        tokens.Add(new Token(TokenType.Identifier, word, start, pos - 1));
                    continue;
                }

                if (char.IsDigit(currentChar))
                {
                    int start = pos;
                    if (pos < input.Length && char.IsDigit(input[pos]))
                    {
                        while (pos < input.Length && char.IsDigit(input[pos]))
                            pos++;
                    }
                    string numberStr = input.Substring(start, pos - start);
                    tokens.Add(new Token(TokenType.Integer, numberStr, start, pos - 1));
                    continue;
                }
                if (currentChar == '+')
                {
                    tokens.Add(new Token(TokenType.Plus, "+", pos, pos));
                    pos++;
                    continue;
                }

                if (currentChar == '.')
                {
                    tokens.Add(new Token(TokenType.Dot, ".", pos, pos));
                    pos++;
                    continue;
                }

                if (currentChar == '-')
                {
                    tokens.Add(new Token(TokenType.Minus, "-", pos, pos));
                    pos++;
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
                if (tokens.Count > 0 && tokens.Last().Type == TokenType.Unknown)
                {
                    tokens.Last().Value += currentChar.ToString();
                    tokens.Last().EndPos = pos;
                }
                else
                    tokens.Add(new Token(TokenType.Unknown, currentChar.ToString(), pos, pos));
                pos++;
            }

            return tokens;
        }


        private int FindNextToken(TokenType findType, TokenType before = TokenType.Null)
        {
            int currentindex = tokenIndex;
            while (currentindex < tokens.Count)
            {
                if (tokens[currentindex].Type == before) break;
                if (tokens[currentindex].Type == findType) return currentindex;
                currentindex++;
            }
            return -1;
        }


        public void SyntaxAnalyze(List<Token> tokenList)
        {
            tokens = tokenList;
            tokenIndex = 0;
            errors.Clear();
            try
            {
                ParseConst();

            }
            catch (Exception ex)
            {

            }
        }

        private void ParseConst()
        {
            string errorstring = string.Empty;
            string token = string.Empty;
            if (tokens[tokenIndex].Type == TokenType.Space) tokenIndex++;
            while (tokens[tokenIndex].Type != TokenType.Space && tokens[tokenIndex].Type != TokenType.Assignment)
            {
                while (tokens[tokenIndex].Type != TokenType.Integer && tokens[tokenIndex].Type != TokenType.Identifier && tokens[tokenIndex].Type != TokenType.Keyword
                    && tokens[tokenIndex].Type != TokenType.Space && tokens[tokenIndex].Type != TokenType.Assignment)
                {
                    errorstring += tokens[tokenIndex].Value;
                    tokenIndex++;
                }
                if (errorstring != string.Empty)
                {
                    errors.Add($"Неожиданный символ, отброшено {errorstring}");
                }
                errorstring = "";
                if (tokens[tokenIndex].Type == TokenType.Identifier || tokens[tokenIndex].Type == TokenType.Integer
                    || tokens[tokenIndex].Type == TokenType.Keyword)
                {
                    token += tokens[tokenIndex].Value;
                    tokenIndex++;
                }

            }

            if (token == "float" || token == "double")
            {
                errors.Clear();
                errors.Add("Пропущено ключевое слово const");
                //errors.Add("Пропущен пробел после const");
                tokenIndex = 0;
                ParseSpaceAfterConst();
                //ParseSpaceAfterType();
            }
            else
            if (token != "const")
            {
                if (FindNextToken(TokenType.Identifier, TokenType.Assignment) != -1)
                {
                    errors.Add($"Ожидали const получили {token}");
                    ParseSpaceAfterConst();
                }
                else
                {
                    errors.Clear();
                    errors.Add("Пропущено ключевое слово const");
                    errors.Add("Пропущен пробел после const");
                    errors.Add("Пропущен тип");
                    errors.Add("Пропущен пробел после типа");
                    tokenIndex = 0;

                    ParseId();
                }

            }
            else
            {
                ParseSpaceAfterConst();
            }


        }
        private void ParseSpaceAfterConst()
        {
            if (tokens[tokenIndex].Type != TokenType.Space)
            {
                errors.Add("Пропущен пробел после const");
            }
            else tokenIndex++;
            ParseTypename();
        }

        private void ParseTypename()
        {
            //int startindex = tokenIndex;
            var starterrors = errors;
            string errorstring = string.Empty;
            string token = string.Empty;
            int startindex = tokenIndex;

            while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Space && tokens[tokenIndex].Type != TokenType.Assignment)
            {
                if (tokens[tokenIndex].Type != TokenType.Identifier && tokens[tokenIndex].Type != TokenType.Keyword &&
                    tokens[tokenIndex].Type != TokenType.Space && tokens[tokenIndex].Type != TokenType.Integer
                    && tokens[tokenIndex].Type != TokenType.Assignment)
                {
                    errorstring += tokens[tokenIndex].Value;
                    tokenIndex++;
                }
                if (errorstring != string.Empty)
                {
                    errors.Add($"Неожиданный символ, отброшено {errorstring}");
                }
                errorstring = "";
                if (tokens[tokenIndex].Type == TokenType.Identifier || tokens[tokenIndex].Type == TokenType.Keyword
                    || tokens[tokenIndex].Type == TokenType.Integer)
                {
                    token += tokens[tokenIndex].Value;
                    tokenIndex++;
                }
            }
            if (token != "float" && token != "double")
            {
                if (FindNextToken(TokenType.Identifier, TokenType.Assignment) != -1)
                {
                    errors.Add($"Ожидали double или float получили {token}");
                    ParseSpaceAfterType();

                }
                else
                {
                    errors = starterrors;
                    errors.Add("Пропущен float или double");
                    tokenIndex = startindex;

                    ParseSpaceAfterType();
                }
            }
            else
            {
                ParseSpaceAfterType();
            }
        }

        private void ParseSpaceAfterType()
        {
            
            if (tokenIndex >= tokens.Count || tokens[tokenIndex].Type != TokenType.Space)
            {
                errors.Add("Пропущен пробел после имени типа");
            }
            else tokenIndex++;
            //tokenIndex++;
            ParseId();
            //}
        }

        private void ParseId()
        {
            string errorstring = string.Empty;
            string token = string.Empty;
            bool idStarted = false;
            while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Assignment && tokens[tokenIndex].Type != TokenType.Space)
            {
                while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Identifier &&
                    (tokens[tokenIndex].Type != TokenType.Integer || !idStarted)
                    && tokens[tokenIndex].Type != TokenType.Keyword && tokens[tokenIndex].Type != TokenType.Assignment &&
                    tokens[tokenIndex].Type != TokenType.Space)
                {
                    errorstring += tokens[tokenIndex].Value;
                    tokenIndex++;
                }

                if (errorstring != string.Empty)
                {
                    errors.Add($"Неожиданный символ, отброшено {errorstring}");
                }
                errorstring = "";
                if (tokens[tokenIndex].Type == TokenType.Identifier || tokens[tokenIndex].Type == TokenType.Integer
                    || tokens[tokenIndex].Type == TokenType.Keyword)
                {
                    idStarted = true;
                    token += tokens[tokenIndex].Value;
                    tokenIndex++;
                }

            }
            if (token == "")
            {
                errors.Add("Пропущен идентификатор");
                ParseEqual();
            }
            else if (isKeyword(token))
            {
                errors.Add($"Ожидали идентификатор получили ключевое слово {token}");
                ParseEqual();
            }
            else
            {
                ParseEqual();
            }
        }


        private void ParseEqual()
        {
            string errorstring = string.Empty;

            while (tokenIndex < tokens.Count && tokens[tokenIndex].Type == TokenType.Space) tokenIndex++;
            if (tokenIndex < tokens.Count && tokens[tokenIndex].Type == TokenType.Assignment)
            {
                tokenIndex++;
                ParseNumber();
            }
            else
            {
                while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Integer && tokens[tokenIndex].Type != TokenType.Semicolon
                    && tokens[tokenIndex].Type != TokenType.Assignment && tokens[tokenIndex].Type != TokenType.Plus && 
                    tokens[tokenIndex].Type != TokenType.Minus)
                {
                    errorstring += tokens[tokenIndex].Value;
                    tokenIndex++;
                }
                if (errorstring != string.Empty)
                {
                    errors.Add($"Неожиданный символ, отброшено {errorstring}");
                }
                if (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Assignment || tokenIndex >= tokens.Count)
                {
                    errors.Add($"Пропущен оператор =");
                }
                else tokenIndex++;
                ParseNumber();

            }


        }


        private void ParseNumber()
        {

            string errorstring = string.Empty;
            string token = string.Empty;
            bool numberStarted = false;
            bool hasSign = false;
            bool hasDot = false;
            bool hasNumbersAfterDot = false;
            while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Semicolon)
            {
                if (tokens[tokenIndex].Type == TokenType.Space && numberStarted) break;
                while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.Integer &&
                (tokens[tokenIndex].Type != TokenType.Dot || hasDot || !numberStarted) &&
                ((tokens[tokenIndex].Type != TokenType.Plus && tokens[tokenIndex].Type != TokenType.Minus)
                || hasSign)
                 && tokens[tokenIndex].Type != TokenType.Semicolon)
                {
                    if (tokens[tokenIndex].Type == TokenType.Space)
                    {
                        if (numberStarted) break;
                        else
                        {
                            tokenIndex++;
                            continue;
                        }
                    }
                    errorstring += tokens[tokenIndex].Value;
                    tokenIndex++;
                }
                if (errorstring != string.Empty)
                {
                    errors.Add($"Неожиданный символ, отброшено {errorstring}");
                }
                errorstring = "";


                if (tokenIndex < tokens.Count && (tokens[tokenIndex].Type == TokenType.Plus ||
                    tokens[tokenIndex].Type == TokenType.Minus) && !hasSign)
                {
                    token += tokens[tokenIndex].Value;
                    hasSign = true;
                    tokenIndex++;
                }

                if (tokenIndex < tokens.Count && tokens[tokenIndex].Type == TokenType.Integer)
                {
                    if (hasDot) hasNumbersAfterDot = true;
                    token += tokens[tokenIndex].Value;
                    numberStarted = true;
                    tokenIndex++;
                }

                if (tokenIndex < tokens.Count && tokens[tokenIndex].Type == TokenType.Dot)
                {
                    if (!hasDot)
                    {
                        hasDot = true;
                        token += tokens[tokenIndex].Value;
                        tokenIndex++;
                    }

                }
            }
            if (token == ""  || !numberStarted)
            {
                errors.Add("Пропущено число");

            }
            else if (hasDot && !hasNumbersAfterDot)
            {
                errors.Add("Неверно заданное число. Отсутствуют числа после .");
            }
            ParseSemicolon();


        }


        private void ParseSemicolon()
        {
            bool semicolonFound = false;
            string errorstring = string.Empty;

            while (tokenIndex < tokens.Count)
            {
                var currentToken = tokens[tokenIndex];

                if (currentToken.Type == TokenType.Semicolon)
                {
                    semicolonFound = true;
                    tokenIndex++;
                    break;
                }

                if (currentToken.Type != TokenType.Space)
                {
                    errorstring+=currentToken.Value;
                }

                tokenIndex++;
            }

            if (errorstring.Length > 0)
            {
                errors.Add($"Неожиданный символы, отброшено: {errorstring}");
            }

            if (!semicolonFound)
            {
                errors.Add("Не найден символ ;");
            }

        }


        public List<string> GetErrors()
        {
            return errors;
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
