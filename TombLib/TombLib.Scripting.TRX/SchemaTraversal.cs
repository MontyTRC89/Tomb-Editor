using Newtonsoft.Json.Schema;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Enumerates all schemas reachable from a root <see cref="JSchema"/>, guarding against cycles.
/// </summary>
public static class SchemaTraversal
{
	/// <summary>
	/// Returns the root schema and every nested schema reachable through properties,
	/// array items and oneOf/anyOf/allOf combinators.
	/// </summary>
	public static IReadOnlyList<JSchema> FlattenSchemas(JSchema schema)
	{
		var visited = new HashSet<JSchema>();
		var result = new List<JSchema>();

		Visit(schema);

		return result;

		void Visit(JSchema current)
		{
			if (!visited.Add(current))
				return;

			result.Add(current);

			if (current.Properties is not null)
				foreach (var property in current.Properties.Values)
					Visit(property);

			if (current.Items is not null)
				foreach (var item in current.Items)
					Visit(item);

			foreach (var nestedSchema in current.OneOf ?? [])
				Visit(nestedSchema);

			foreach (var nestedSchema in current.AnyOf ?? [])
				Visit(nestedSchema);

			foreach (var nestedSchema in current.AllOf ?? [])
				Visit(nestedSchema);
		}
	}
}
