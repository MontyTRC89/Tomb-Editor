using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadMoveableId : IWadObjectId, IEquatable<WadMoveableId>, IComparable<WadMoveableId>
    {
        public uint TypeId;

        public WadMoveableId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadMoveableId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadMoveableId)other);
        public static bool operator <(WadMoveableId first, WadMoveableId second) => first.TypeId < second.TypeId;
        public static bool operator <=(WadMoveableId first, WadMoveableId second) => first.TypeId <= second.TypeId;
        public static bool operator >(WadMoveableId first, WadMoveableId second) => first.TypeId > second.TypeId;
        public static bool operator >=(WadMoveableId first, WadMoveableId second) => first.TypeId >= second.TypeId;
        public static bool operator ==(WadMoveableId first, WadMoveableId second) => first.TypeId == second.TypeId;
        public static bool operator !=(WadMoveableId first, WadMoveableId second) => !(first == second);
        public bool Equals(WadMoveableId other) => this == other;
        public override bool Equals(object other) => other is WadMoveableId && this == (WadMoveableId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(WadGameVersion gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetMoveableName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(WadGameVersion.TR4_TRNG);

        public static WadMoveableId Lara = new WadMoveableId(0);
        public static WadMoveableId LaraSkin = new WadMoveableId(8);
        public static WadMoveableId SkyBox = new WadMoveableId(459);

        public bool IsWaterfall(WadGameVersion gameVersion)
        {
            return gameVersion == WadGameVersion.TR4_TRNG && TypeId >= 423 && TypeId <= 425 ||
                   gameVersion == WadGameVersion.TR5 && TypeId >= 410 && TypeId <= 415;
        }

        public bool IsAI(WadGameVersion gameVersion)
        {
            return TypeId >= 398 && TypeId <= 406;
        }
    }

    public class WadMoveable : IWadObject
    {
        public WadMoveableId Id { get; private set; }
        public List<WadMesh> Meshes => Skeleton.LinearizedBones.Select(bone => bone.Mesh).ToList();
        public List<WadAnimation> Animations { get; } = new List<WadAnimation>();
        public WadBone Skeleton { get; set; } = new WadBone();

        public WadMoveable(WadMoveableId id)
        {
            Id = id;
        }

        public string ToString(WadGameVersion gameVersion) => Id.ToString(gameVersion);
        public override string ToString() => "Uncertain game version - " + ToString(WadGameVersion.TR4_TRNG);
        IWadObjectId IWadObject.Id => Id;

        public IEnumerable<WadSoundInfo> Sounds
        {
            get
            {
                foreach (var animation in Animations)
                    foreach (var command in animation.AnimCommands)
                        if (command.Type == WadAnimCommandType.PlaySound)
                            yield return command.SoundInfo;
            }
        }
    }
}
