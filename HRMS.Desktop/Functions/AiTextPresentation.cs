using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QLyNSu.Functions
{
    public sealed class AiTypingTimeline
    {
        private readonly string _text;
        private readonly int[] _wordEnds;
        public double DurationMs { get; }
        public int WordCount => _wordEnds.Length;

        public AiTypingTimeline(string text)
        {
            _text = text ?? string.Empty;
            _wordEnds = Regex.Matches(_text, @"\S+\s*").Cast<Match>()
                .Select(match => match.Index + match.Length).ToArray();
            DurationMs = Math.Min(6000, Math.Max(45, 45.0 * WordCount / (1 + WordCount / 180.0)));
        }

        public int VisibleLength(double elapsedMs)
        {
            if (WordCount == 0 || elapsedMs >= DurationMs) return _text.Length;
            int count = Math.Max(1, (int)Math.Floor(WordCount * Math.Max(0, elapsedMs) / DurationMs));
            return _wordEnds[Math.Min(count, WordCount) - 1];
        }

        // Separate labels avoid native control height limits. Preserve every text element.
        public static List<string> SplitForDisplay(string text)
        {
            text = text ?? string.Empty;
            var result = new List<string>();
            int start = 0;
            var elements = StringInfo.ParseCombiningCharacters(text);
            while (start < text.Length)
            {
                int end = Math.Min(start + 1600, text.Length);
                if (end < text.Length)
                {
                    // Prefer a paragraph or word boundary so continuations do not break a word.
                    int lowerBound = start + 800;
                    int lineBreak = text.LastIndexOf('\n', end - 1, end - lowerBound);
                    if (lineBreak >= lowerBound) end = lineBreak + 1;
                    else
                    {
                        for (int i = end - 1; i >= lowerBound; i--)
                        {
                            if (char.IsWhiteSpace(text[i])) { end = i + 1; break; }
                        }
                    }
                    int boundary = Array.BinarySearch(elements, end);
                    if (boundary < 0)
                    {
                        int next = ~boundary;
                        end = elements[next - 1];
                        if (end <= start) end = next < elements.Length ? elements[next] : text.Length;
                    }
                }
                result.Add(text.Substring(start, end - start));
                start = end;
            }
            if (result.Count == 0) result.Add(string.Empty);
            return result;
        }
    }

    public sealed class AiChatFlowPanel : FlowLayoutPanel
    {
        public AiChatFlowPanel() { DoubleBuffered = true; }
        // Avoid WinForms pulling a tall answer back to its top when focus changes.
        protected override System.Drawing.Point ScrollToControl(Control activeControl) => DisplayRectangle.Location;
    }
}
