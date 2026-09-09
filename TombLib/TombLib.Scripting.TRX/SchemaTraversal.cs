using Newtonsoft.Json.Schema;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Enumerates all schemas reachable from a root <see cref="JSchema"/>, guarding against cycles.
/// Internal to TRX: schema traversal is an implementation detail and is not part of the
/// public provider contract.
/// </summary>
internal static class SchemaTraversal
{
	/// <summary>
	/// Returns the root schema and every nested schema reachable through properties,
	/// array items, oneOf/anyOf/allOf combinators and resolved $ref targets.
	/// </summary>
	internal static IReadOnlyList<JSchema> FlattenSchemas(JSchema schema)
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
			{
				foreach (var property in current.Properties.Values)
					Visit(property);
			}

			if (current.Items is not null)
			{
				foreach (var item in current.Items)
					Visit(item);
			}

			foreach (var nestedSchema in current.OneOf ?? [])
				Visit(nestedSchema);

			foreach (var nestedSchema in current.AnyOf ?? [])
				Visit(nestedSchema);

			foreach (var nestedSchema in current.AllOf ?? [])
				Visit(nestedSchema);

			// In-file $ref targets are inlined by the schema reader, but follow an unresolved
			// reference when present so traversal stays complete for any schema shape.
			if (current.Ref is not null)
				Visit(current.Ref);
		}
	}
}
