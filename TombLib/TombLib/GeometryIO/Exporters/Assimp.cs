using Assimp;
using Assimp.Configs;
using System.IO;
using System.Xml.Linq;

namespace TombLib.GeometryIO.Exporters
{
    public class Assimp : BaseGeometryExporter
    {
        public enum Exporter
        {
            Collada,
            GLTF2,
            X3d
        }

        private GetTextureDelegate _callback;
        private Exporter _exporter;

        public Assimp(IOGeometrySettings settings, Exporter exporter, GetTextureDelegate getTexturePathCallback)
            : base(settings, getTexturePathCallback)
        {
            _callback = getTexturePathCallback;
            _exporter = exporter;
        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            var tempFile    = Path.Combine(Path.GetDirectoryName(filename), "_proxy.obj");
            var tempMtlFile = Path.Combine(Path.GetDirectoryName(filename), "_proxy.mtl");
            var objExporter = new ObjExporter(_settings, _callback);
            var objData = objExporter.ExportToFile(model, tempFile);

            AssimpContext context = new AssimpContext();
            context.SetConfig(new NormalSmoothingAngleConfig(90.0f));
            Scene scene = context.ImportFile(tempFile,
                PostProcessPreset.TargetRealTimeFast ^
                PostProcessSteps.Triangulate);

            switch (_exporter)
            {
                case Exporter.Collada:
                    {
                        context.ExportFile(scene, filename, "collada");

                        // Open up exported collada file and fix all texture filenames to relative
                        XDocument exported = XDocument.Load(filename);
                        foreach (var node in exported.Root.DescendantNodes())
                            if (node is XElement)
                            {
                                var element = (XElement)node;
                                if (element.Name.LocalName == "init_from" && element.Value.EndsWith(".png"))
                                    element.Value = Path.GetFileName(element.Value);
                            }
                        exported.Save(filename);
                    }
                    break;

                case Exporter.GLTF2:
                    context.ExportFile(scene, filename, "gltf2");
                    break;

                case Exporter.X3d:
                    context.ExportFile(scene, filename, "x3d");
                    break;
            }

            File.Delete(tempFile);
            File.Delete(tempMtlFile);

            return true;
        }
    }
}
