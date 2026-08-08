using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Text;

/// <summary>
/// A structural lexer that tokenizes script source text into language-neutral
/// <see cref="ScriptToken"/> values. The lexer recognizes syntactic form and
/// does not assign semantic meaning such as command classification.
/// </summary>
public static class ScriptLexer
{
	/// <summary>
	/// Returns a stack-only enumerator that produces tokens for a single line.
	/// This method allocates no memory and is suitable for hot paths.
	/// </summary>
	/// <param name="line">The line text without line terminators.</param>
	/// <param name="lineOffset">
	/// The zero-based absolute offset of the start of this line in the source text.
	/// </param>
	/// <param name="lineNumber">The one-based line number.</param>
	/// <param name="options">Lexer options for the target language.</param>
	/// <returns>A <see cref="ScriptTokenEnumerator"/> for the line.</returns>
	public static ScriptTokenEnumerator TokenizeLine(
		ReadOnlySpan<char> line, int lineOffset, int lineNumber, ScriptLexerOptions options)
	{
		return new ScriptTokenEnumerator(line, lineOffset, lineNumber, options);
	}

	/// <summary>
	/// Produces all tokens in a document. Each line is read from
	/// <paramref name="source"/> and tokenized individually.
	/// </summary>
	/// <param name="source">The text source to tokenize.</param>
	/// <param name="options">Lexer options for the target language.</param>
	/// <returns>A read-only list of every token in the document.</returns>
	/// <remarks>
	/// This method allocates a line string per source line and collects all tokens
	/// into a list. Callers in hot paths should prefer <see cref="TokenizeLine"/>
	/// with a span obtained from a string-backed source.
	/// </remarks>
	public static IReadOnlyList<ScriptToken> TokenizeDocument(ITextSnapshot source, ScriptLexerOptions options)
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));

		var tokens = new List<ScriptToken>();

		foreach (var line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);
			var enumerator = TokenizeLine(lineText.AsSpan(), line.Offset, line.LineNumber, options);

			while (enumerator.MoveNext())
				tokens.Add(enumerator.Current);
		}

		return tokens;
	}

	/// <summary>
	/// A stack-only ref struct enumerator that produces <see cref="ScriptToken"/>
	/// values for a single line of text without allocation.
	/// </summary>
	public ref struct ScriptTokenEnumerator
	{
		private readonly ReadOnlySpan<char> _line;
		private readonly int _lineOffset;
		private readonly int _lineNumber;
		private readonly ScriptLexerOptions _options;
		private int _position;
		private ScriptToken _current;

		internal ScriptTokenEnumerator(
			ReadOnlySpan<char> line, int lineOffset, int lineNumber, ScriptLexerOptions options)
		{
			_line = line;
			_lineOffset = lineOffset;
			_lineNumber = lineNumber;
			_options = options;
			_position = 0;
			_current = default;
		}

		/// <summary>
		/// Gets the token at the current enumerator position.
		/// </summary>
		public ScriptToken Current => _current;

		/// <summary>
		/// Advances the enumerator to the next token.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the enumerator advanced to a valid token;
		/// <see langword="false"/> if the end of the line has been reached.
		/// </returns>
		public bool MoveNext()
		{
			if (_position >= _line.Length)
				return false;

			char ch = _line[_position];

			// Whitespace run.
			if (ch == ' ' || ch == '\t')
			{
				int start = _position;

				while (_position < _line.Length && (_line[_position] == ' ' || _line[_position] == '\t'))
					_position++;

				_current = MakeToken(ScriptTokenType.Whitespace, start, _position - start);
				return true;
			}

			// Comment: check for delimiter at current position.
			if (MatchesCommentDelimiter())
			{
				int start = _position;
				_position = _line.Length;
				_current = MakeToken(ScriptTokenType.Comment, start, _position - start);
				return true;
			}

			// Section header [Name].
			if (ch == _options.SectionOpenBracket && _options.SectionOpenBracket != '\0')
			{
				int closingIndex = _line.Slice(_position + 1).IndexOf(_options.SectionCloseBracket);

				if (closingIndex >= 0)
				{
					int start = _position;
					_position += closingIndex + 2; // Skip past closing bracket.
					_current = MakeToken(ScriptTokenType.SectionHeader, start, _position - start);
					return true;
				}
			}

			// Directive #Keyword (only at line start, after optional whitespace).
			if (ch == _options.DirectivePrefix && _options.DirectivePrefix != '\0' && IsAtLineStart(_position))
			{
				int start = _position;
				_position++; // Skip '#'.

				int keywordStart = _position;

				while (_position < _line.Length && IsIdentifierChar(_line[_position]))
					_position++;

				// Only emit Directive if at least one identifier character follows the prefix.
				if (_position > keywordStart)
				{
					_current = MakeToken(ScriptTokenType.Directive, start, _position - start);
					return true;
				}

				// No keyword after '#' — treat '#' as Unknown.
				_position = start + 1;
				_current = MakeToken(ScriptTokenType.Unknown, start, 1);
				return true;
			}

			// Continuation marker.
			if (ch == _options.ContinuationMarker && _options.ContinuationMarker != '\0')
			{
				if (IsAtEndOfCode(_position))
				{
					int start = _position;
					_position++;
					_current = MakeToken(ScriptTokenType.ContinuationMarker, start, _position - start);
					return true;
				}
			}

			// String literal.
			if (ch == _options.StringQuote && _options.StringQuote != '\0')
			{
				int start = _position;
				_position++; // Skip opening quote.

				while (_position < _line.Length)
				{
					if (MatchesCommentDelimiter())
					{
						// ClassicScript comments begin inside quoted text as well.
						break;
					}
					else if (_line[_position] == '\\' && _position + 1 < _line.Length)
					{
						// Skip escaped character.
						_position += 2;
					}
					else if (_line[_position] == _options.StringQuote)
					{
						_position++; // Skip closing quote.
						break;
					}
					else
					{
						_position++;
					}
				}

				_current = MakeToken(ScriptTokenType.StringLiteral, start, _position - start);
				return true;
			}

			// Hex value $XXXX.
			if (ch == _options.HexPrefix && _options.HexPrefix != '\0'
				&& _position + 1 < _line.Length
				&& IsHexDigit(_line[_position + 1]))
			{
				int start = _position;
				_position++; // Skip '$'.

				while (_position < _line.Length && IsHexDigit(_line[_position]))
					_position++;

				_current = MakeToken(ScriptTokenType.HexValue, start, _position - start);
				return true;
			}

			// Decimal value.
			if (IsDigit(ch))
			{
				int start = _position;

				while (_position < _line.Length && IsDigit(_line[_position]))
					_position++;

				_current = MakeToken(ScriptTokenType.DecimalValue, start, _position - start);
				return true;
			}

			// Identifier (or mnemonic-like identifier).
			if (IsIdentifierStartChar(ch))
			{
				int start = _position;

				while (_position < _line.Length && IsIdentifierChar(_line[_position]))
					_position++;

				int length = _position - start;
				var identifierSpan = _line.Slice(start, length);
				var type = LooksLikeMnemonic(identifierSpan)
					? ScriptTokenType.MnemonicLikeIdentifier
					: ScriptTokenType.Identifier;

				_current = MakeToken(type, start, length);
				return true;
			}

			// Single-character operators and punctuation.
			int tokenStart = _position;
			_position++;

			_current = ch switch
			{
				'=' => MakeToken(ScriptTokenType.Equals, tokenStart, 1),
				',' => MakeToken(ScriptTokenType.Comma, tokenStart, 1),
				'+' => MakeToken(ScriptTokenType.Plus, tokenStart, 1),
				'-' => MakeToken(ScriptTokenType.Minus, tokenStart, 1),
				'*' => MakeToken(ScriptTokenType.Asterisk, tokenStart, 1),
				'/' => MakeToken(ScriptTokenType.Slash, tokenStart, 1),
				'(' => MakeToken(ScriptTokenType.OpenParen, tokenStart, 1),
				')' => MakeToken(ScriptTokenType.CloseParen, tokenStart, 1),
				_ => MakeToken(ScriptTokenType.Unknown, tokenStart, 1)
			};

			return true;
		}

		/// <summary>
		/// Returns this enumerator, enabling <c>foreach</c> usage.
		/// </summary>
		public ScriptTokenEnumerator GetEnumerator() => this;

		// -----------------------------------------------------------------------
		// Helpers
		// -----------------------------------------------------------------------

		private ScriptToken MakeToken(ScriptTokenType type, int offset, int length) =>
			new(type, _lineOffset + offset, length, _lineNumber);

		/// <summary>
		/// Determines whether the given position is at the start of the line
		/// after accounting for optional leading whitespace.
		/// </summary>
		private bool IsAtLineStart(int position)
		{
			for (int i = 0; i < position; i++)
			{
				if (_line[i] != ' ' && _line[i] != '\t')
					return false;
			}

			return true;
		}

		private bool MatchesCommentDelimiter()
		{
			var delimiter = _options.CommentDelimiter;

			if (string.IsNullOrEmpty(delimiter))
				return false;

			if (_position + delimiter.Length > _line.Length)
				return false;

			for (int i = 0; i < delimiter.Length; i++)
			{
				if (_line[_position + i] != delimiter[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the character at the given position is a continuation
		/// marker, meaning the rest of the line (after stripping the comment) contains
		/// only whitespace.
		/// </summary>
		private bool IsAtEndOfCode(int markerPosition)
		{
			var afterMarker = _line.Slice(markerPosition + 1);
			var delimiter = _options.CommentDelimiter;

			// Strip comment from the remaining text.
			int commentStart = string.IsNullOrEmpty(delimiter)
				? -1
				: afterMarker.IndexOf(delimiter, StringComparison.Ordinal);

			var codeAfter = commentStart >= 0
				? afterMarker.Slice(0, commentStart)
				: afterMarker;

			// Must be only whitespace (or empty).
			foreach (char c in codeAfter)
			{
				if (c != ' ' && c != '\t')
					return false;
			}

			return true;
		}

		private static bool IsIdentifierStartChar(char ch) =>
			ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

		private static bool IsIdentifierChar(char ch) =>
			ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_';

		private static bool IsDigit(char ch) => ch is >= '0' and <= '9';

		private static bool IsHexDigit(char ch) =>
			ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

		/// <summary>
		/// Determines whether an identifier span looks like a mnemonic constant:
		/// all uppercase letters, digits, and underscores, with at least one underscore.
		/// </summary>
		private static bool LooksLikeMnemonic(ReadOnlySpan<char> identifier)
		{
			bool hasUnderscore = false;

			foreach (char c in identifier)
			{
				if (c == '_')
				{
					hasUnderscore = true;
				}
				else if (!(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9'))
				{
					return false;
				}
			}

			return hasUnderscore;
		}
	}
}
