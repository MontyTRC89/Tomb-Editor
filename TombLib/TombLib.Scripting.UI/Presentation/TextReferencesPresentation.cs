#nullable enable

namespace TombLib.Scripting.UI.Presentation;

public sealed record class TextReferencesPresentation(
    string NoActiveDocumentText,
    string UnsupportedText,
    string LoadingText,
    string EmptyText);