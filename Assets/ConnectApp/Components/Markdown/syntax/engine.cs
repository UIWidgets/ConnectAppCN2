using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.UIWidgets.painting;

namespace ConnectApp.Components.Markdown.syntax {
    public interface IEngine {
        TextSpan Highlight(Definition definition, string input);
    }

    public class Engine : IEngine {
        const RegexOptions DefaultRegexOptions = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;

        List<InlineSpan> textSpans;
        int processedLength;
        string source;

        public TextSpan Highlight(Definition definition, string input) {
            if (definition == null) {
                throw new ArgumentNullException("definition");
            }

            this.textSpans = new List<InlineSpan>();
            this.processedLength = 0;
            this.source = input;
            this.HighlightUsingRegex(definition: definition, input: input);
            if (this.processedLength < this.source.Length) {
                this.textSpans.Add(new TextSpan(this.source.Substring(startIndex: this.processedLength)));
            }

            return new TextSpan(
                children: this.textSpans,
                style: definition.Style.toTextStyle()
            );
        }

        void HighlightUsingRegex(Definition definition, string input) {
            var regexOptions = this.GetRegexOptions(definition: definition);
            var evaluator = this.GetMatchEvaluator(definition: definition);
            var regexPattern = definition.GetRegexPattern();
            Regex.Replace(input: input, pattern: regexPattern, evaluator: evaluator, options: regexOptions);
        }

        RegexOptions GetRegexOptions(Definition definition) {
            if (definition.CaseSensitive) {
                return DefaultRegexOptions | RegexOptions.IgnoreCase;
            }

            return DefaultRegexOptions;
        }

        string ElementMatchHandler(Definition definition, Match match) {
            if (definition == null) {
                throw new ArgumentNullException("definition");
            }

            if (match == null) {
                throw new ArgumentNullException("match");
            }

            var pattern = definition.Patterns.First(x => match.Groups[groupname: x.Key].Success).Value;
            if (match.Index > this.processedLength) {
                this.textSpans.Add(new TextSpan(this.source.Substring(startIndex: this.processedLength,
                    match.Index - this.processedLength)));
            }

            if (pattern != null) {
                if (pattern is BlockPattern blockPattern) {
                    this.textSpans.Add(this.ProcessBlockPatternMatch(definition: definition, pattern: blockPattern,
                        match: match));
                }
                else if (pattern is MarkupPattern markupPattern) {
                    this.textSpans.Add(this.ProcessMarkupPatternMatch(definition: definition, pattern: markupPattern,
                        match: match));
                }
                else if (pattern is WordPattern wordPattern) {
                    this.textSpans.Add(this.ProcessWordPatternMatch(definition: definition, pattern: wordPattern,
                        match: match));
                }
            }

            this.processedLength = match.Index + match.Length;

            return "";
        }

        MatchEvaluator GetMatchEvaluator(Definition definition) {
            return match => this.ElementMatchHandler(definition: definition, match: match);
        }

        protected TextSpan ProcessBlockPatternMatch(Definition definition, BlockPattern pattern, Match match) {
            return new TextSpan(text: match.Value, pattern.Style.toTextStyle());
        }

        protected TextSpan ProcessMarkupPatternMatch(Definition definition, MarkupPattern pattern, Match match) {
            return new TextSpan(text: match.Value, pattern.Style.toTextStyle());
        }

        protected TextSpan ProcessWordPatternMatch(Definition definition, WordPattern pattern, Match match) {
            return new TextSpan(text: match.Value, pattern.Style.toTextStyle());
        }
    }
}