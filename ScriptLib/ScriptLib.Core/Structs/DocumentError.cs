using ICSharpCode.AvalonEdit.Document;

namespace ScriptLib.Core.Structs;

public readonly record struct DocumentError(ISegment Segment, string ErrorMessage);
