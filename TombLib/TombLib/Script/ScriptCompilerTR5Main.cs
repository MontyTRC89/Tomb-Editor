using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;

namespace TombLib.Script
{
    public class ScriptCompilerTombEngine : IScriptCompiler
    {
        private class LanguageStrings
        {
            public string FileName { get; set; }
            public string Name { get; set; }
            public List<string> Strings { get; set; }

            public LanguageStrings(string fileName, string name)
            {
                FileName = fileName;
                Name = name;
                Strings = new List<string>();
            }
        }

        private static readonly ChunkId TombEngineFlags = ChunkId.FromString("TombEngineFlags");
        private static readonly ChunkId TombEngineLevel = ChunkId.FromString("TombEngineLevel");
        private static readonly ChunkId TombEngineLevelFlags = ChunkId.FromString("TombEngineLevelFlags");
        private static readonly ChunkId TombEngineLevelInfo = ChunkId.FromString("TombEngineLevelInfo");
        private static readonly ChunkId TombEngineTitleBackground = ChunkId.FromString("TombEngineTitleBackground");
        private static readonly ChunkId TombEngineLevelPuzzle = ChunkId.FromString("TombEngineLevelPuzzle");
        private static readonly ChunkId TombEngineLevelKey = ChunkId.FromString("TombEngineLevelKey");
        private static readonly ChunkId TombEngineLevelPuzzleCombo = ChunkId.FromString("TombEngineLevelPuzzleCombo");
        private static readonly ChunkId TombEngineLevelKeyCombo = ChunkId.FromString("TombEngineLevelKeyCombo");
        private static readonly ChunkId TombEngineLevelPickup = ChunkId.FromString("TombEngineLevelPickup");
        private static readonly ChunkId TombEngineLevelPickupCombo = ChunkId.FromString("TombEngineLevelPickupCombo");
        private static readonly ChunkId TombEngineLevelExamine = ChunkId.FromString("TombEngineLevelExamine");
        private static readonly ChunkId TombEngineLevelLayer = ChunkId.FromString("TombEngineLevelLayer");
        private static readonly ChunkId TombEngineLevelLuaEvent = ChunkId.FromString("TombEngineLevelLuaEvent");
        private static readonly ChunkId TombEngineLevelLegend = ChunkId.FromString("TombEngineLevelLegend");
        private static readonly ChunkId TombEngineAudioTracks = ChunkId.FromString("TombEngineAudioTracks");
        private static readonly ChunkId TombEngineStrings = ChunkId.FromString("TombEngineStrings");

        private string _srcPath;
        private string _dstPath;

        private List<LevelScript> _levels;
        private List<LanguageStrings> _languageStrings;
        private List<string> _levelFileNames;
        private List<string> _tracks;

        private bool _loadSave;
        private bool _title;
        private bool _playAnyLevel;
        private bool _flyCheat;
        private bool _diagnostics;
        private int _levelFarView;
        private string _intro;

        public ScriptCompilerTombEngine()
        {

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

            _languageStrings = new List<LanguageStrings>();
            _levels = new List<LevelScript>();
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

                        if (lastBlock == Enumerations.Language)
                        {
                            if (command == "File")
                            {
                                var tokensFile = value.Split(',');
                                if (tokensFile.Length < 2)
                                    continue;
                                _languageStrings.Add(new LanguageStrings(tokensFile[2], tokensFile[1]));
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
                            else if (command == "Diagnostics")
                                _diagnostics = (value == "ENABLED");
                            else if (command == "LevelFarView")
                                _levelFarView = int.Parse(value);
                            else if (command == "Intro")
                                _intro = value;
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
                                if (entry.Command.Name == "Background")
                                    lastLevel.Background = (string)entry.Parameters[0];
                                else if (entry.Command.Name == "AmbientTrack")
                                    lastLevel.AudioTrack = byte.Parse(entry.Parameters[0].ToString());
                                else if (entry.Command.Name == "LaraType")
                                    lastLevel.LaraType = GetLaraType((string)entry.Parameters[0]);
                                else if (entry.Command.Name == "Horizon")
                                    lastLevel.Horizon = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "ResetInventory")
                                    lastLevel.ResetInventory = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "Sky")
                                    lastLevel.Sky = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "ColAddHorizon")
                                    lastLevel.ColAddHorizon = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "Lightning")
                                    lastLevel.Lightning = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "UVRotate")
                                    lastLevel.UVRotate = (byte)entry.Parameters[0];
                                else if (entry.Command.Name == "LevelFarView")
                                    lastLevel.LevelFarView = (int)entry.Parameters[0];
                                else if (entry.Command.Name == "LoadScreen")
                                    lastLevel.LoadScreen = (string)entry.Parameters[0];
                                else if (entry.Command.Name == "LevelFile")
                                    lastLevel.FileName = (string)entry.Parameters[0];
                                else if (entry.Command.Name == "Rumble")
                                    lastLevel.Rumble = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "UnlimitedAir")
                                    lastLevel.UnlimitedAir = (entry.Parameters[0].ToString() == "True");
                                else if (entry.Command.Name == "Weather")
                                {
                                    if (entry.Parameters[0].ToString() == "RAIN")
                                        lastLevel.Weather = Weather.Rain;
                                    else if (entry.Parameters[0].ToString() == "SNOW")
                                        lastLevel.Weather = Weather.Snow;
                                }
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

                for (int i = 0; i < _languageStrings.Count; i++)
                { 
                    string languageFilePath = _srcPath + "\\" + _languageStrings[i].FileName;
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

                            _languageStrings[i].Strings.Add(line);
                        }
                    }
                }

                _tracks = new List<string>();
                using (var reader = new StreamReader(File.OpenRead(_srcPath + "\\" + "Tracks.txt")))
                {
                    while (reader.EndOfStream == false)
                    {
                        _tracks.Add(reader.ReadLine());
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int GetLaraType(string s)
        {
            if (s == "LARA_NORMAL")
                return 0;
            else if (s == "LARA_YOUNG")
                return 1;
            else if (s == "LARA_RUSSIA")
                return 2;
            else if (s == "LARA_CATSUIT")
                return 3;
            return 0;
        }

        private bool BuildScripts()
        {
            string scriptPath = _dstPath + "\\Script.dat";

            if (File.Exists(scriptPath))
                File.Delete(scriptPath);

            using (var stream = File.OpenWrite(scriptPath))
            {
                var chunkIO = new ChunkWriter(new byte[] { 0x54, 0x52, 0x35, 0x4D }, new BinaryWriterFast(stream));

                // Main flags
                chunkIO.WriteChunk(TombEngineFlags, () =>
                {
                    LEB128.Write(chunkIO.Raw, (_loadSave ? 1 : 0));
                    LEB128.Write(chunkIO.Raw, (_playAnyLevel ? 1 : 0));
                    LEB128.Write(chunkIO.Raw, (_flyCheat ? 1 : 0));
                    LEB128.Write(chunkIO.Raw, (_diagnostics ? 1 : 0));
                    LEB128.Write(chunkIO.Raw, _levelFarView);
                    chunkIO.Raw.WriteStringUTF8(_intro);
                });

                // Game strings
                foreach (var stringsObj in _languageStrings)
                {
                    chunkIO.WriteChunk(TombEngineStrings, () =>
                    {
                        chunkIO.Raw.WriteStringUTF8(stringsObj.Name);
                        LEB128.Write(chunkIO.Raw, stringsObj.Strings.Count);
                        foreach (var str in stringsObj.Strings)
                            chunkIO.Raw.WriteStringUTF8(str);
                    });
                }

                // Audio tracks
                chunkIO.WriteChunk(TombEngineAudioTracks, () =>
                {
                    LEB128.Write(chunkIO.Raw, _tracks.Count);
                    foreach (var track in _tracks)
                        chunkIO.Raw.WriteStringUTF8(track);
                });

                // Levels
                foreach (var level in _levels)
                {
                    chunkIO.WriteChunkWithChildren(TombEngineLevel, () =>
                    {
                        // Level informations
                        chunkIO.WriteChunk(TombEngineLevelInfo, () =>
                        {
                            chunkIO.Raw.WriteStringUTF8(level.FileName);
                            chunkIO.Raw.WriteStringUTF8(level.LoadScreen);
                            chunkIO.Raw.WriteStringUTF8(level.Background);
                            LEB128.Write(chunkIO.Raw, GetStringIndex(level.Name));
                            LEB128.Write(chunkIO.Raw, level.AudioTrack);
                        });

                        // Level flags
                        chunkIO.WriteChunk(TombEngineLevelFlags, () =>
                        {
                            LEB128.Write(chunkIO.Raw, (level.Horizon ? 1 : 0));
                            LEB128.Write(chunkIO.Raw, (level.Sky ? 1 : 0));
                            LEB128.Write(chunkIO.Raw, (level.ColAddHorizon ? 1 : 0));
                            LEB128.Write(chunkIO.Raw, (level.Lightning ? 1 : 0));
                            LEB128.Write(chunkIO.Raw, (level.ResetInventory ? 1 : 0));
                            LEB128.Write(chunkIO.Raw, level.LaraType);
                            LEB128.Write(chunkIO.Raw, level.UVRotate);
                            LEB128.Write(chunkIO.Raw, (level.LevelFarView != 0 ? level.LevelFarView : _levelFarView));
                            LEB128.Write(chunkIO.Raw, (level.Rumble ? 1 : 0));
                            LEB128.Write(chunkIO.Raw, (byte)(level.Weather));
                            LEB128.Write(chunkIO.Raw, (level.UnlimitedAir ? 1 : 0));
                        });

                        // Puzzles and various things
                        foreach (var entry in level.Entries)
                        {
                            if (entry.Command.Name == "Puzzle")
                            {
                                chunkIO.WriteChunk(TombEngineLevelPuzzle, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[1].ToString()));
                                    for (int j = 2; j < 8; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "Key")
                            {
                                chunkIO.WriteChunk(TombEngineLevelKey, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[1].ToString()));
                                    for (int j = 2; j < 8; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "Pickup")
                            {
                                chunkIO.WriteChunk(TombEngineLevelPickup, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[1].ToString()));
                                    for (int j = 2; j < 8; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            if (entry.Command.Name == "Examine")
                            {
                                chunkIO.WriteChunk(TombEngineLevelExamine, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[1].ToString()));
                                    for (int j = 2; j < 8; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            if (entry.Command.Name == "PuzzleCombo")
                            {
                                chunkIO.WriteChunk(TombEngineLevelPuzzleCombo, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[1].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[2].ToString()));
                                    for (int j = 3; j < 9; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "KeyCombo")
                            {
                                chunkIO.WriteChunk(TombEngineLevelKeyCombo, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[1].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[2].ToString()));
                                    for (int j = 3; j < 9; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "PickupCombo")
                            {
                                chunkIO.WriteChunk(TombEngineLevelPickupCombo, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[1].ToString()));
                                    LEB128.Write(chunkIO.Raw, (int)GetStringIndex(entry.Parameters[2].ToString()));
                                    for (int j = 3; j < 9; j++)
                                        LEB128.Write(chunkIO.Raw, short.Parse(entry.Parameters[j].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "Layer1")
                            {
                                chunkIO.WriteChunk(TombEngineLevelLayer, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, 1);
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[1].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[2].ToString()));
                                    LEB128.Write(chunkIO.Raw, sbyte.Parse(entry.Parameters[3].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "Layer2")
                            {
                                chunkIO.WriteChunk(TombEngineLevelLayer, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, 2);
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[1].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[2].ToString()));
                                    LEB128.Write(chunkIO.Raw, sbyte.Parse(entry.Parameters[3].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "DistantFog")
                            {
                                chunkIO.WriteChunk(TombEngineLevelLayer, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[0].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[1].ToString()));
                                    LEB128.Write(chunkIO.Raw, byte.Parse(entry.Parameters[2].ToString()));
                                    LEB128.Write(chunkIO.Raw, int.Parse(entry.Parameters[3].ToString()));
                                });
                            }
                            else if (entry.Command.Name == "OnLevelStart" ||
                                     entry.Command.Name == "OnLevelFinish" ||
                                     entry.Command.Name == "OnLoadGame" ||
                                     entry.Command.Name == "OnSaveGame" ||
                                     entry.Command.Name == "OnLaraDeath" ||
                                     entry.Command.Name == "OnLevelControl" ||
                                     entry.Command.Name == "OnBeginFrame")
                            {
                                chunkIO.WriteChunk(TombEngineLevelLuaEvent, () =>
                                {
                                    chunkIO.Raw.WriteStringUTF8(entry.Command.Name);
                                    chunkIO.Raw.WriteStringUTF8(entry.Parameters[0].ToString());
                                });
                            }
                        }
                    });
                }

                // EOF
                chunkIO.Raw.Write((int)0);

                chunkIO.Raw.Flush();
            }

            return true;
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

        private int GetStringIndex(string str)
        {
            var strings = _languageStrings[0].Strings;
            for (int i = 0; i < strings.Count; i++)
                if (strings[i] == str)
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
