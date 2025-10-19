#nullable enable

using Newtonsoft.Json.Schema;
using TombLib.Scripting.Tomb1Main.Objects;

public interface IGameflowSchemaService
{
	JSchema? Schema { get; }
	SchemaKeywords? GetSchemaKeywords();
}
