using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionParser
{
    public class Error
    {
        public string Type { get; }
        public string Message { get; }
        public int Position { get; }

        public Error(string type, string message, int position)
        {
            Type = type;
            Message = message;
            Position = position;
        }

        public override string ToString() => $"{Type}: {Message} в позиции {Position}";
    }

    public class Token
    {
        public string Type { get; }
        public string Value { get; }
        public int Position { get; }

        public Token(string type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString() => $"({Type}, {Value}, {Position})";
    }

    public class Quadruple
    {
        public string Operator { get; }
        public string Arg1 { get; }
        public string Arg2 { get; }
        public string Result { get; }

        public Quadruple(string op, string arg1, string arg2, string result)
        {
            Operator = op;
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }

        public override string ToString() => $"({Operator}, {Arg1}, {Arg2}, {Result})";
    }

    public class Lexer
    {
        private readonly string _input;
        private int _pos;
        private readonly List<Token> _tokens;
        private readonly List<Error> _errors;

        public Lexer(string input)
        {
            _input = input;
            _pos = 0;
            _tokens = new List<Token>();
            _errors = new List<Error>();
            Tokenize();
        }

        public List<Error> Errors => _errors;

        private void Tokenize()
        {
            while (_pos < _input.Length)
            {
                char current = _input[_pos];
                if (char.IsWhiteSpace(current))
                {
                    _pos++;
                    continue;
                }
                if (char.IsLetter(current))
                {
                    string id = "";
                    StringBuilder invalidChars = new StringBuilder();
                    int startPos = _pos;
                    int invalidStartPos = -1;
                    bool hasInvalid = false;

                    while (_pos < _input.Length && !char.IsWhiteSpace(_input[_pos]) && !"+-*/()=".Contains(_input[_pos]))
                    {
                        current = _input[_pos];
                        if (char.IsLetter(current))
                        {
                            if (invalidChars.Length > 0)
                            {
                                _errors.Add(new Error("Лексическая ошибка",
                                                     $"недопустимый символ '{invalidChars.ToString()}'",
                                                     invalidStartPos));
                                invalidChars.Clear();
                                hasInvalid = true;
                            }
                            id += current;
                        }
                        else
                        {
                            if (invalidChars.Length == 0)
                                invalidStartPos = _pos;
                            invalidChars.Append(current);
                        }
                        _pos++;
                    }

                    if (invalidChars.Length > 0)
                    {
                        _errors.Add(new Error("Лексическая ошибка",
                                             $"недопустимый символ '{invalidChars.ToString()}'",
                                             invalidStartPos));
                        hasInvalid = true;
                    }

                    if (id.Length > 0)
                    {
                        _tokens.Add(new Token("ID", id, startPos));
                    }
                    else if (!hasInvalid)
                    {
                        _pos++;
                    }
                    continue;
                }
                if ("+-*/()=".Contains(current))
                {
                    _tokens.Add(new Token(current.ToString(), current.ToString(), _pos));
                    _pos++;
                    continue;
                }
                StringBuilder invalidSeq = new StringBuilder();
                int invalidSeqStart = _pos;
                while (_pos < _input.Length && !char.IsWhiteSpace(_input[_pos]) &&
                       !char.IsLetter(_input[_pos]) && !"+-*/()=".Contains(_input[_pos]))
                {
                    invalidSeq.Append(_input[_pos]);
                    _pos++;
                }
                _errors.Add(new Error("Лексическая ошибка",
                                     $"недопустимый символ '{invalidSeq.ToString()}'",
                                     invalidSeqStart));
            }
            _tokens.Add(new Token("EOF", "", _pos));
        }

        public Token GetNextToken()
        {
            if (_tokens.Count > 0)
            {
                var token = _tokens[0];
                _tokens.RemoveAt(0);
                return token;
            }
            return new Token("EOF", "", _pos);
        }
    }


    public class ExpressionParser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;
        private int _tempCount;
        private readonly List<Quadruple> _quadruples;
        private readonly List<Error> _errors;

        public ExpressionParser(string input)
        {
            _lexer = new Lexer(input);
            _currentToken = _lexer.GetNextToken();
            _tempCount = 1;
            _quadruples = new List<Quadruple>();
            _errors = new List<Error>(_lexer.Errors);
        }

        public List<Quadruple> Quadruples => _quadruples;
        public List<Error> Errors => _errors;

        private void AddError(string expected)
        {
            _errors.Add(new Error("Синтаксическая ошибка",
                                 $"ожидался {expected}, найдено {_currentToken.Type}",
                                 _currentToken.Position));
        }

        private void Consume(string tokenType)
        {
            if (_currentToken.Type == tokenType)
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                AddError(tokenType);
                if ("+-*/()=".Contains(_currentToken.Type))
                {
                    _currentToken = _lexer.GetNextToken();
                }
                while (_currentToken.Type != "EOF" && _currentToken.Type != "ID" &&
                       _currentToken.Type != "(")
                {
                    _currentToken = _lexer.GetNextToken();
                }
            }
        }

        private (string Id, string Expr)? S()
        {
            // S → id = E
            if (_currentToken.Type == "ID")
            {
                string id = _currentToken.Value;
                Consume("ID");
                if (_currentToken.Type == "=")
                {
                    Consume("=");
                    string expr = E();
                    if (expr != null)
                    {
                        _quadruples.Add(new Quadruple("=", expr, "_", id));
                        return (id, expr);
                    }
                }
                else
                {
                    AddError("'='");
                }
            }
            else
            {
                AddError("идентификатор");
                while (_currentToken.Type != "EOF" && _currentToken.Type != "=")
                {
                    _currentToken = _lexer.GetNextToken();
                }
                if (_currentToken.Type == "=")
                {
                    Consume("=");
                    string expr = E();
                    if (expr != null)
                    {
                        return (null, expr);
                    }
                }
            }
            return null;
        }

        private string E()
        {
            string result = T();
            if (result != null)
            {
                result = A(result);
            }
            return result;
        }

        private string A(string left)
        {
            if (_currentToken.Type == "+" || _currentToken.Type == "-")
            {
                string op = _currentToken.Type;
                Consume(_currentToken.Type);
                string right = T();
                if (right != null && left != null)
                {
                    string temp = $"t{_tempCount++}";
                    _quadruples.Add(new Quadruple(op, left, right, temp));
                    return A(temp);
                }
            }
            return left;
        }

        private string T()
        {
            string result = F();
            if (result != null)
            {
                result = B(result);
            }
            return result;
        }

        private string B(string left)
        {
            if (_currentToken.Type == "*" || _currentToken.Type == "/")
            {
                string op = _currentToken.Type;
                Consume(_currentToken.Type);
                string right = F();
                if (right != null && left != null)
                {
                    string temp = $"t{_tempCount++}";
                    _quadruples.Add(new Quadruple(op, left, right, temp));
                    return B(temp);
                }
            }
            return left;
        }

        private string F()
        {
            if (_currentToken.Type == "-")
            {
                Consume("-");
                string operand = O();
                if (operand != null)
                {
                    string temp = $"t{_tempCount++}";
                    _quadruples.Add(new Quadruple("neg", operand, "_", temp));
                    return temp;
                }
                return null;
            }
            return O();
        }

        private string O()
        {
            if (_currentToken.Type == "ID")
            {
                string idValue = _currentToken.Value;
                Consume("ID");
                return idValue;
            }
            if (_currentToken.Type == "(")
            {
                Consume("(");
                string result = E();
                if (_currentToken.Type == ")")
                {
                    Consume(")");
                    return result;
                }
                else
                {
                    AddError("')'");
                    return null;
                }
            }
            AddError("идентификатор или '('");
            return null;
        }

        public (string Id, string Expr, List<Quadruple> Quadruples, List<Error> Errors) Parse()
        {
            var result = S();
            while (_currentToken.Type != "EOF" && _errors.Count > 0)
            {
                if (_currentToken.Type == "ID" || _currentToken.Type == "(")
                {
                    string expr = E();
                    if (expr != null)
                    {
                        result = (result?.Id, expr);
                    }
                }
                else
                {
                    _currentToken = _lexer.GetNextToken();
                }
            }
            if (_currentToken.Type != "EOF" && _errors.Count == 0)
            {
                AddError("конец ввода");
            }
            return (result?.Id, result?.Expr, _quadruples, _errors);
        }


    }
}