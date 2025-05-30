using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Compiler.Parser
{
    public class EmailMatch
    {
        public int Index { get; }
        public int Length { get; }
        public string Value { get; }

        public EmailMatch(int index, int length, string value)
        {
            Index = index;
            Length = length;
            Value = value;
        }

        public override string ToString() => $"Match: '{Value}' at index {Index}, length {Length}";
    }
    public class RegexParser
    {
        private static readonly Regex RussianPhoneRegex = new Regex(@"(\d[ -]?){6}\d", RegexOptions.Compiled);
        private static readonly Regex Isbn13Regex = new Regex(@"(?:ISBN(?:-13)?:? )?(97[89])([ -]?)([0-9]{1,5})\2([0-9]{1,7})\2([0-9]{1,6})\2([0-9])", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);

        private enum State
        {
            q0, // Начальное состояние
            q1, // Локальная часть
            q2, // После @
            q3, // Доменная часть
            q4, // После точки
            q5, // Первая буква доменной зоны
            q6  // Вторая и последующие буквы доменной зоны (финальное)
        }

        // Поиск всех вхождений email в строке
        public static List<string> FindEmailMatches(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<string>();

            // Определяем допустимые наборы символов
            var localPartChars = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._%+-");
            var domainPartChars = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-");
            var domainZoneChars = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");

            var matches = new List<EmailMatch>();

            // Проверяем каждую позицию в строке как возможное начало email
            for (int startIndex = 0; startIndex < input.Length; startIndex++)
            {
                State currentState = State.q0;
                int currentIndex = startIndex;
                int lastValidIndex = -1; // Индекс последнего символа валидной подстроки

                // Обрабатываем символы, пока можем
                while (currentIndex < input.Length)
                {
                    char c = input[currentIndex];

                    switch (currentState)
                    {
                        case State.q0:
                            if (localPartChars.Contains(c))
                                currentState = State.q1;
                            else
                                goto EndOfMatch;
                            break;

                        case State.q1:
                            if (localPartChars.Contains(c))
                                currentState = State.q1;
                            else if (c == '@')
                                currentState = State.q2;
                            else
                                goto EndOfMatch;
                            break;

                        case State.q2:
                            if (domainPartChars.Contains(c))
                                currentState = State.q3;
                            else
                                goto EndOfMatch;
                            break;

                        case State.q3:
                            if (domainPartChars.Contains(c))
                                currentState = State.q3;
                            else if (c == '.')
                                currentState = State.q4;
                            else
                                goto EndOfMatch;
                            break;

                        case State.q4:
                            if (domainZoneChars.Contains(c))
                                currentState = State.q5;
                            else
                                goto EndOfMatch;
                            break;

                        case State.q5:
                            if (domainZoneChars.Contains(c))
                                currentState = State.q6;
                            else
                                goto EndOfMatch;
                            break;

                        case State.q6:
                            if (domainZoneChars.Contains(c))
                                currentState = State.q6;
                            else
                                goto EndOfMatch;
                            break;
                    }

                    // Если достигли финального состояния, обновляем индекс последней валидной позиции
                    if (currentState == State.q6)
                        lastValidIndex = currentIndex;

                    currentIndex++;
                }

            EndOfMatch:
                // Если была найдена валидная подстрока (достигли q6), добавляем её в результат
                if (lastValidIndex >= startIndex)
                {
                    int length = lastValidIndex - startIndex + 1;
                    string value = input.Substring(startIndex, length);
                    matches.Add(new EmailMatch(startIndex, length, value));
                    // Пропускаем символы до конца найденного email, чтобы избежать перекрывающихся совпадений
                    startIndex = lastValidIndex;
                }
            }
            List<string> result = new List<string>();
            foreach (var match in matches)
            {
                result.Add(match.ToString() + "\n");
            }
            return result;
        }


        public static string[] FindAndFormatMatches(string input, string patternType)
        {
            var results = new List<string>();

            // Проверяем все три regex на входной строке
            if(patternType == "Number") results.AddRange(FormatMatches(input, RussianPhoneRegex, "Номер"));
            if (patternType == "ISBN") results.AddRange(FormatMatches(input, Isbn13Regex, "ISBN-13"));
            if (patternType == "Email") results = FindEmailMatches(input);

            return results.ToArray();
        }

        private static IEnumerable<string> FormatMatches(string input, Regex regex, string patternType)
        {
            var matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                yield return $"[{patternType}] Найдено: '{match.Value}' на позиции {match.Index}\n";
            }
        }
    }

}
