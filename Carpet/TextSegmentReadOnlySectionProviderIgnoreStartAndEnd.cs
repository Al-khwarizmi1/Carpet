using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Carpet
{
    public class TextSegmentReadOnlySectionProviderIgnoreStartAndEnd<T> : TextSegmentReadOnlySectionProvider<T> where T : TextSegment
    {
        private TextDocument _textDocument;
        public TextSegmentReadOnlySectionProviderIgnoreStartAndEnd(TextDocument textDocument)
            : base(textDocument)
        {
            this._textDocument = textDocument;
        }


        public override bool CanInsert(int offset)
        {
            if (offset == 0 || offset == _textDocument.TextLength)
            {
                return false;
            }
            return base.CanInsert(offset);
        }
    }
}
