using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData;

namespace TombLib.NG
{
    public enum NgParameterKind
    {
        Empty,
        AnyNumber,
        FixedEnumeration,
        LinearModel,
        Choice,
        MoveablesInLevel, // MOVEABLES
        StaticsInLevel, // STATIC_LIST
        CamerasInLevel, // CAMERA_EFFECTS
        SinksInLevel, // SINK_LIST
        FlybyCamerasInLevel, // FLYBY_LIST
        Rooms255, // ROOMS_255
        SoundEffectsA, // SOUND_EFFECT_A
        SoundEffectsB, // SOUND_EFFECT_B
        Sfx1024, // SFX_1024
        PcStringsList, // PC_STRING_LIST
        PsxStringsList, // PSX_STRING_LIST
        NgStringsList255, // NG_STRING_LIST_255
        NgStringsAll, // NG_STRING_LIST_ALL
        StringsList255, // STRING_LIST_255
        WadSlots, // WAD-SLOTS
        StaticsSlots, // STATIC_SLOTS
        LaraStartPosOcb, // LARA_POS_OCB
        LuaScript
    }

    public struct NgLinearParameter
    {
        public string FixedStr;
        public decimal Add;
        public decimal Factor;
    }

    public struct NgLinearModel
    {
        public List<NgLinearParameter> Parameters;
        public ushort Start;
        public ushort EndInclusive;

        public string ToString(ushort index)
        {
            string result = "";
            foreach (NgLinearParameter linearParameter in Parameters)
                if (linearParameter.FixedStr != null)
                    result += linearParameter.FixedStr;
                else
                    result += linearParameter.Factor * index + linearParameter.Add;
            return result;
        }
    }

    public struct NgParameterRange : IEquatable<NgParameterRange>
    {
        public NgParameterKind Kind { get; }
        public IDictionary<ushort, TriggerParameterUshort> FixedEnumeration { get; }
        public NgLinearModel? LinearModel { get; }
        public IReadOnlyList<NgParameterRange> Choices { get; }

        public NgParameterRange(NgParameterKind kind)
        {
            if (kind == NgParameterKind.FixedEnumeration)
                kind = NgParameterKind.Empty;
            Kind = kind;
            FixedEnumeration = null;
            LinearModel = null;
            Choices = null;
        }

        public NgParameterRange(IDictionary<ushort, TriggerParameterUshort> fixedEnumeration)
        {
            LinearModel = null;
            Choices = null;
            if (fixedEnumeration == null || fixedEnumeration.Count == 0)
            {
                Kind = NgParameterKind.Empty;
                FixedEnumeration = null;
            }
            else
            {
                Kind = NgParameterKind.FixedEnumeration;
                FixedEnumeration = fixedEnumeration;
            }
        }

        public NgParameterRange(NgLinearModel linearModel)
        {
            Choices = null;
            FixedEnumeration = null;
            Kind = NgParameterKind.LinearModel;
            LinearModel = linearModel;
        }

        public NgParameterRange(IEnumerable<NgParameterRange> choice)
        {
            NgParameterRange[] choice2 = choice.Where(e => !e.IsEmpty).ToArray();
            if (choice2.Length == 0)
            {
                Kind = NgParameterKind.Empty;
                FixedEnumeration = null;
                LinearModel = null;
                Choices = null;
            }
            else if (choice2.Length == 1)
            {
                Kind = choice2[0].Kind;
                FixedEnumeration = choice2[0].FixedEnumeration;
                LinearModel = choice2[0].LinearModel;
                Choices = choice2[0].Choices;
            }
            else
            {
                Kind = NgParameterKind.Choice;
                FixedEnumeration = null;
                LinearModel = null;
                Choices = choice2;
            }
        }

        public bool IsObject
        {
            get
            {
                switch (Kind)
                {
                    case NgParameterKind.MoveablesInLevel:
                    case NgParameterKind.StaticsInLevel:
                    case NgParameterKind.CamerasInLevel:
                    case NgParameterKind.SinksInLevel:
                    case NgParameterKind.FlybyCamerasInLevel:
                        return true;
                    case NgParameterKind.Choice:
                        foreach (var choice in Choices)
                            if (choice.IsObject)
                                return true;
                        return false;
                    default:
                        return false;
                }
            }
        }

        public bool IsRoom
        {
            get
            {
                switch (Kind)
                {
                    case NgParameterKind.Rooms255:
                        return true;
                    case NgParameterKind.Choice:
                        foreach (var choice in Choices)
                            if (choice.IsRoom)
                                return true;
                        return false;
                    default:
                        return false;
                }
            }
        }

        public bool IsEmpty => Kind == NgParameterKind.Empty;

        public bool IsNumber => !(IsEmpty || IsObject || IsRoom);

        public bool ParameterMatches(ITriggerParameter parameter, bool nullResult)
        {
            if (parameter == null)
                return nullResult || Kind == NgParameterKind.Empty;

            switch (Kind)
            {
                case NgParameterKind.Empty:
                    return parameter == null;

                case NgParameterKind.Choice:
                    foreach (NgParameterRange choice in Choices)
                        if (choice.ParameterMatches(parameter, false))
                            return true;
                    return false;

                case NgParameterKind.MoveablesInLevel:
                    return parameter is MoveableInstance;
                case NgParameterKind.StaticsInLevel:
                    return parameter is StaticInstance;
                case NgParameterKind.CamerasInLevel:
                    return parameter is CameraInstance;
                case NgParameterKind.SinksInLevel:
                    return parameter is SinkInstance;
                case NgParameterKind.FlybyCamerasInLevel:
                    return parameter is FlybyCameraInstance;

                case NgParameterKind.Rooms255:
                    return parameter is Room;

                default:
                    if (IsNumber)
                        return parameter is TriggerParameterUshort;
                    else
                        return false;
            }
        }

        public static bool operator ==(NgParameterRange first, NgParameterRange second)
        {
            if (first.Kind != second.Kind)
                return false;
            if (first.FixedEnumeration == null != (second.FixedEnumeration == null))
                return false;
            if (first.FixedEnumeration != null && !first.FixedEnumeration.SequenceEqual(second.FixedEnumeration))
                return false;
            if (first.LinearModel.HasValue != (second.LinearModel.HasValue))
                return false;
            if (first.LinearModel.HasValue && !first.LinearModel.Value.Equals(second.LinearModel.Value))
                return false;

            return true;
        }
        public static bool operator !=(NgParameterRange first, NgParameterRange second) => !(first == second);
        public bool Equals(NgParameterRange other) => this == other;
        public override bool Equals(object other)
        {
            if (!(other is NgParameterRange))
                return false;
            return this == (NgParameterRange)other;
        }
        public override int GetHashCode() => base.GetHashCode();

        public IEnumerable<ITriggerParameter> BuildList(Level level)
        {
            Func<int, TriggerParameterUshort> formatSounds = i =>
            {
                Wad.WadFixedSoundInfo fixedSoundInfo = level?.Settings?.WadTryGetFixedSoundInfo(new Wad.WadFixedSoundInfoId((ushort)i));
                if (fixedSoundInfo != null)
                    return new TriggerParameterUshort((ushort)i, i + ": " + fixedSoundInfo.SoundInfo.Name);
                else
                    return new TriggerParameterUshort((ushort)i, i + ": --- Not present ---");
            };

            switch (Kind)
            {
                case NgParameterKind.Empty:
                    return new ITriggerParameter[0];

                case NgParameterKind.AnyNumber:
                case NgParameterKind.LuaScript:
                    return null;

                case NgParameterKind.FixedEnumeration:
                    return FixedEnumeration.Values;

                case NgParameterKind.LinearModel:
                    NgLinearModel linearModel = LinearModel.Value;
                    return Enumerable.Range(linearModel.Start, linearModel.EndInclusive + 1 - linearModel.Start)
                        .Select(i => new TriggerParameterUshort((ushort)i, linearModel.ToString((ushort)i)));

                case NgParameterKind.Choice:
                    return Choices.Aggregate(
                        Enumerable.Empty<ITriggerParameter>(),
                        (last, newChoice) => last.Concat(newChoice.BuildList(level)));

                case NgParameterKind.MoveablesInLevel:
                    return level.Rooms.Where(room => room != null).SelectMany(room => room.Objects).OfType<MoveableInstance>();

                case NgParameterKind.StaticsInLevel:
                    return level.Rooms.Where(room => room != null).SelectMany(room => room.Objects).OfType<StaticInstance>();

                case NgParameterKind.CamerasInLevel:
                    return level.Rooms.Where(room => room != null).SelectMany(room => room.Objects).OfType<CameraInstance>();

                case NgParameterKind.SinksInLevel:
                    return level.Rooms.Where(room => room != null).SelectMany(room => room.Objects).OfType<SinkInstance>();

                case NgParameterKind.FlybyCamerasInLevel:
                    return level.Rooms.Where(room => room != null).SelectMany(room => room.Objects).OfType<FlybyCameraInstance>();

                case NgParameterKind.Rooms255:
                    return level.Rooms.Where(room => room != null);

                case NgParameterKind.SoundEffectsA:
                    return Enumerable.Range(0, 256).Select(formatSounds);

                case NgParameterKind.SoundEffectsB:
                    return Enumerable.Range(256, 512).Select(formatSounds);

                case NgParameterKind.Sfx1024:
                    return Enumerable.Range(0, 1024).Select(formatSounds);

                case NgParameterKind.PcStringsList:
                    return LoadStringsFromTxt(level, "PCStrings");

                case NgParameterKind.PsxStringsList:
                    return LoadStringsFromTxt(level, "PSXStrings");

                case NgParameterKind.NgStringsList255:
                    return LoadStringsFromTxt(level, "ExtraNG", 256);

                case NgParameterKind.NgStringsAll:
                    return LoadStringsFromTxt(level, "ExtraNG");

                case NgParameterKind.StringsList255:
                    return LoadStringsFromTxt(level, "Strings", 256);

                case NgParameterKind.WadSlots:
                    if (level?.Settings == null)
                        return new ITriggerParameter[0];
                    return level.Settings.WadGetAllMoveables().Select(p => new TriggerParameterUshort(checked((ushort)p.Key.TypeId), p.Value.ToString()));

                case NgParameterKind.StaticsSlots:
                    if (level?.Settings == null)
                        return new ITriggerParameter[0];
                    return level.Settings.WadGetAllStatics().Select(p => new TriggerParameterUshort(checked((ushort)p.Key.TypeId), p.Value.ToString()));

                case NgParameterKind.LaraStartPosOcb:
                    return level.Rooms.Where(room => room != null)
                        .SelectMany(room => room.Objects)
                        .OfType<MoveableInstance>().Where(obj => obj.WadObjectId.TypeId == 406) // Lara start pos
                        .Select(obj => new TriggerParameterUshort(unchecked((ushort)obj.Ocb), obj));

                default:
                    throw new ArgumentException("Unknown NgListKind \"" + Kind + "\"");
            }
        }

        private static IEnumerable<TriggerParameterUshort> LoadStringsFromTxt(Level level, string block, int max = 1024)
        {
            var path = Path.Combine(level.Settings.MakeAbsolute(level.Settings.ScriptDirectory), "english.txt");
            try
            {
                using (var reader = new StreamReader(path))
                {
                    var foundBlock = false;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Trim();
                        if (line.StartsWith("["))
                        {
                            line = line.Replace("[", "").Replace("]", "");
                            if (line == block)
                            {
                                foundBlock = true;
                                break;
                            }
                        }
                    }

                    if (!foundBlock)
                        throw new Exception("Block \"" + block + "\" not found in language file \"" + path + "\"");

                    // Read strings of block until end of stream or next block or limit reached
                    var result = new SortedList<int, TriggerParameterUshort>();
                    while (!reader.EndOfStream && result.Count <= max)
                    {
                        var line = reader.ReadLine().Trim();
                        if (line.StartsWith("["))
                            break;
                        result.Add(result.Count, new TriggerParameterUshort(checked((ushort)result.Count), line));
                    }
                    return result.Values;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to open language file \"" + path + "\" for block \"" + block + "\"", e);
            }
        }
    }
}
