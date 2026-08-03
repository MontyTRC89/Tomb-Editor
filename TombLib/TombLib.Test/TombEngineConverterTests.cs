using System.Reflection;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombLib.Test;

[TestClass]
public class TombEngineConverterTests
{
	private static readonly MethodInfo ConvertLegacyMaterialSidecarMethod =
		typeof(TombEngineConverter).GetMethod("ConvertLegacyMaterialSidecar", BindingFlags.NonPublic | BindingFlags.Static)!;

	[TestMethod]
	public void ConvertLegacyMaterialSidecar_WithLegacyBumpPath_CreatesXmlWithNormalMap()
	{
		var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempDirectory);

		try
		{
			var texturePath = Path.Combine(tempDirectory, "default.png");
			var bumpPath = Path.Combine(tempDirectory, "default_custom_n.png");
			File.WriteAllBytes(texturePath, [1]);
			File.WriteAllBytes(bumpPath, [2]);

			var settings = new LevelSettings
			{
				LevelFilePath = Path.Combine(tempDirectory, "test.prj2")
			};

			var migratedLegacyBumpPath = ConvertLegacyMaterialSidecarMethod.Invoke(null, [settings, texturePath, bumpPath]);
			var xmlPath = Path.Combine(tempDirectory, "default.xml");
			var materialData = MaterialData.ReadFromXml(xmlPath);

			Assert.AreEqual(true, migratedLegacyBumpPath);
			Assert.IsNotNull(materialData);
			Assert.AreEqual(texturePath, materialData.ColorMap);
			Assert.AreEqual(bumpPath, materialData.NormalMap);
		}
		finally
		{
			Directory.Delete(tempDirectory, true);
		}
	}

	[TestMethod]
	public void ConvertLegacyMaterialSidecar_WithoutLegacyData_DoesNotCreateXml()
	{
		var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempDirectory);

		try
		{
			var texturePath = Path.Combine(tempDirectory, "default.png");
			File.WriteAllBytes(texturePath, [1]);

			var settings = new LevelSettings
			{
				LevelFilePath = Path.Combine(tempDirectory, "test.prj2")
			};

			var migratedLegacyBumpPath = ConvertLegacyMaterialSidecarMethod.Invoke(null, [settings, texturePath, null]);
			var xmlPath = Path.Combine(tempDirectory, "default.xml");

			Assert.AreEqual(false, migratedLegacyBumpPath);
			Assert.IsFalse(File.Exists(xmlPath));
		}
		finally
		{
			Directory.Delete(tempDirectory, true);
		}
	}
}
