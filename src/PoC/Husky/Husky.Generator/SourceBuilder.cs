using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Husky.Generator
{
    public sealed class SourceBuilder
    {
        private sealed class DisposableAction : IDisposable
        {
            private readonly Action _action;

            public DisposableAction(Action action)
                => _action = action;

            public void Dispose()
                => _action();
        }

        private enum LastAction
        {
            None,
            BlockClose,
        }

        private readonly StringBuilder _sb;
        private int _indent;
        private LastAction _lastAction;

        public SourceBuilder(StringBuilder? sb = null)
            => _sb ??= new();

        public IDisposable Block(string? line = null)
        {
            BlockOpen(line);
            return new DisposableAction(() => BlockClose());
        }

        public SourceBuilder BlockClose(bool appendLine = true)
        {
            IndentDown();
            if (appendLine)
                AppendIndentedLine("}");
            else
                AppendIndented("}");

            return this;
        }

        public SourceBuilder BlockOpen(string? line = null)
        {
            AddBlankLineIfNeeded();
            if (line is not null)
                AppendIndentedLine(line);
            return AppendIndentedLine("{").IndentUp();
        }

        public SourceBuilder DelimitedLines(string delimiter, params string[] lines)
            => DelimitedLines(delimiter, lines as IReadOnlyList<string>);

        public SourceBuilder DelimitedLines(string delimiter, IEnumerable<string> lines)
            => DelimitedLines(delimiter, lines.ToList());

        public SourceBuilder DelimitedLines(string delimiter, IReadOnlyList<string> lines)
        {
            AddBlankLineIfNeeded();
            for (var i = 0; i < lines.Count; ++i)
            {
                AppendIndented(lines[i]);
                    Append(delimiter);
                AppendNewLine();
            }
            return this;
        }

        public SourceBuilder IndentDown()
        {
            --_indent;
            return this;
        }

        public SourceBuilder IndentUp()
        {
            ++_indent;
            return this;
        }

        public SourceBuilder InlineText(string text) => Append(text);

        public SourceBuilder Text(string text) => AppendIndented(text);

        public SourceBuilder Line() => AppendNewLine();

        public SourceBuilder Line(string line) => AppendIndentedLine(line);

        public SourceBuilder Lines(params string[] lines) => DelimitedLines("", lines);

        public SourceBuilder Lines(IEnumerable<string> lines) => DelimitedLines("", lines.ToList());

        public IDisposable Parens(string line, string? postfix = null)
        {
            AddBlankLineIfNeeded();
            AppendIndented(line).Append("(").AppendNewLine().IndentUp();

            return new DisposableAction(() => IndentDown().AppendIndented(")").Append(postfix).AppendNewLine());
        }

        public IDisposable InlineParens(string? postfix = null)
        {
            Append("(");
            return new DisposableAction(() => Append(")").Append(postfix));
        }

        public IDisposable InlineParensIndentedEnd(string? postfix = null)
        {
            Append("(");
            return new DisposableAction(() => AppendIndented(")").Append(postfix));
        }

        public override string ToString()
            => _sb.ToString();

        private SourceBuilder AddBlankLineIfNeeded()
        {
            if (_lastAction != LastAction.BlockClose)
                return this;

            _lastAction = LastAction.None;
            return AppendNewLine();
        }

        private SourceBuilder Append(string? text)
        {
            if (text is not null)
                _sb.Append(text);

            return this;
        }

        private SourceBuilder AppendIndent()
        {
            _sb.Append(' ', _indent * 4);

            return this;
        }

        private SourceBuilder AppendIndented(string text)
            => AppendIndent().Append(text);

        private SourceBuilder AppendIndentedLine(string text)
            => AppendIndent().Append(text).AppendNewLine();

        private SourceBuilder AppendNewLine()
        {
            _sb.AppendLine();
            return this;
        }
    }
}