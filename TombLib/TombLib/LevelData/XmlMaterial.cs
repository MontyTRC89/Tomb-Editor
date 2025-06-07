using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData
{
	public enum MaterialType : byte
	{
		Opaque,
		Water
	}

	public class XmlMaterial
	{
		public MaterialType Type { get; set; }
		public string ColorMap { get; set; }
		public string NormalMap { get; set; }
		public string SpecularMap { get; set; }
		public string RoughnessMap { get; set; }
		public string AlphaMaskMap { get; set; }
		public string AdditionalColorMap { get; set; }
		public Vector4 FloatParameters0 { get; set; }
		public Vector4 FloatParameters1 { get; set; }
		public Vector4 FloatParameters2 { get; set; }
		public Vector4 FloatParameters3 { get; set; }
		public VectorInt4 IntegerParameters0 { get; set; }
		public VectorInt4 IntegerParameters1 { get; set; }
		public VectorInt4 IntegerParameters2 { get; set; }
		public VectorInt4 IntegerParameters3 { get; set; }

		public XmlMaterial() { }

		public static XmlMaterial ReadFromXml(string filename)
		{
			return XmlUtils.ReadXmlFile<XmlMaterial>(filename);
		}

		public static bool SaveToXml(string filename, WadSounds sounds)
		{
			XmlUtils.WriteXmlFile(filename, sounds);
			return true;
		}
	}
}
