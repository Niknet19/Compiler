using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.NewParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum TokenType
    {
        Keyword,
        Identifier,
        Number,
        Operator,
        Delimiter,
        EOF,
        Error
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string value, int position, int line, int column)
        {
            Type = type;
            Value = value;
            Position = position;
            Line = line;
            Column = column;
        }

        public override string ToString() => $"{Type}: '{Value}' (pos {Position}, line {Line}, col {Column})";
    }

    public class Lexer
    {
        private readonly string input;
        private int position;
        private int currentLine;
        private int currentColumn;
        private List<string> lexerErrors;

        public Lexer(string input)
        {
            this.input = input;
            position = 0;
            currentLine = 1;
            currentColumn = 1;
            lexerErrors = new List<string>();
        }

        public (List<Token> Tokens, List<string> Errors) Tokenize()
        {
            var tokens = new List<Token>();
            Token token;

            do
            {
                token = NextToken();
                tokens.Add(token);
                if (token.Type == TokenType.Error)
                {
                    lexerErrors.Add($"Лексическая ошибка: недопустимый символ '{token.Value}' " +
                                   $"(строка {token.Line}, позиция {token.Column})");
                }
            } while (token.Type != TokenType.EOF);

            return (tokens, lexerErrors);
        }

        private Token NextToken()
        {
            SkipWhitespace();

            if (position >= input.Length)
                return new Token(TokenType.EOF, "", position, currentLine, currentColumn);

            char current = input[position];
            int startPos = position;
            int startLine = currentLine;
            int startCol = currentColumn;

            if (char.IsLetter(current))
            {
                string word = ReadWhile(c => char.IsLetterOrDigit(c));
                switch (word)
                {
                    case "begin":
                    case "end":
                    case "if":
                    case "then":
                    case "else":
                    case "while":
                    case "do":
                        return new Token(TokenType.Keyword, word, startPos, startLine, startCol);
                    default:
                        return new Token(TokenType.Identifier, word, startPos, startLine, startCol);
                }
            }

            if (char.IsDigit(current))
            {
                string number = ReadWhile(c => char.IsDigit(c));
                return new Token(TokenType.Number, number, startPos, startLine, startCol);
            }

            switch (current)
            {
                case ';':
                    Advance();
                    return new Token(TokenType.Delimiter, ";", startPos, startLine, startCol);
                case '(':
                    Advance();
                    return new Token(TokenType.Delimiter, "(", startPos, startLine, startCol);
                case ')':
                    Advance();
                    return new Token(TokenType.Delimiter, ")", startPos, startLine, startCol);
                case '+':
                    Advance();
                    return new Token(TokenType.Operator, "+", startPos, startLine, startCol);
                case '*':
                    Advance();
                    return new Token(TokenType.Operator, "*", startPos, startLine, startCol);
                case ':':
                    if (position + 1 < input.Length && input[position + 1] == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Operator, ":=", startPos, startLine, startCol);
                    }
                    break;
                case '=':
                    if (position + 1 < input.Length && input[position + 1] == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Operator, "==", startPos, startLine, startCol);
                    }
                    break;
                case '<':
                    if (position + 1 < input.Length && input[position + 1] == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Operator, "<=", startPos, startLine, startCol);
                    }
                    Advance();
                    return new Token(TokenType.Operator, "<", startPos, startLine, startCol);
                case '>':
                    if (position + 1 < input.Length && input[position + 1] == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Operator, ">=", startPos, startLine, startCol);
                    }
                    Advance();
                    return new Token(TokenType.Operator, ">", startPos, startLine, startCol);
                case '!':
                    if (position + 1 < input.Length && input[position + 1] == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Operator, "!=", startPos, startLine, startCol);
                    }
                    break;
            }


            string errorValue = input[position].ToString();
            Advance();
            return new Token(TokenType.Error, errorValue, startPos, startLine, startCol);
        }

        private string ReadWhile(Func<char, bool> predicate)
        {
            StringBuilder result = new StringBuilder();
            while (position < input.Length && predicate(input[position]))
            {
                result.Append(input[position]);
                Advance();
            }
            return result.ToString();
        }

        private void Advance(int steps = 1)
        {
            for (int i = 0; i < steps && position < input.Length; i++)
            {
                if (input[position] == '\n')
                {
                    currentLine++;
                    currentColumn = 1;
                }
                else
                {
                    currentColumn++;
                }
                position++;
            }
        }

        private void SkipWhitespace()
        {
            while (position < input.Length && char.IsWhiteSpace(input[position]))
            {
                Advance();
            }
        }
    }


    public class RecursiveDescentParser
    {
        private List<Token> tokens;
        private int currentTokenIndex;
        private List<string> callSequence;
        private int indentLevel;
        private List<string> errors;

        public RecursiveDescentParser(List<Token> tokens)
        {
            this.tokens = tokens;
            currentTokenIndex = 0;
            callSequence = new List<string>();
            indentLevel = 0;
            errors = new List<string>();
        }

        public (List<string> CallSequence, List<string> Errors) Parse()
        {
            try
            {
                AddLog("Начало синтаксического анализа");

                SkipErrorTokens();

                BeginStmt();

                while (currentTokenIndex < tokens.Count && CurrentToken.Type != TokenType.EOF)
                {
                    AddError($"Неожиданный токен {CurrentToken}");
                    currentTokenIndex++;
                    SkipErrorTokens();
                }

                AddLog("Синтаксический анализ завершен");
            }
            catch (Exception ex)
            {
                AddError($"Критическая ошибка: {ex.Message}");
            }
            return (callSequence, errors);
        }

        private Token CurrentToken
        {
            get
            {
                SkipErrorTokens();
                return currentTokenIndex < tokens.Count ?
                       tokens[currentTokenIndex] :
                       tokens.Last();
            }
        }

        private Token PeekToken(int ahead = 1)
        {
            int peekIndex = currentTokenIndex + ahead;
            while (peekIndex < tokens.Count && tokens[peekIndex].Type == TokenType.Error)
            {
                peekIndex++;
            }
            return peekIndex < tokens.Count ? tokens[peekIndex] : tokens[tokens.Count - 1];
        }

        private void AddLog(string message, bool isError = false)
        {
            var indent = new string(' ', indentLevel * 2);
            var formattedMessage = $"{indent}{(isError ? "!! " : "")}{message}";
            callSequence.Add(formattedMessage);
        }

        private void AddError(string message)
        {
            errors.Add($"Ошибка (позиция {CurrentToken.Position}): {message}");
            AddLog(message, true);
        }

        private void SkipErrorTokens()
        {
            while (currentTokenIndex < tokens.Count &&
                   tokens[currentTokenIndex].Type == TokenType.Error)
            {
                AddLog($"Пропущен ошибочный токен: {tokens[currentTokenIndex]}", true);
                currentTokenIndex++;
            }
        }

        private void Consume(TokenType expectedType, string expectedValue = null)
        {
            var current = CurrentToken; 

            if (current.Type == expectedType &&
               (expectedValue == null || current.Value == expectedValue))
            {
                AddLog($"Consumed: {current}");
                currentTokenIndex++;
                SkipErrorTokens(); 
            }
            else
            {
                string expectation = expectedValue ?? expectedType.ToString();
                AddError($"Ожидается {expectation}, но получено {current}");
                currentTokenIndex++;
            }
        }

        private bool Match(TokenType type, string value = null)
        {
            if (CurrentToken.Type == type && (value == null || CurrentToken.Value == value))
            {
                Consume(type, value);
                return true;
            }
            return false;
        }

        private void BeginStmt()
        {
            AddLog("BeginStmt");
            indentLevel++;

            if (!Match(TokenType.Keyword, "begin"))
            {
                AddError("Отсутствует ключевое слово 'begin'");
                while (currentTokenIndex < tokens.Count &&
                      !(CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "begin"))
                {
                    currentTokenIndex++;
                }
                if (currentTokenIndex >= tokens.Count) return;
            }

            StmtList();

            if (!Match(TokenType.Keyword, "end"))
            {
                AddError("Отсутствует ключевое слово 'end'");
                while (currentTokenIndex < tokens.Count &&
                      !(CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "end"))
                {
                    currentTokenIndex++;
                }
                if (currentTokenIndex < tokens.Count) currentTokenIndex++;
            }

            indentLevel--;
        }

        private void StmtList()
        {
            AddLog("StmtList");
            indentLevel++;

            Stmt();

            while (Match(TokenType.Delimiter, ";"))
            {
                Stmt();
            }

            indentLevel--;
        }

        private void Stmt()
        {
            AddLog("Stmt");
            indentLevel++;

            if (CurrentToken.Type == TokenType.Keyword)
            {
                switch (CurrentToken.Value)
                {
                    case "if":
                        IfStmt();
                        break;
                    case "while":
                        WhileStmt();
                        break;
                    case "begin":
                        BeginStmt();
                        break;
                    default:
                        AddError($"Неожиданное ключевое слово: {CurrentToken.Value}");
                        currentTokenIndex++;
                        break;
                }
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                AssgStmt();
            }
            else
            {
                AddError($"Ожидается оператор, но получено {CurrentToken}");
                currentTokenIndex++;
            }

            indentLevel--;
        }

        private void IfStmt()
        {
            AddLog("IfStmt");
            indentLevel++;

            Consume(TokenType.Keyword, "if");
            BoolExpr();

            if (!Match(TokenType.Keyword, "then"))
            {
                AddError("Отсутствует ключевое слово 'then'");
            }

            Stmt();

            if (!Match(TokenType.Keyword, "else"))
            {
                AddError("Отсутствует ключевое слово 'else'");
            }

            Stmt();

            indentLevel--;
        }

        private void WhileStmt()
        {
            AddLog("WhileStmt");
            indentLevel++;

            Consume(TokenType.Keyword, "while");
            BoolExpr();

            if (!Match(TokenType.Keyword, "do"))
            {
                AddError("Отсутствует ключевое слово 'do'");
            }

            Stmt();

            indentLevel--;
        }

        private void AssgStmt()
        {
            AddLog("AssgStmt");
            indentLevel++;

            if (CurrentToken.Type != TokenType.Identifier)
            {
                AddError($"Ожидается идентификатор, но получено {CurrentToken}");
                return;
            }

            Consume(TokenType.Identifier);

            if (!Match(TokenType.Operator, ":="))
            {
                AddError("Отсутствует оператор присваивания ':='");
            }

            ArithExpr();

            indentLevel--;
        }

        private void BoolExpr()
        {
            AddLog("BoolExpr");
            indentLevel++;

            ArithExpr();

            if (CurrentToken.Type != TokenType.Operator ||
                !(CurrentToken.Value == "==" || CurrentToken.Value == "<" ||
                  CurrentToken.Value == "<=" || CurrentToken.Value == ">" ||
                  CurrentToken.Value == ">=" || CurrentToken.Value == "!="))
            {
                AddError($"Ожидается оператор сравнения, но получено {CurrentToken}");
                while (currentTokenIndex < tokens.Count &&
                      !(CurrentToken.Type == TokenType.Keyword &&
                        (CurrentToken.Value == "then" || CurrentToken.Value == "do")))
                {
                    currentTokenIndex++;
                }
            }
            else
            {
                Consume(TokenType.Operator);
            }

            ArithExpr();

            indentLevel--;
        }

        private void ArithExpr()
        {
            AddLog("ArithExpr");
            indentLevel++;

            if (Match(TokenType.Delimiter, "("))
            {
                ArithExpr();

                if (!Match(TokenType.Delimiter, ")"))
                {
                    AddError("Отсутствует закрывающая скобка ')'");
                }
            }
            else if (CurrentToken.Type == TokenType.Number)
            {
                Consume(TokenType.Number);
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                Consume(TokenType.Identifier);
            }
            else
            {
                AddError($"Ожидается арифметическое выражение, но получено {CurrentToken}");
                currentTokenIndex++;
            }

            while (CurrentToken.Type == TokenType.Operator &&
                  (CurrentToken.Value == "+" || CurrentToken.Value == "*"))
            {
                string op = CurrentToken.Value;
                Consume(TokenType.Operator);
                AddLog($"Operator: {op}");
                ArithExpr();
            }

            indentLevel--;
        }
    }

}
