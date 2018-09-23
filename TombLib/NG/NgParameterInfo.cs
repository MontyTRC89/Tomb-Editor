using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.NG
{
    public static class NgParameterInfo
    {
        public static IEnumerable<TriggerType> GetTriggerTypeRange(LevelSettings levelSettings)
        {
            yield return TriggerType.Trigger;
            yield return TriggerType.Pad;
            yield return TriggerType.Switch;
            yield return TriggerType.Key;
            yield return TriggerType.Pickup;
            yield return TriggerType.Heavy;
            yield return TriggerType.Antipad;
            yield return TriggerType.Combat;
            yield return TriggerType.Dummy;
            yield return TriggerType.Antitrigger;
            yield return TriggerType.HeavySwitch;
            yield return TriggerType.HeavyAntritrigger;
            if (levelSettings.GameVersion == GameVersion.TRNG)
                yield return TriggerType.ConditionNg;
        }

        public static IEnumerable<TriggerTargetType> GetTargetTypeRange(LevelSettings levelSettings, TriggerType triggerType)
        {
            if (triggerType == TriggerType.ConditionNg)
            {
                yield return TriggerTargetType.ParameterNg;
            }
            else
            {
                yield return TriggerTargetType.Object;
                yield return TriggerTargetType.Camera;
                yield return TriggerTargetType.Sink;
                yield return TriggerTargetType.FlipMap;
                yield return TriggerTargetType.FlipOn;
                yield return TriggerTargetType.FlipOff;
                yield return TriggerTargetType.Target;
                yield return TriggerTargetType.FinishLevel;
                yield return TriggerTargetType.PlayAudio;
                yield return TriggerTargetType.FlipEffect;
                yield return TriggerTargetType.Secret;
                yield return TriggerTargetType.FlyByCamera;
                if (levelSettings.GameVersion == GameVersion.TRNG)
                {
                    yield return TriggerTargetType.ActionNg;
                    yield return TriggerTargetType.FmvNg;
                    yield return TriggerTargetType.TimerfieldNg;
                }
                if (levelSettings.GameVersion == GameVersion.TR5Main)
                {
                    yield return TriggerTargetType.LuaScript;
                }
            }
        }

        public static NgParameterRange ToParameterRange(this IEnumerable @this)
        {
            var sortedDictionary = new SortedDictionary<ushort, TriggerParameterUshort>();
            foreach (object value in @this)
                sortedDictionary.Add((ushort)value, new TriggerParameterUshort((ushort)value, value.ToString()));
            return new NgParameterRange(sortedDictionary);
        }

        public static NgParameterRange GetTargetRange(LevelSettings levelSettings, TriggerType triggerType, TriggerTargetType targetType, ITriggerParameter timer)
        {
            switch (triggerType)
            {
                case TriggerType.ConditionNg:
                    if (!(timer is TriggerParameterUshort))
                        return new NgParameterRange(NgParameterKind.Empty);
                    NgTriggerSubtype conditionSubtriggerType = NgCatalog.ConditionTrigger.MainList.TryGetOrDefault(((TriggerParameterUshort)timer).Key);
                    return conditionSubtriggerType?.Target ?? new NgParameterRange(NgParameterKind.Empty);

                default:
                    switch (targetType)
                    {
                        case TriggerTargetType.Object:
                            return new NgParameterRange(NgParameterKind.MoveablesInLevel);

                        case TriggerTargetType.Camera:
                            return new NgParameterRange(NgParameterKind.CamerasInLevel);

                        case TriggerTargetType.Sink:
                            return new NgParameterRange(NgParameterKind.SinksInLevel);

                        case TriggerTargetType.Target:
                            // Actually it is possible to not only target Target objects, but all movables.
                            // This is also useful: It makes sense to target egg a trap or an enemy.
                            return new NgParameterRange(NgParameterKind.MoveablesInLevel);

                        case TriggerTargetType.FlyByCamera:
                            return new NgParameterRange(NgParameterKind.FlybyCamerasInLevel);

                        case TriggerTargetType.FlipEffect:
                            if (levelSettings.GameVersion == GameVersion.TRNG)
                                return new NgParameterRange(NgCatalog.FlipEffectTrigger.MainList.DicSelect(e => (TriggerParameterUshort)e.Value));
                            else
                                return new NgParameterRange(NgCatalog.FlipEffectTrigger.MainList
                                    .DicWhere(entry => entry.Value.Name.StartsWith("OldFlip"))
                                    .DicSelect(e => (TriggerParameterUshort)e.Value));

                        case TriggerTargetType.ActionNg:
                            if (!(timer is TriggerParameterUshort))
                                return new NgParameterRange(NgParameterKind.Empty);
                            NgTriggerSubtype actionSubtriggerType = NgCatalog.ActionTrigger.MainList.TryGetOrDefault(((TriggerParameterUshort)timer).Key);
                            return actionSubtriggerType?.Target ?? new NgParameterRange(NgParameterKind.Empty);

                        case TriggerTargetType.TimerfieldNg:
                            return NgCatalog.TimerFieldTrigger;

                        default:
                            return new NgParameterRange(NgParameterKind.AnyNumber);
                    }
            }
        }

        public static NgParameterRange GetTimerRange(LevelSettings levelSettings, TriggerType triggerType, TriggerTargetType targetType, ITriggerParameter target)
        {
            switch (triggerType)
            {
                case TriggerType.ConditionNg:
                    return new NgParameterRange(NgCatalog.ConditionTrigger.MainList.DicSelect(e => (TriggerParameterUshort)e.Value));

                default:
                    switch (targetType)
                    {
                        case TriggerTargetType.FlipEffect:
                            if (!(target is TriggerParameterUshort))
                                return new NgParameterRange(NgParameterKind.Empty);
                            NgTriggerSubtype flipEffectSubtriggerType = NgCatalog.FlipEffectTrigger.MainList.TryGetOrDefault(((TriggerParameterUshort)target).Key);
                            return flipEffectSubtriggerType?.Timer ?? new NgParameterRange(NgParameterKind.Empty);

                        case TriggerTargetType.ActionNg:
                            return new NgParameterRange(NgCatalog.ActionTrigger.MainList.DicSelect(e => (TriggerParameterUshort)e.Value));

                        case TriggerTargetType.TimerfieldNg:
                            return new NgParameterRange(NgParameterKind.Empty);

                        default:
                            return new NgParameterRange(NgParameterKind.AnyNumber);
                    }
            }
        }

        public static NgParameterRange GetExtraRange(LevelSettings levelSettings, TriggerType triggerType, TriggerTargetType targetType, ITriggerParameter target, ITriggerParameter timer)
        {
            switch (triggerType)
            {
                case TriggerType.ConditionNg:
                    if (!(timer is TriggerParameterUshort))
                        return new NgParameterRange(NgParameterKind.Empty);
                    NgTriggerSubtype conditionSubtriggerType = NgCatalog.ConditionTrigger.MainList.TryGetOrDefault(((TriggerParameterUshort)timer).Key);
                    return conditionSubtriggerType?.Extra ?? new NgParameterRange(NgParameterKind.Empty);

                default:
                    switch (targetType)
                    {
                        case TriggerTargetType.FlipEffect:
                            if (!(target is TriggerParameterUshort))
                                return new NgParameterRange(NgParameterKind.Empty);
                            NgTriggerSubtype flipEffectSubtriggerType = NgCatalog.FlipEffectTrigger.MainList.TryGetOrDefault(((TriggerParameterUshort)target).Key);
                            return flipEffectSubtriggerType?.Extra ?? new NgParameterRange(NgParameterKind.Empty);

                        case TriggerTargetType.ActionNg:
                            if (!(timer is TriggerParameterUshort))
                                return new NgParameterRange(NgParameterKind.Empty);
                            NgTriggerSubtype actionSubtriggerType = NgCatalog.ActionTrigger.MainList.TryGetOrDefault(((TriggerParameterUshort)timer).Key);
                            return actionSubtriggerType?.Extra ?? new NgParameterRange(NgParameterKind.Empty);

                        default:
                            return new NgParameterRange(NgParameterKind.Empty);
                    }
            }
        }

        public static bool TriggerIsValid(LevelSettings levelSettings, TriggerInstance trigger)
        {
            if (!GetTriggerTypeRange(levelSettings).Contains(trigger.TriggerType))
                return false;
            if (!GetTargetTypeRange(levelSettings, trigger.TriggerType).Contains(trigger.TargetType))
                return false;
            if (!GetTargetRange(levelSettings, trigger.TriggerType, trigger.TargetType, trigger.Timer).ParameterMatches(trigger.Target, false))
                return false;
            if (!GetTimerRange(levelSettings, trigger.TriggerType, trigger.TargetType, trigger.Target).ParameterMatches(trigger.Timer, false))
                return false;
            if (!GetExtraRange(levelSettings, trigger.TriggerType, trigger.TargetType, trigger.Target, trigger.Timer).ParameterMatches(trigger.Extra, false))
                return false;
            return true;
        }

        public delegate ushort BoundedValueCallback(ushort upperBound);

        public static ushort EncodeNGRealTimer(TriggerTargetType targetType, TriggerType triggerType, ushort target, ushort upperBound, BoundedValueCallback timer, BoundedValueCallback extra)
        {
            ushort timerUpperBound = (ushort)(upperBound & 255);
            ushort extraUpperBound = (ushort)(upperBound >> 8);
            switch (triggerType)
            {
                case TriggerType.ConditionNg:
                    return (ushort)(timer(timerUpperBound) | (extra(extraUpperBound) << 8));

                default:
                    switch (targetType)
                    {
                        case TriggerTargetType.ActionNg:
                            return (ushort)(timer(timerUpperBound) | (extra(extraUpperBound) << 8));

                        case TriggerTargetType.TimerfieldNg:
                            return timer(upperBound);

                        case TriggerTargetType.FlipEffect:
                            NgTriggerSubtype flipEffectSubtriggerType = NgCatalog.FlipEffectTrigger.MainList.TryGetOrDefault(target);
                            if (flipEffectSubtriggerType != null && flipEffectSubtriggerType.Extra.IsEmpty)
                                return timer(upperBound);
                            else
                                return (ushort)(timer(timerUpperBound) | (extra(extraUpperBound) << 8));

                        default:
                            return timer(upperBound);
                    }
            }
        }

        public static void DecodeNGRealTimer(TriggerTargetType targetType, TriggerType triggerType, ushort target, ushort realTimer, out ushort? timer, out ushort? extra)
        {
            switch (triggerType)
            {
                case TriggerType.ConditionNg:
                    timer = (ushort)(realTimer & 255);
                    var conditionTrigger = NgCatalog.ConditionTrigger.MainList.TryGetOrDefault(timer.Value);
                    if (conditionTrigger != null && conditionTrigger.Extra.IsEmpty)
                        extra = null;
                    else
                        extra = (ushort)(realTimer >> 8);
                    return;

                default:
                    switch (targetType)
                    {
                        case TriggerTargetType.ActionNg:
                            timer = (ushort)(realTimer & 255);
                            var actionTrigger = NgCatalog.ActionTrigger.MainList.TryGetOrDefault(timer.Value);
                            if (actionTrigger != null && actionTrigger.Extra.IsEmpty)
                                extra = null;
                            else
                                extra = (ushort)(realTimer >> 8);
                            return;

                        case TriggerTargetType.TimerfieldNg:
                            timer = realTimer;
                            extra = null;
                            return;

                        case TriggerTargetType.FlipEffect:
                            var flipEffectTrigger = NgCatalog.FlipEffectTrigger.MainList.TryGetOrDefault(target);
                            if (flipEffectTrigger != null && flipEffectTrigger.Extra.IsEmpty)
                            {
                                timer = realTimer;
                                extra = null;
                            }
                            else
                            {
                                timer = (ushort)(realTimer & 255);
                                extra = (ushort)(realTimer >> 8);
                            }
                            return;

                        default:
                            timer = realTimer;
                            extra = null;
                            return;
                    }
            }
        }

        public class ExceptionScriptIdMissing : Exception
        {
            public ExceptionScriptIdMissing()
                : base("ScriptID is missing")
            {}
        }
        public class ExceptionScriptNotSupported : NotSupportedException
        {
            public ExceptionScriptNotSupported()
                : base("Script not supported")
            { }
        }
        private static ushort GetValue(Level level, ITriggerParameter parameter)
        {
            if (parameter == null)
                return 0;
            else if (parameter is TriggerParameterUshort)
                return ((TriggerParameterUshort)parameter).Key;
            else if (parameter is IHasScriptID)
            {
                uint? Id = ((IHasScriptID)parameter).ScriptId;
                if (Id == null)
                    throw new ExceptionScriptIdMissing();
                return checked((ushort)Id);
            }
            else if (parameter is Room)
                return (ushort)level.Rooms.ReferenceIndexOf(parameter);
            else
                throw new Exception("Trigger parameter of invalid type!");
        }
        public static string ExportToScriptTrigger(Level level, TriggerInstance trigger, bool withComment = false)
        {
            checked
            {
                string result = null;

                switch (trigger.TriggerType)
                {
                    case TriggerType.ConditionNg:
                        {
                            if (!TriggerIsValid(level.Settings, trigger))
                                throw new Exception("Trigger is invalid.");

                            ushort conditionId = GetValue(level, trigger.Timer);
                            NgTriggerSubtype conditionTrigger = NgCatalog.ConditionTrigger.MainList[conditionId];

                            ushort firstValue = GetValue(level, trigger.Target);
                            ushort secondValue = conditionId;
                            if (!conditionTrigger.Extra.IsEmpty)
                                secondValue |= (ushort)(GetValue(level, trigger.Extra) << 8);

                            result = trigger.Target is ObjectInstance ? "$9000," : "$8000,";
                            result += firstValue + ",$" + secondValue.ToString("X4");
                            break;
                        }
                    default:
                        switch (trigger.TargetType)
                        {
                            case TriggerTargetType.FlipEffect:
                                {
                                    if (!TriggerIsValid(level.Settings, trigger))
                                        throw new Exception("Trigger is invalid.");

                                    ushort flipeffectId = GetValue(level, trigger.Target);
                                    NgTriggerSubtype flipeffectTrigger = NgCatalog.FlipEffectTrigger.MainList[flipeffectId];

                                    ushort firstValue = flipeffectId;
                                    ushort secondValue = GetValue(level, trigger.Timer);
                                    if (!flipeffectTrigger.Extra.IsEmpty)
                                        secondValue |= (ushort)(GetValue(level, trigger.Extra) << 8);

                                    result = "$2000," + firstValue + ",$" + secondValue.ToString("X4");
                                    break;
                                }

                            case TriggerTargetType.ActionNg:
                                {
                                    if (!TriggerIsValid(level.Settings, trigger))
                                        throw new Exception("Trigger is invalid.");

                                    ushort actionId = GetValue(level, trigger.Timer);
                                    NgTriggerSubtype actionTrigger = NgCatalog.ActionTrigger.MainList[actionId];

                                    ushort firstValue = GetValue(level, trigger.Target);
                                    ushort secondValue = actionId;
                                    if (!actionTrigger.Extra.IsEmpty)
                                        secondValue |= (ushort)(GetValue(level, trigger.Extra) << 8);

                                    result = "$5000," + firstValue + ",$" + secondValue.ToString("X4");
                                    break;
                                }
                        }
                        break;
                }

                if(!string.IsNullOrEmpty(result))
                    return (withComment ? 
                            "; "       + trigger.TriggerType + " for " + trigger.TargetType +
                            "\n; <#> " + trigger.Target +
                            "\n; <&> " + trigger.Timer  +
                            (trigger.Extra == null ? "" : "\n; <E> " + trigger.Extra) +
                            "\n; Copy next line to your script: \n\n"
                        : "")
                        + result;
            }
            throw new ExceptionScriptNotSupported();
        }

    }
}