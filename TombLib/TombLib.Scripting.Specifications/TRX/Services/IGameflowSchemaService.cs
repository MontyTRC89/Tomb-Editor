#nullable enable

using Newtonsoft.Json.Schema;
using TombLib.Scripting.Specifications.TRX.Models;

namespace TombLib.Scripting.Specifications.TRX.Services;

public interface IGameflowSchemaService
{
	JSchema? Schema { get; }
	SchemaKeywords? GetSchemaKeywords();
}
