#nullable enable

using System.IO;
using System.Linq;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXGameFlowSchemaServiceTests
{
	[TestMethod]
	public void BundledSchema_LoadsAndYieldsKeywords()
	{
		var service = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());

		Assert.AreEqual(TRXSchemaLoadState.Loaded, service.LoadState);
		Assert.IsNotNull(service.Model);
		Assert.IsTrue(service.Keywords.Collections.Count > 0 || service.Keywords.Properties.Count > 0 || service.Keywords.Constants.Count > 0);
		Assert.IsTrue(service.Model.Properties.Count > 0);
	}

	[TestMethod]
	public void MissingResource_SetsLoadStateAndEmptyKeywords()
	{
		var service = new TRXGameFlowSchemaService(Path.Combine(Path.GetTempPath(), "does-not-exist", "gameflow-schema.json"));

		Assert.AreEqual(TRXSchemaLoadState.MissingResource, service.LoadState);
		Assert.IsNull(service.Model);
		Assert.AreSame(TRXSchemaKeywords.Empty, service.Keywords);
	}

	[TestMethod]
	public void InvalidSchema_SetsLoadStateAndEmptyKeywords()
	{
		string path = Path.Combine(Path.GetTempPath(), "invalid-gameflow-schema.json");
		File.WriteAllText(path, "this is not valid json");

		try
		{
			var service = new TRXGameFlowSchemaService(path);

			Assert.AreEqual(TRXSchemaLoadState.InvalidSchema, service.LoadState);
			Assert.IsNull(service.Model);
			Assert.IsTrue(service.Keywords.Collections.Count == 0 && service.Keywords.Properties.Count == 0 && service.Keywords.Constants.Count == 0);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void ValidSchemaWithoutProperties_LoadsWithEmptyModel()
	{
		string path = Path.Combine(Path.GetTempPath(), "empty-gameflow-schema.json");
		File.WriteAllText(path, "{ \"type\": \"object\" }");

		try
		{
			var service = new TRXGameFlowSchemaService(path);

			Assert.AreEqual(TRXSchemaLoadState.Loaded, service.LoadState);
			Assert.IsNotNull(service.Model);
			Assert.AreEqual(0, service.Model.Properties.Count);
			Assert.AreEqual(0, service.Keywords.Collections.Count + service.Keywords.Properties.Count + service.Keywords.Constants.Count);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void FixtureWithDefsRefsAndDefinitions_IncludesDefinitionOnlyContent()
	{
		string path = WriteFixture("gameflow-defs-refs-fixture.json", DefsAndRefsFixture);

		try
		{
			var service = new TRXGameFlowSchemaService(path);

			Assert.AreEqual(TRXSchemaLoadState.Loaded, service.LoadState);
			Assert.IsNotNull(service.Model);

			// $defs-only content surfaces even though no property references the definition.
			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "only_in_defs"));
			Assert.IsTrue(service.Keywords.Properties.Contains("only_in_defs"));

			// $defs content reachable through a $ref from the root and from a definition.
			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "file"));
			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "file_type"));
			Assert.IsTrue(service.Keywords.Properties.Contains("file"));
			Assert.IsTrue(service.Keywords.Properties.Contains("file_type"));

			// Legacy definitions content remains covered.
			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "title_name"));
			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "path"));
			Assert.IsTrue(service.Keywords.Properties.Contains("title_name"));
			Assert.IsTrue(service.Keywords.Properties.Contains("path"));

			// Root array properties are classified as collections.
			Assert.IsTrue(service.Keywords.Collections.Contains("levels"));

			// Enum values inside a $defs definition survive the conversion.
			Assert.IsTrue(service.Keywords.Constants.Contains("level"));
			Assert.IsTrue(service.Keywords.Constants.Contains("cutscene"));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void SchemaWithOnlyDefs_SurfacesDefinitionProperties()
	{
		string path = WriteFixture("gameflow-defs-only-fixture.json", DefsOnlyFixture);

		try
		{
			var service = new TRXGameFlowSchemaService(path);

			Assert.AreEqual(TRXSchemaLoadState.Loaded, service.LoadState);
			Assert.IsNotNull(service.Model);

			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "root_prop"));
			Assert.IsTrue(service.Model.Properties.Any(p => p.Name == "item_name"));
			Assert.IsTrue(service.Keywords.Properties.Contains("root_prop"));
			Assert.IsTrue(service.Keywords.Properties.Contains("item_name"));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void SchemaWithDuplicateKeywords_DeduplicatesKeywords()
	{
		string path = WriteFixture("gameflow-duplicate-keywords-fixture.json", DuplicateKeywordsFixture);

		try
		{
			var service = new TRXGameFlowSchemaService(path);

			Assert.AreEqual(TRXSchemaLoadState.Loaded, service.LoadState);
			Assert.IsNotNull(service.Model);

			// A keyword reachable through both the root and a referenced definition is surfaced
			// exactly once, in both the keyword lists and the model properties.
			Assert.AreEqual(1, service.Keywords.Properties.Count(name => name == "shared"));
			Assert.AreEqual(1, service.Keywords.Constants.Count(name => name == "ENGINE_1"));
			Assert.AreEqual(1, service.Model.Properties.Count(property => property.Name == "shared"));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void SchemaWithDescriptions_SurfacesPropertyDescriptions()
	{
		string path = WriteFixture("gameflow-descriptions-fixture.json", DescriptionsFixture);

		try
		{
			var service = new TRXGameFlowSchemaService(path);

			Assert.AreEqual(TRXSchemaLoadState.Loaded, service.LoadState);
			Assert.IsNotNull(service.Model);

			Assert.AreEqual("Human-readable level name.", service.Model.Properties.First(p => p.Name == "name").Description);
			Assert.AreEqual("Name of an individual level.", service.Model.Properties.First(p => p.Name == "level_name").Description);
		}
		finally
		{
			File.Delete(path);
		}
	}

	private static string WriteFixture(string fileName, string content)
	{
		string path = Path.Combine(Path.GetTempPath(), fileName);

		File.WriteAllText(path, content);

		return path;
	}

	private const string DefsAndRefsFixture =
		"""
		{
		  "$defs": {
		    "path": {
		      "type": "object",
		      "properties": {
		        "file": { "type": "string" },
		        "file_type": { "enum": [ "level", "cutscene" ] }
		      }
		    },
		    "hidden": {
		      "type": "object",
		      "properties": {
		        "only_in_defs": { "type": "integer" }
		      }
		    }
		  },
		  "type": "object",
		  "properties": {
		    "name": { "type": "string" },
		    "levels": {
		      "type": "array",
		      "items": { "$ref": "#/definitions/level" }
		    },
		    "main_script": { "$ref": "#/$defs/path" }
		  },
		  "definitions": {
		    "level": {
		      "type": "object",
		      "properties": {
		        "title_name": { "type": "string" },
		        "path": { "$ref": "#/$defs/path" }
		      }
		    }
		  }
		}
		""";

	private const string DefsOnlyFixture =
		"""
		{
		  "$defs": {
		    "item": {
		      "type": "object",
		      "properties": {
		        "item_name": { "type": "string" }
		      }
		    }
		  },
		  "type": "object",
		  "properties": {
		    "root_prop": { "type": "string" }
		  }
		}
		""";

	private const string DuplicateKeywordsFixture =
		"""
		{
		  "$defs": {
		    "shared": {
		      "type": "object",
		      "properties": {
		        "shared": { "type": "string" },
		        "engine": { "enum": [ "ENGINE_1", "ENGINE_2" ] }
		      }
		    }
		  },
		  "type": "object",
		  "properties": {
		    "name": { "type": "string" },
		    "shared": { "$ref": "#/$defs/shared" },
		    "engine": { "enum": [ "ENGINE_1" ] }
		  }
		}
		""";

	private const string DescriptionsFixture =
		"""
		{
		  "$defs": {
		    "level": {
		      "type": "object",
		      "properties": {
		        "level_name": { "type": "string", "description": "Name of an individual level." }
		      }
		    }
		  },
		  "type": "object",
		  "properties": {
		    "name": { "type": "string", "description": "Human-readable level name." },
		    "levels": {
		      "type": "array",
		      "items": { "$ref": "#/$defs/level" }
		    }
		  }
		}
		""";
}