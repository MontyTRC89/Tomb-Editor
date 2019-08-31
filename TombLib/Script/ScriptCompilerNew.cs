using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;

namespace TombLib.Script
{
    public class ScriptCompilerNew : IScriptCompiler
    {
        private string _srcPath;
        private string _dstPath;
        private GameVersion _version;

        private List<LevelScript> _levels;
        private List<string> _languageFiles;
        private List<LanguageScript> _strings;
        private List<string> _levelFileNames;

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

        public ScriptCompilerNew(GameVersion version)
        {
            _version = version;
        }

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
            _levelFileNames = new List<string>();

            try
            {
                using (var stream = File.OpenRead(_srcPath + "\\Script.txt"))
                using (var reader = new StreamReader(stream))
                {
                    var lastBlock = Enumerations.None;
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
                                lastBlock = Enumerations.PsxExtensions;
                                continue;
                            }

                            if (line == "[PCExtensions]")
                            {
                                lastBlock = Enumerations.PcExtensions;
                                continue;
                            }

                            if (line == "[Language]")
                            {
                                lastBlock = Enumerations.Language;
                                continue;
                            }

                            if (line == "[Options]")
                            {
                                lastBlock = Enumerations.Options;
                                continue;
                            }

                            if (line == "[Title]")
                            {
                                lastBlock = Enumerations.Title;
                                lastLevel = new LevelScript(true);
                                _levels.Add(lastLevel);
                                continue;
                            }

                            if (line == "[Level]")
                            {
                                lastBlock = Enumerations.Level;
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

                        if (lastBlock == Enumerations.PsxExtensions)
                        {
                            if (command == "Level")
                                _psxLevel = value;
                            else if (command == "Cut")
                                _psxCut = value;
                            else if (command == "FMV")
                                _psxFmv = value;
                        }

                        if (lastBlock == Enumerations.PcExtensions)
                        {
                            if (command == "Level")
                                _pcLevel = value;
                            else if (command == "Cut")
                                _pcCut = value;
                            else if (command == "FMV")
                                _pcFmv = value;
                        }

                        if (lastBlock == Enumerations.Language)
                        {
                            if (command == "File")
                            {
                                var tokensFile = value.Split(',');
                                if (tokensFile.Length < 2 || !File.Exists(_srcPath + "\\" + tokensFile[1]))
                                    continue;
                                _languageFiles.Add(tokensFile[1]);
                            }
                        }

                        if (lastBlock == Enumerations.Options)
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

                        if (lastBlock == Enumerations.Title || lastBlock == Enumerations.Level)
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
            string scriptPath = _dstPath + "\\Script.dat";

            if (File.Exists(scriptPath))
                File.Delete(scriptPath);

            using (var stream = File.OpenWrite(scriptPath))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // Prepare flags
                    short flags = 0;
                    if (_flyCheat) flags += 0x01;
                    if (_loadSave) flags += 0x02;
                    if (_title) flags += 0x04;
                    if (_playAnyLevel) flags += 0x08;

                    // Get unique level file names
                    short sizeOfLevelFileNames = 0;
                    foreach (var level in _levels)
                        if (!_levelFileNames.Contains(level.FileName))
                        {
                            _levelFileNames.Add(level.FileName);
                            sizeOfLevelFileNames += (short)(level.FileName.Length + 1);
                        }

                    writer.Write((short)flags);
                    writer.Write((short)0);
                    writer.Write((int)_inputTimeout);
                    writer.Write((byte)_security);

                    writer.Write((byte)_levels.Count);
                    writer.Write((byte)_levelFileNames.Count);
                    writer.Write((byte)0);
                    writer.Write((short)sizeOfLevelFileNames);

                    // Here we'll store the entire level bytecode size
                    int levelDataOffset = (int)writer.BaseStream.Position;
                    writer.Write((short)0);

                    // Write extensions
                    WriteRawString(writer, _psxLevel);
                    WriteRawString(writer, _psxFmv);
                    WriteRawString(writer, _psxCut);

                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);

                    WriteRawString(writer, _pcLevel);
                    WriteRawString(writer, _pcFmv);
                    WriteRawString(writer, _pcCut);

                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);

                    // Write file names offsets
                    short lastOffset = 0;
                    foreach (var fn in _levelFileNames)
                    {
                        writer.Write(lastOffset);
                        lastOffset += (short)(fn.Length + 1);
                    }

                    // Write file names
                    foreach (var fn in _levelFileNames)
                        WriteRawString(writer, fn);

                    // Now we have to build level data and store offsets
                    var compiledLevelScripts = new List<byte[]>();

                    lastOffset = 0;
                    foreach (var level in _levels)
                    {
                        writer.Write(lastOffset);
                        var data = BuildRawScriptData(level, _version);
                        compiledLevelScripts.Add(data);
                        lastOffset += (short)data.Length;
                    }
                    short sizeOfLevelData = lastOffset;

                    // Write level data
                    foreach (var data in compiledLevelScripts)
                        writer.Write(data);

                    // Write language files
                    foreach (var languageFile in _languageFiles)
                        WriteRawString(writer, languageFile.ToUpper().Replace(".TXT", ".DAT"));
                    writer.Write(0);


                    // Store level data size
                    writer.BaseStream.Seek(levelDataOffset, SeekOrigin.Begin);
                    writer.Write((short)sizeOfLevelData);
                }
            }

            // Now write languages files
            for (int i = 0; i < _languageFiles.Count; i++)
            {
                var languageFilePath = _dstPath + "\\" + _languageFiles[i].ToUpper().Replace(".TXT", ".DAT");
                using (var stream = File.OpenWrite(languageFilePath))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write((short)_strings[i].GeneralStrings.Count);
                        writer.Write((short)_strings[i].PsxStrings.Count);
                        writer.Write((short)_strings[i].PcStrings.Count);

                        short size = 0;
                        foreach (var str in _strings[i].GeneralStrings)
                            size += (short)(str.Length + 1);
                        writer.Write(size);

                        foreach (var str in _strings[i].PsxStrings)
                            size += (short)(str.Length + 1);
                        writer.Write(size);

                        foreach (var str in _strings[i].PcStrings)
                            size += (short)(str.Length + 1);
                        writer.Write(size);

                        short offset = 0;
                        foreach (var str in _strings[i].AllStrings)
                        {
                            writer.Write(offset);
                            offset += (short)(str.Length + 1);
                        }

                        foreach (string str in _strings[i].AllStrings)
                            WriteRawString(writer, GetTrueString(str), true);
                    }
                }
            }

            return true;
        }

        private byte[] GetTrueString(string str)
        {
            var ms = new MemoryStream();

            for (int i=0;i<str.Length;i++)
            {
                if (str[i]=='\\' && i<str.Length-1 && str[i+1]=='x')
                {
                    i += 2;

                    string hexCode = "";
                    while (str[i] != ' ' && i < str.Length - 1)
                    {
                        hexCode += str[i];
                        i++;
                    }

                    ms.WriteByte(Convert.ToByte("0x" + hexCode, 16));

                }
                else
                {
                    ms.WriteByte(Convert.ToByte(str[i]));
                }
            }

            return ms.ToArray();
        }

        private void WriteRawString(BinaryWriter writer, string value, bool encode = false)
        {
            var buffer = UTF8Encoding.ASCII.GetBytes(value);
            if (encode)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] ^= 0xA5;

            writer.Write(buffer);
            writer.Write((byte)0);
        }

        private void WriteRawString(BinaryWriter writer, byte[] value, bool encode = false)
        {
            if (encode)
                for (int i = 0; i < value.Length; i++)
                    value[i] ^= 0xA5;

            writer.Write(value);
            writer.Write((byte)0);
        }

        private int GetStringIndex(string str)
        {
            for (int i = 0; i < _strings[0].AllStrings.Count; i++)
                if (_strings[0].AllStrings[i] == str)
                    return i;
            return 0;
        }

        private int GetFileNameIndex(string str)
        {
            for (int i = 0; i < _levelFileNames.Count; i++)
                if (_levelFileNames[i] == str)
                    return i;
            return 0;
        }

        private byte[] BuildRawScriptData(LevelScript level, GameVersion version)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    short flags = 0;

                    foreach (var entry in level.Entries)
                    {
                        if (entry.Command.Name == "Puzzle")
                        {
                            writer.Write((byte)(0x9F + (byte)entry.Parameters[0] - 1));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "Key")
                        {
                            writer.Write((byte)(0x93 + (byte)entry.Parameters[0] - 1));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "Pickup")
                        {
                            writer.Write((byte)(0xAB + (byte)entry.Parameters[0] - 1));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "Examine")
                        {
                            writer.Write((byte)(0xAF + (byte)entry.Parameters[0]) - 1);
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        if (entry.Command.Name == "PuzzleCombo")
                        {
                            writer.Write((byte)(0xC2 + (byte)(2 * (byte)entry.Parameters[0] - 2) + (byte)((byte)entry.Parameters[1] - 1)));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[2]));
                            for (int i = 3; i < 9; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "KeyCombo")
                        {
                            writer.Write((byte)(0xB2 + (byte)(2 * (byte)entry.Parameters[0] - 2) + (byte)((byte)entry.Parameters[1] - 1)));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[2]));
                            for (int i = 3; i < 9; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "PickupCombo")
                        {
                            writer.Write((byte)(0xD2 + (byte)(2 * (byte)entry.Parameters[0] - 2) + (byte)((byte)entry.Parameters[1] - 1)));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[2]));
                            for (int i = 3; i < 9; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "FMV")
                        {
                            writer.Write((byte)0x80);
                            writer.Write((byte)entry.Parameters[0]);
                        }
                        else if (entry.Command.Name == "UVrotate")
                        {
                            writer.Write((byte)0x8B);
                            writer.Write((byte)entry.Parameters[0]);
                        }
                        else if (entry.Command.Name == "Cut")
                        {
                            writer.Write((byte)0x84);
                            writer.Write((byte)entry.Parameters[0]);
                        }
                        else if (version == GameVersion.TR5 && entry.Command.Name == "GiveItem")
                        {
                            writer.Write((byte)0xD9);
                            writer.Write((byte)entry.Parameters[0]);
                        }
                        else if (version == GameVersion.TR5 && entry.Command.Name == "LoseItem")
                        {
                            writer.Write((byte)0xDA);
                            writer.Write((byte)entry.Parameters[0]);
                        }
                        else if (entry.Command.Name == "Legend")
                        {
                            writer.Write((byte)0x8C);
                            writer.Write((byte)GetStringIndex((string)entry.Parameters[0]));
                        }
                        else if (entry.Command.Name == "Layer1")
                        {
                            writer.Write((byte)0x89);
                            writer.Write((byte)entry.Parameters[0]);
                            writer.Write((byte)entry.Parameters[1]);
                            writer.Write((byte)entry.Parameters[2]);
                            writer.Write((sbyte)entry.Parameters[3]);
                            flags |= 0x08;
                        }
                        else if (entry.Command.Name == "Layer2")
                        {
                            writer.Write((byte)0x8A);
                            writer.Write((byte)entry.Parameters[0]);
                            writer.Write((byte)entry.Parameters[1]);
                            writer.Write((byte)entry.Parameters[2]);
                            writer.Write((sbyte)entry.Parameters[3]);
                            flags |= 0x10;
                        }
                        else if (entry.Command.Name == "ResidentCut")
                        {
                            writer.Write((byte)0x85 + (byte)((byte)entry.Parameters[0] - 1));
                            writer.Write((byte)entry.Parameters[1]);
                        }
                        else if (entry.Command.Name == "Fog")
                        {
                            writer.Write((byte)0x8F);
                            writer.Write((byte)entry.Parameters[0]);
                            writer.Write((byte)entry.Parameters[1]);
                            writer.Write((byte)entry.Parameters[2]);
                        }
                        else if (entry.Command.Name == "Horizon")
                            flags |= 0x04;
                        else if (entry.Command.Name == "ColAddHorizon")
                            flags |= 0x200;
                        else if (entry.Command.Name == "Lightning")
                            flags |= 0x40;
                        else if (entry.Command.Name == "Pulse")
                            flags |= 0x100;
                        else if (entry.Command.Name == "StarField")
                            flags |= 0x20;
                        else if (entry.Command.Name == "Weather")
                            flags |= 0x02;
                        else if (entry.Command.Name == "Timer")
                            flags |= 0x1000;
                        else if (entry.Command.Name == "Train")
                            flags |= 0x80;
                        else if (entry.Command.Name == "YoungLara")
                            flags |= 0x01;
                        else if (entry.Command.Name == "RemoveAmulet")
                            flags |= 0x4000;
                        else if (entry.Command.Name == "Train")
                            flags |= 0x80;
                        /*else if (entry.Command.Name == "NoLevel")
                            flags |= (short)0x8000;*/
                        else if (entry.Command.Name == "LensFlare")
                        {
                            writer.Write((byte)0x8D);
                            writer.Write((byte)((int)entry.Parameters[0] / 256));
                            writer.Write((byte)((int)entry.Parameters[1] / 256));
                            writer.Write((byte)((int)entry.Parameters[2] / 256));
                            writer.Write((byte)entry.Parameters[3]);
                            writer.Write((byte)entry.Parameters[4]);
                            writer.Write((byte)entry.Parameters[5]);
                            flags |= 0x800;
                        }
                        else if (entry.Command.Name == "Mirror")
                        {
                            writer.Write((byte)0x8E);
                            writer.Write((byte)entry.Parameters[0]);
                            writer.Write((int)((short)entry.Parameters[1]));
                        }
                        else if (entry.Command.Name == "LoadCamera")
                        {
                            writer.Write((byte)0x91);
                            writer.Write((int)entry.Parameters[0]);
                            writer.Write((int)entry.Parameters[1]);
                            writer.Write((int)entry.Parameters[2]);
                            writer.Write((int)entry.Parameters[3]);
                            writer.Write((int)entry.Parameters[4]);
                            writer.Write((int)entry.Parameters[5]);
                            writer.Write((byte)entry.Parameters[6]);
                        }
                        else if (entry.Command.Name == "ResetHUB")
                        {
                            writer.Write((byte)0x92);
                            writer.Write((byte)entry.Parameters[0]);
                        }
                    }

                    if (level.IsTitle)
                    {
                        writer.Write((byte)0x82);
                        writer.Write((short)flags);
                        writer.Write((byte)0);
                        writer.Write((byte)level.AudioTrack);
                    }
                    else
                    {
                        writer.Write((byte)0x81);
                        writer.Write((byte)GetStringIndex(level.Name));
                        writer.Write((short)flags);
                        writer.Write((byte)GetFileNameIndex(level.FileName));
                        writer.Write((byte)level.AudioTrack);
                    }

                    // End of level script
                    writer.Write((byte)0x83);
                }

                return ms.ToArray();
            }
        }
    }
}
