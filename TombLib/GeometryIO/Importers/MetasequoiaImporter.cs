using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO.Importers
{
    public class MetasequoiaImporter : BaseGeometryImporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MetasequoiaImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback) 
            : base(settings, getTextureCallback)
        {

        }

        public override IOModel ImportFromFile(string filename)
        {
            var model = new IOModel();
            //var materials
            /*using (var reader = new StreamReader(File.OpenRead(filename)))
            {
                var line = reader.ReadLine();
                if (line.Trim() != "Metasequoia Document")
                {
                    logger.Error("Not a valid Metasequoia file");
                    return null;
                }
                line = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (line.StartsWith("Material"))
                    {
                        var numMaterials = Int32.Parse(line.Split(' ')[1]);
                        for (var i=0;i<numMaterials;i++)
                        {

                        }
                    }
                }

            }*/

            return model;
        }

        
    }
}
