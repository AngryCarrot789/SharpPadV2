using System;

namespace SharpPadV2.EditorV2.Strings {
    public readonly struct TextRange {
        public int Begin { get; }

        public int Length { get; }

        public int EndIndex => this.Begin + this.Length;

        public TextRange(int begin, int length) {
            this.Begin = begin;
            this.Length = length;
        }

        public static TextRange FromRange(int begin, int length) {
            return new TextRange(begin, length);
        }

        public static TextRange FromIndices(int startIndex, int endIndex) {
            int length = endIndex - startIndex;
            if (length < 0) {
                throw new ArgumentException($"endIndex - startIndex (length) cannot be less than 0 ({endIndex} - {startIndex} == {length})");
            }

            return new TextRange(startIndex, length);
        }

        public void ValidateLength() {
            if (this.Length < 0) {
                throw new ArgumentException($"Length must be non-negative");
            }
        }
    }
}