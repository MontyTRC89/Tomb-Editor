﻿using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombLib.LevelData
{
	public enum MaterialType : byte
	{
		Opaque,
		Water
	}

	public class MaterialData
	{
		public MaterialType Type { get; set; }
		public string ColorMap { get; set; }
		public string NormalMap { get; set; }
		public string SpecularMap { get; set; }
		public string RoughnessMap { get; set; }
		public string AmbientOcclusionMap { get; set; }
		public string AlphaMaskMap { get; set; }
		public string AdditionalColorMap { get; set; }
		public string EmissiveMap { get; set; }
		public Vector4 FloatParameters0 { get; set; }
		public Vector4 FloatParameters1 { get; set; }
		public Vector4 FloatParameters2 { get; set; }
		public Vector4 FloatParameters3 { get; set; }
		public VectorInt4 IntegerParameters0 { get; set; }
		public VectorInt4 IntegerParameters1 { get; set; }
		public VectorInt4 IntegerParameters2 { get; set; }
		public VectorInt4 IntegerParameters3 { get; set; }
		public Vector2 Vector2Parameters0 { get; set; }
		public Vector2 Vector2Parameters1 { get; set; }
		public Vector2 Vector2Parameters2 { get; set; }
		public Vector2 Vector2Parameters3 { get; set; }
		public Vector3 Vector3Parameters0 { get; set; }
		public Vector3 Vector3Parameters1 { get; set; }
		public Vector3 Vector3Parameters2 { get; set; }
		public Vector3 Vector3Parameters3 { get; set; }
		public Vector4 Vector4Parameters0 { get; set; }
		public Vector4 Vector4Parameters1 { get; set; }
		public Vector4 Vector4Parameters2 { get; set; }
		public Vector4 Vector4Parameters3 { get; set; }

		[XmlIgnore]
		public string XmlMaterialFileName { get; set; }

		public MaterialData() { }

		public static MaterialData ReadFromXml(string filename)
		{
			var materialData = XmlUtils.ReadXmlFile<MaterialData>(filename);

			materialData.XmlMaterialFileName = filename;

			// Make all paths absolutes
			if (materialData is not null)
			{
				var basePath = Path.GetDirectoryName(filename);

				if (!string.IsNullOrEmpty(materialData.ColorMap))
				{
					if (!PathC.IsTrulyAbsolutePath(materialData.ColorMap))
						materialData.ColorMap = Path.Combine(basePath, materialData.ColorMap);
				}

				if (!string.IsNullOrEmpty(materialData.NormalMap))
				{
					if (!PathC.IsTrulyAbsolutePath(materialData.NormalMap))
						materialData.NormalMap = Path.Combine(basePath, materialData.NormalMap);	
				}

				if (!string.IsNullOrEmpty(materialData.EmissiveMap))
				{
					if (!PathC.IsTrulyAbsolutePath(materialData.EmissiveMap))
						materialData.EmissiveMap = Path.Combine(basePath, materialData.EmissiveMap);
				}

				if (!string.IsNullOrEmpty(materialData.SpecularMap))
				{
					if (!PathC.IsTrulyAbsolutePath(materialData.SpecularMap))
						materialData.SpecularMap = Path.Combine(basePath, materialData.SpecularMap);
				}

				if (!string.IsNullOrEmpty(materialData.RoughnessMap))
				{
					if (!PathC.IsTrulyAbsolutePath(materialData.RoughnessMap))
						materialData.RoughnessMap = Path.Combine(basePath, materialData.RoughnessMap);
				}

				if (!string.IsNullOrEmpty(materialData.AmbientOcclusionMap))
				{
					if (!PathC.IsTrulyAbsolutePath(materialData.AmbientOcclusionMap))
						materialData.AmbientOcclusionMap = Path.Combine(basePath, materialData.AmbientOcclusionMap);
				}
			}

			return materialData;
		}

		public static bool SaveToXml(string filename, MaterialData materialData)
		{
			// Make all paths relatives
			if (!string.IsNullOrEmpty(materialData.ColorMap) && File.Exists(materialData.ColorMap))
			{
				var relativePath = PathC.GetRelativePath(Path.GetDirectoryName(filename), materialData.ColorMap);
				if (relativePath is not null)
					materialData.ColorMap = relativePath;
			}

			if (!string.IsNullOrEmpty(materialData.NormalMap) && File.Exists(materialData.NormalMap))
			{
				var relativePath = PathC.GetRelativePath(Path.GetDirectoryName(filename), materialData.NormalMap);
				if (relativePath is not null)
					materialData.NormalMap = relativePath;
			}

			if (!string.IsNullOrEmpty(materialData.SpecularMap) && File.Exists(materialData.SpecularMap))
			{
				var relativePath = PathC.GetRelativePath(Path.GetDirectoryName(filename), materialData.SpecularMap);
				if (relativePath is not null)
					materialData.SpecularMap = relativePath;
			}

			if (!string.IsNullOrEmpty(materialData.EmissiveMap) && File.Exists(materialData.EmissiveMap))
			{
				var relativePath = PathC.GetRelativePath(Path.GetDirectoryName(filename), materialData.EmissiveMap);
				if (relativePath is not null)
					materialData.EmissiveMap = relativePath;
			}

			if (!string.IsNullOrEmpty(materialData.AmbientOcclusionMap) && File.Exists(materialData.AmbientOcclusionMap))
			{
				var relativePath = PathC.GetRelativePath(Path.GetDirectoryName(filename), materialData.AmbientOcclusionMap);
				if (relativePath is not null)
					materialData.AmbientOcclusionMap = relativePath;
			}

			if (!string.IsNullOrEmpty(materialData.RoughnessMap) && File.Exists(materialData.RoughnessMap))
			{
				var relativePath = PathC.GetRelativePath(Path.GetDirectoryName(filename), materialData.RoughnessMap);
				if (relativePath is not null)
					materialData.RoughnessMap = relativePath;
			}

			XmlUtils.WriteXmlFile(filename, materialData);
			return true;
		}

		public static MaterialData TrySidecarLoadOrLoadExisting(string textureAbsolutePath)
		{
			if (string.IsNullOrEmpty(textureAbsolutePath))
				return null;

			string externalMaterialDataPath = Path.Combine(
				 Path.GetDirectoryName(textureAbsolutePath),
				 Path.GetFileNameWithoutExtension(textureAbsolutePath) + ".xml");

			// If a material XML file already exists, just load it
			if (!string.IsNullOrEmpty(externalMaterialDataPath) && File.Exists(externalMaterialDataPath))
			{
				return ReadFromXml(externalMaterialDataPath);
			}

			// Otherwise, try to sidecar load textures
			var materialData = new MaterialData();

			materialData.ColorMap = textureAbsolutePath;

			materialData.NormalMap = Path.Combine(
						Path.GetDirectoryName(textureAbsolutePath),
						Path.GetFileNameWithoutExtension(textureAbsolutePath) + "_N" +
						Path.GetExtension(textureAbsolutePath));

			materialData.SpecularMap = Path.Combine(
						Path.GetDirectoryName(textureAbsolutePath),
						Path.GetFileNameWithoutExtension(textureAbsolutePath) + "_S" +
						Path.GetExtension(textureAbsolutePath));

			materialData.AmbientOcclusionMap = Path.Combine(
						Path.GetDirectoryName(textureAbsolutePath),
						Path.GetFileNameWithoutExtension(textureAbsolutePath) + "_AO" +
						Path.GetExtension(textureAbsolutePath));

			materialData.RoughnessMap = Path.Combine(
						Path.GetDirectoryName(textureAbsolutePath),
						Path.GetFileNameWithoutExtension(textureAbsolutePath) + "_R" +
						Path.GetExtension(textureAbsolutePath));

			materialData.EmissiveMap = Path.Combine(
					  Path.GetDirectoryName(textureAbsolutePath),
					  Path.GetFileNameWithoutExtension(textureAbsolutePath) + "_E" +
					  Path.GetExtension(textureAbsolutePath));

			// Clear textures which are not found
			if (!File.Exists(materialData.NormalMap))
				materialData.NormalMap = "";
			if (!File.Exists(materialData.SpecularMap))
				materialData.SpecularMap = "";
			if (!File.Exists(materialData.RoughnessMap))
				materialData.RoughnessMap = "";
			if (!File.Exists(materialData.AmbientOcclusionMap))
				materialData.AmbientOcclusionMap = "";
			if (!File.Exists(materialData.EmissiveMap))
				materialData.EmissiveMap = "";

			// Default material is opaque with normal and specular intensitites equals to 1.0f
			materialData.Type = MaterialType.Opaque;
			materialData.FloatParameters0 = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);

			return materialData;
		}
	}
}
