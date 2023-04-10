using System.Collections.Generic;

namespace SharpPadV2.EditorV2.Strings {
    public class LineSpan {
        private readonly List<PlainTextFragment> fragments;

        public LineSpan() {
            this.fragments = new List<PlainTextFragment>();
        }
    }
}