using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace NeeView.Text
{
    public class StringCollectionParser
    {

        private enum CharType
        {
            End,
            Splitter,
            DoubleQuat,
            Any,
        }


        private class Context
        {
            private readonly string _source;
            private readonly StringBuilder _work = new();
            private readonly List<string> _tokens = new();
            private int _index = -1;


            public Context(string source)
            {
                _source = source;
            }

            public string Source => _source;

            public List<string> Tokens => _tokens;


            public char GetCurrentChar()
            {
                if (_index < 0 || _source.Length <= _index)
                {
                    return '\0';
                }

                return _source[_index];
            }

            public void Next()
            {
                _index++;
            }

            public void Push()
            {
                var c = GetCurrentChar();
                if (c == '\0') throw new InvalidOperationException();
                _work.Append(c);
            }

            public void Take()
            {
                var s = _work.ToString().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    _tokens.Add(s);
                }
                _work.Clear();
            }
        }


        private class State
        {
            public State(Action<Context> action)
            {
                Action = action;
            }

            public Action<Context> Action { get; private set; }

            // NOTE: 状態遷移を構成する手順で最初はnullになってしまうため
            [AllowNull]
            public List<State> NextStates { get; set; }
        }


        private static readonly State s00 = new(StateAction_Next);
        private static readonly State s01 = new(StateAction_Take);
        private static readonly State s02 = new(StateAction_Next);
        private static readonly State s03 = new(StateAction_Push);
        private static readonly State s04 = new(StateAction_Take);
        private static readonly State s05 = new(StateAction_Next);
        private static readonly State s06 = new(StateAction_Push);
        private static readonly State s07 = new(StateAction_Next);
        private static readonly State err = new(StateAction_Error);
        private static readonly State eos = new(StateAction_None);

        static StringCollectionParser()
        {
            s00.NextStates = new List<State>() { s04, s01, s02, s03 };
            s01.NextStates = new List<State>() { s00, s00, s00, s00 };
            s02.NextStates = new List<State>() { err, s06, s05, s06 };
            s03.NextStates = new List<State>() { s07, s07, s07, s07 };
            s04.NextStates = new List<State>() { eos, eos, eos, eos };
            s05.NextStates = new List<State>() { s04, s01, s06, err };
            s06.NextStates = new List<State>() { s02, s02, s02, s02 };
            s07.NextStates = new List<State>() { s04, s01, err, s03 };
            err.NextStates = new List<State>() { eos, eos, eos, eos };
        }


        public static string Create(IEnumerable<string> items)
        {
            if (items is null) return "";

            return string.Join(";", items.Select(e => e.Contains(';') || e.Contains('"') ? ("\"" + e.Replace("\"", "\"\"") + "\"") : e));
        }

        public static List<string> Parse(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            var context = new Context(source);
            var state = s00;

            while (state != eos)
            {
                state.Action(context);
                state = state.NextStates[(int)GetCharType(context.GetCurrentChar())];
            }

            return context.Tokens;
        }

        private static CharType GetCharType(char c)
        {
            return c switch
            {
                '\0' => CharType.End,
                ';' => CharType.Splitter,
                '"' => CharType.DoubleQuat,
                _ => CharType.Any,
            };
        }

        private static void StateAction_None(Context context)
        {
        }

        private static void StateAction_Next(Context context)
        {
            context.Next();
        }

        private static void StateAction_Push(Context context)
        {
            context.Push();
        }

        private static void StateAction_Take(Context context)
        {
            context.Take();
        }

        private static void StateAction_Error(Context context)
        {
            throw new FormatException($"StringCollectionParser failed: \"{context.Source}\"");
        }

    }
}
