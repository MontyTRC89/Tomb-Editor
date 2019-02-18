using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public class ScriptCompilerTR4OrTR5 : IScriptCompiler
    {
        private string _srcPath;
        private string _dstPath;

        private List<LevelScript> _levels;
        private List<string> _languageFiles;
        private List<LanguageScript> _strings;
        private string _psxLevel;
        private string _psxCut;
        private string _psxFmv;
        private string _pcLevel;
        private string _pcCut;
        private string _pcFmv;
        private bool _loadSave;
        private bool _title;
        private bool _playAnyLevel;
        private bool _flyCheat;
        private bool _demoDisc;
        private int _inputTimeout;
        private byte _security;

        public bool CompileScripts(string srcPath, string dstPath)
        {
            _srcPath = srcPath;
            _dstPath = dstPath;

            if (!ReadScripts())
                return false;

            return BuildScripts();
        }

        private bool ReadScripts()
        {
            LevelScriptCatalog.LoadCatalog();

            _languageFiles = new List<string>();
            _levels = new List<LevelScript>();
            _strings = new List<LanguageScript>();

            try
            {
                using (var stream = File.OpenRead(_srcPath + "\\Script.txt"))
                using (var reader = new StreamReader(stream))
                {
                    var lastBlock = ScriptBlocks.None;
                    LevelScript lastLevel = null;

                    while (reader.EndOfStream == false)
                    {
                        string line = reader.ReadLine().Trim();

                        // Comment or empty line?
                        if (line.StartsWith(";") || line == "")
                            continue;

                        // Inline comment?
                        var tokensLine = line.Split(';');
                        line = tokensLine[0];

                        // Block header?
                        if (line.StartsWith("["))
                        {
                            if (line == "[PSXExtensions]")
                            {
                                lastBlock = ScriptBlocks.PsxExtensions;
                                continue;
                            }

                            if (line == "[PCExtensions]")
                            {
                                lastBlock = ScriptBlocks.PcExtensions;
                                continue;
                            }

                            if (line == "[Language]")
                            {
                                lastBlock = ScriptBlocks.Language;
                                continue;
                            }

                            if (line == "[Options]")
                            {
                                lastBlock = ScriptBlocks.Options;
                                continue;
                            }

                            if (line == "[Title]")
                            {
                                lastBlock = ScriptBlocks.Title;
                                lastLevel = new LevelScript(true);
                                _levels.Add(lastLevel);
                                continue;
                            }

                            if (line == "[Level]")
                            {
                                lastBlock = ScriptBlocks.Level;
                                lastLevel = new LevelScript(false);
                                _levels.Add(lastLevel);
                                continue;
                            }
                        }

                        // Block content?
                        var tokens = line.Split('=');
                        if (tokens.Length < 2)
                            continue;

                        string command = tokens[0].Trim();
                        string value = tokens[1].Trim();

                        if (lastBlock == ScriptBlocks.PsxExtensions)
                        {
                            if (command == "Level")
                                _psxLevel = value;
                            else if (command == "Cut")
                                _psxCut = value;
                            else if (command == "FMV")
                                _psxFmv = value;
                        }

                        if (lastBlock == ScriptBlocks.PcExtensions)
                        {
                            if (command == "Level")
                                _pcLevel = value;
                            else if (command == "Cut")
                                _pcCut = value;
                            else if (command == "FMV")
                                _pcFmv = value;
                        }

                        if (lastBlock == ScriptBlocks.Language)
                        {
                            if (command == "File")
                            {
                                var tokensFile = value.Split(',');
                                if (tokensFile.Length < 2)
                                    continue;
                                _languageFiles.Add(tokensFile[1]);
                            }
                        }

                        if (lastBlock == ScriptBlocks.Options)
                        {
                            if (command == "LoadSave")
                                _loadSave = (value == "ENABLED");
                            else if (command == "Title")
                                _title = (value == "ENABLED");
                            else if (command == "PlayAnyLevel")
                                _playAnyLevel = (value == "ENABLED");
                            else if (command == "FlyCheat")
                                _flyCheat = (value == "ENABLED");
                            else if (command == "DemoDisc")
                                _demoDisc = (value == "ENABLED");
                            else if (command == "InputTimeout")
                                _inputTimeout = int.Parse(value);
                            else if (command == "Security")
                                _security = Convert.ToByte(value.Replace("$", ""), 16);
                        }

                        if (lastBlock == ScriptBlocks.Title || lastBlock == ScriptBlocks.Level)
                        {
                            if (LevelScriptCatalog.Commands.ContainsKey(command))
                            {
                                var cmd = LevelScriptCatalog.Commands[command];
                                var tokensParams = value.Split(',');
                                if (tokensParams.Length != cmd.Parameters.Count)
                                    continue;

                                var entry = new LevelScriptEntry(cmd);

                                for (int j = 0; j < tokensParams.Length; j++)
                                {
                                    string paramValue = tokensParams[j].Trim();

                                    switch (cmd.Parameters[j])
                                    {
                                        case LevelScriptCatalogParameterType.Boolean:
                                            entry.Parameters.Add(paramValue == "ENABLED");
                                            break;
                                        case LevelScriptCatalogParameterType.Hex:
                                            short val = 0;
                                            bool success = short.TryParse(paramValue.Replace("$", ""), NumberStyles.HexNumber, null, out val);
                                            entry.Parameters.Add(val);
                                            break;
                                        case LevelScriptCatalogParameterType.Int8:
                                            entry.Parameters.Add(byte.Parse(paramValue));
                                            break;
                                        case LevelScriptCatalogParameterType.SInt8:
                                            entry.Parameters.Add(sbyte.Parse(paramValue));
                                            break;
                                        case LevelScriptCatalogParameterType.Int16:
                                            entry.Parameters.Add(short.Parse(paramValue));
                                            break;
                                        case LevelScriptCatalogParameterType.Int32:
                                            entry.Parameters.Add(int.Parse(paramValue));
                                            break;
                                        case LevelScriptCatalogParameterType.String:
                                            entry.Parameters.Add(paramValue);
                                            break;
                                    }
                                }

                                if (entry.Command.Name == "Name")
                                    lastLevel.Name = (string)entry.Parameters[0];
                                else if (entry.Command.Name == "Level")
                                {
                                    lastLevel.FileName = (string)entry.Parameters[0];
                                    lastLevel.AudioTrack = (byte)entry.Parameters[1];
                                }

                                lastLevel.Entries.Add(entry);
                            }
                        }
                    }
                }

                foreach (var languageFile in _languageFiles)
                {
                    string languageFilePath = _srcPath + "\\" + languageFile;
                    if (!File.Exists(languageFilePath))
                        continue;
                    using (var reader = new StreamReader(File.OpenRead(languageFilePath)))
                    {
                        var strings = new LanguageScript();
                        string lastBlock = "";
                        while (reader.EndOfStream == false)
                        {
                            var line = reader.ReadLine().Trim();

                            if (line.StartsWith(";") || line == "")
                                continue;

                            if (line.StartsWith("["))
                            {
                                lastBlock = line;
                                continue;
                            }

                            if (lastBlock == "[Strings]")
                                strings.GeneralStrings.Add(line);
                            else if (lastBlock == "[PSXStrings]")
                                strings.PsxStrings.Add(line);
                            else if (lastBlock == "[PCStrings]")
                                strings.PcStrings.Add(line);
                            else if (lastBlock == "[ExtraNG]")
                                strings.NgStrings.Add(line);
                        }
                        _strings.Add(strings);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool BuildScripts()
        {

        }
    }
}
