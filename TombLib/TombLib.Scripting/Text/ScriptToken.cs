using System;

namespace TombLib.Scripting.Text;

/// <summary>
/// Represents a single lexical token produced by <see cref="ScriptLexer"/>.
/// The offset and length are relative to the full source text.
/// </summary>
public readonly struct ScriptToken : IEquatable<ScriptToken>
{
	/// <summary>
	/// Gets the structural type of this token.
	/// </summary>
	public ScriptTokenType Type { get; }

	/// <summary>
	/// Gets the zero-based absolute offset of the token in the source text.
	/// </summary>
	public int Offset { get; }

	/// <summary>
	/// Gets the length of the token in characters.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// Gets the one-based line number on which this token appears.
	/// </summary>
	public int LineNumber { get; }

	/// <summary>
	/// Gets the zero-based offset of the first character after this token,
	/// equal to <c>Offset + Length</c>.
	/// </summary>
	public int EndOffset => Offset + Length;

	/// <summary>
	/// Initializes a new instance of the <see cref="ScriptToken"/> struct.
	/// </summary>
	/// <param name="type">The structural token type.</param>
	/// <param name="offset">The zero-based absolute offset in the source text.</param>
	/// <param name="length">The length of the token in characters.</param>
	/// <param name="lineNumber">The one-based line number.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="offset"/> or <paramref name="length"/> is negative.
	/// </exception>
	public ScriptToken(ScriptTokenType type, int offset, int length, int lineNumber)
	{
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset));
		if (length < 0)
			throw new ArgumentOutOfRangeException(nameof(length));

		Type = type;
		Offset = offset;
		Length = length;
		LineNumber = lineNumber;
	}

	/// <summary>
	/// Returns the text of this token by slicing the given source text.
	/// </summary>
	/// <param name="source">The full source text.</param>
	/// <returns>The substring covered by this token.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The token extends beyond the length of <paramref name="source"/>.
	/// </exception>
	public string GetText(string source)
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));

		// Overflow-safe bounds check: the subtraction cannot overflow because
		// Offset is verified to be within the source length first.
		if (Offset > source.Length || Length > source.Length - Offset)
			throw new ArgumentOutOfRangeException(nameof(source));

		return source.Substring(Offset, Length);
	}

	/// <inheritdoc />
	public bool Equals(ScriptToken other) =>
		Type == other.Type && Offset == other.Offset && Length == other.Length && LineNumber == other.LineNumber;

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is ScriptToken other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(Type, Offset, Length, LineNumber);

	/// <inheritdoc />
	public override string ToString() =>
		$"{Type} @{Offset} len={Length} L{LineNumber}";
}
