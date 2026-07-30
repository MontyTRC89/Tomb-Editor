#nullable enable

using Newtonsoft.Json.Schema;
using TombLib.Scripting.TRX.Models;

namespace TombLib.Scripting.TRX.Services;

public interface IGameflowSchemaService
{
	JSchema? Schema { get; }

	SchemaKeywords? GetSchemaKeywords();
}
