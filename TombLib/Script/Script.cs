using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public class Script
    {
        // File extensions
        public string PsxLevel { get; set; }
        public string PsxCut { get; set; }
        public string PsxFmv { get; set; }

        public string PcLevel { get; set; }
        public string PcCut { get; set; }
        public string PcFmv { get; set; }

        // Language files
        public List<string> LanguageFiles { get; private set; } = new List<string>();

        // Main flags and settings
        public bool LoadSave { get; set; }
        public bool Title { get; set; }
        public bool PlayAnyLevel { get; set; }
        public bool FlyCheat { get; set; }
        public bool DemoDisc { get; set; }
        public bool Diagnostic { get; set; }
        public int InputTimeout { get; set; }
        public byte SecurityByte { get; set; }

        // Levels
        public List<LevelScript> Levels { get; private set; } = new List<LevelScript>();
        
        // English strings
        public List<LanguageScript> Strings { get; private set; } = new List<LanguageScript>();

        // Level file names
        private List<string> _levelFileNames = new List<string>();

        public static Script LoadFromTxt(string fileName)
        {
            LevelScriptCatalog.LoadCatalog();

            try
            {
                var script = new Script();

                using (var reader = new StreamReader(File.OpenRead(fileName)))
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
                                script.Levels.Add(lastLevel);
                                continue;
                            }

                            if (line == "[Level]")
                            {
                                lastBlock = ScriptBlocks.Level;
                                lastLevel = new LevelScript(false);
                                script.Levels.Add(lastLevel);
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
                                script.PsxLevel = value;
                            else if (command == "Cut")
                                script.PsxCut = value;
                            else if (command == "FMV")
                                script.PsxFmv = value;
                        }

                        if (lastBlock == ScriptBlocks.PcExtensions)
                        {
                            if (command == "Level")
                                script.PcLevel = value;
                            else if (command == "Cut")
                                script.PcCut = value;
                            else if (command == "FMV")
                                script.PcFmv = value;
                        }

                        if (lastBlock == ScriptBlocks.Language)
                        {
                            if (command == "File")
                            {
                                var tokensFile = value.Split(',');
                                if (tokensFile.Length < 2)
                                    continue;
                                script.LanguageFiles.Add(tokensFile[1]);
                            }
                        }

                        if (lastBlock == ScriptBlocks.Options)
                        {
                            if (command == "LoadSave")
                                script.LoadSave = (value == "ENABLED");
                            else if (command == "Title")
                                script.Title = (value == "ENABLED");
                            else if (command == "PlayAnyLevel")
                                script.PlayAnyLevel = (value == "ENABLED");
                            else if (command == "FlyCheat")
                                script.FlyCheat = (value == "ENABLED");
                            else if (command == "DemoDisc")
                                script.DemoDisc = (value == "ENABLED");
                            else if (command == "InputTimeout")
                                script.InputTimeout = int.Parse(value);
                            else if (command == "Security")
                                script.SecurityByte = Convert.ToByte(value.Replace("$", ""), 16);
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

                foreach (var languageFile in script.LanguageFiles)
                {
                    string languageFilePath = Path.GetDirectoryName(fileName) + "\\" + languageFile;
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
                        script.Strings.Add(strings);
                    }
                }

                return script;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool CompileScript(string path)
        {
            string scriptPath = path + "\\SCRIPT.DAT";

            if (File.Exists(scriptPath))
                File.Delete(scriptPath);

            using (var stream = File.OpenWrite(scriptPath))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // Prepare flags
                    short flags = 0;
                    if (FlyCheat) flags += 0x01;
                    if (LoadSave) flags += 0x02;
                    if (Title) flags += 0x04;
                    if (PlayAnyLevel) flags += 0x08;

                    // Get unique level file names
                    short sizeOfLevelFileNames = 0;
                    foreach (var level in Levels)
                        if (!_levelFileNames.Contains(level.FileName))
                        {
                            _levelFileNames.Add(level.FileName);
                            sizeOfLevelFileNames += (short)(level.FileName.Length + 1);
                        }

                    writer.Write((short)flags);
                    writer.Write((short)0);
                    writer.Write((int)InputTimeout);
                    writer.Write((byte)SecurityByte);
                    writer.Write((byte)Levels.Count);
                    writer.Write((byte)_levelFileNames.Count);
                    writer.Write((byte)0);
                    writer.Write((short)sizeOfLevelFileNames);

                    // Here we'll store the entire level bytecode size
                    int levelDataOffset = (int)writer.BaseStream.Position;
                    writer.Write((short)0);

                    // Write extensions
                    WriteRawString(writer, PsxLevel);
                    WriteRawString(writer, PsxFmv);
                    WriteRawString(writer, PsxCut);

                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0);

                    WriteRawString(writer, PcLevel);
                    WriteRawString(writer, PcFmv);
                    WriteRawString(writer, PcCut);

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
                    foreach (var level in Levels)
                    {
                        writer.Write(lastOffset);
                        var data = BuildRawScriptData(level);
                        compiledLevelScripts.Add(data);
                        lastOffset += (short)data.Length;
                    }
                    short sizeOfLevelData = lastOffset;

                    // Write level data
                    foreach (var data in compiledLevelScripts)
                        writer.Write(data);

                    // Write language files
                    foreach (var languageFile in LanguageFiles)
                        WriteRawString(writer, languageFile.ToUpper().Replace(".TXT", ".DAT"));

                    // Store level data size
                    writer.BaseStream.Seek(levelDataOffset, SeekOrigin.Begin);
                    writer.Write((short)sizeOfLevelData);
                }
            }

            // Now write languages files
            for (int i=0;i< LanguageFiles.Count;i++)
            {
                var languageFilePath = path + "\\" + LanguageFiles[i].ToUpper().Replace(".TXT", ".DAT");
                using (var stream = File.OpenWrite(languageFilePath))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write((short)Strings[i].GeneralStrings.Count);
                        writer.Write((short)Strings[i].PsxStrings.Count);
                        writer.Write((short)Strings[i].PcStrings.Count);

                        short size = 0;
                        foreach (var str in Strings[i].GeneralStrings)
                            size += (short)(str.Length + 1);
                        writer.Write(size);

                        foreach (var str in Strings[i].PsxStrings)
                            size += (short)(str.Length + 1);
                        writer.Write(size);

                        foreach (var str in Strings[i].PcStrings)
                            size += (short)(str.Length + 1);
                        writer.Write(size);

                        short offset = 0;
                        foreach (var str in Strings[i].AllStrings)
                        {
                            writer.Write(offset);
                            offset += (short)(str.Length + 1);
                        }

                        foreach (string str in Strings[i].AllStrings)
                            WriteRawString(writer, str, true);
                    }
                }
            }

            return true;
        }

        public static void Test()
        {
            using (var reader = new BinaryReader(File.OpenRead("E:\\trle\\script\\italian.dat")))
            using (var writer = new BinaryWriter(File.OpenWrite("E:\\trle\\script\\italian2.dat")))
                while(reader.BaseStream.Position<reader.BaseStream.Length)
                writer.Write((byte)(reader.ReadByte() ^ 0xA5));
        }

        private byte[] BuildRawScriptData(LevelScript level)
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
                            writer.Write((byte)(0x9F + (byte)entry.Parameters[0]));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "Key")
                        {
                            writer.Write((byte)(0x93 + (byte)entry.Parameters[0]));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "Pickup")
                        {
                            writer.Write((byte)(0xAB + (byte)entry.Parameters[0]));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "Examine")
                        {
                            writer.Write((byte)(0xAF + (byte)entry.Parameters[0]));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[1]));
                            for (int i = 2; i < 8; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        if (entry.Command.Name == "PuzzleCombo")
                        {
                            writer.Write((byte)(0xC2 + (byte)entry.Parameters[0] + (byte)((byte)entry.Parameters[1] - 1)));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[2]));
                            for (int i = 3; i < 9; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "KeyCombo")
                        {
                            writer.Write((byte)(0xB2 + (byte)entry.Parameters[0] + (byte)((byte)entry.Parameters[1] - 1)));
                            writer.Write((short)GetStringIndex((string)entry.Parameters[2]));
                            for (int i = 3; i < 9; i++)
                                writer.Write((short)entry.Parameters[i]);
                        }
                        else if (entry.Command.Name == "PickupCombo")
                        {
                            writer.Write((byte)(0xD2 + (byte)entry.Parameters[0] + (byte)((byte)entry.Parameters[1] - 1)));
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

        private void WriteRawString(BinaryWriter writer, string value, bool encode=false)
        {
            var buffer = UTF8Encoding.ASCII.GetBytes(value);
            if (encode)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] ^= 0xA5;

            writer.Write(buffer);
            writer.Write((byte)0);
        }

        private int GetStringIndex(string str)
        {
            for (int i = 0; i < Strings[0].AllStrings.Count; i++)
                if (Strings[0].AllStrings[i] == str)
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
    }
}
