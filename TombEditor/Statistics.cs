using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor {
    public class StatisticSummary {
        const string compileStatsFormat = "Boxes: {0}, Overlaps: {1}, Textures: {2}";

        public Statistics LevelStats = new Statistics() {Name = "Level" };
        public Statistics RoomStats = new Statistics() { Name = "Room" };
        public int? BoxCount;
        public int? OverlapCount;
        public int? TextureCount;

        public int TextureLimit;
        public int OverlapLimit;
        public int BoxLimit;

        private string GetTextureInfoString(int limit) {
            if (TextureCount.HasValue)
                return TextureCount >= limit ? TextureCount.ToString() + "!!" : TextureCount.ToString();
            return "?";
        }

        private string GetOverlapCountString(int limit) {
            if (OverlapCount.HasValue)
                return OverlapCount >= limit ? OverlapCount.ToString() + "!!" : OverlapCount.ToString();
            return "?";
        }

        private string GetBoxCountString(int limit) {
            if (BoxCount.HasValue)
                return BoxCount >= limit ? BoxCount.ToString() + "!!" : BoxCount.ToString();
            return "?";
        }

        public override string ToString() {
            return string.Format(compileStatsFormat, GetBoxCountString(BoxLimit), GetOverlapCountString(OverlapLimit), GetTextureInfoString(TextureLimit));

        }
    }
    public class Statistics {
        public string GetDynLightCount(int lightLimit) {
            return DynLightCount >= lightLimit ? DynLightCount.ToString() + "!!" : DynLightCount.ToString();
        }

        public string GetItemCountString(int limit) {
            return MoveableCount + StaticCount >= limit ? (MoveableCount + StaticCount).ToString() + "!!" : (MoveableCount + StaticCount).ToString();
        }


        const string statsFormat = "{0} Statistic : Items: {1} Moveables: {2}, Statics: {3}, Triggers: {4}, DynLights: {5}, All Lights: {6}, Cameras: {7}, Flybys:{8}";
        public int MoveableCount;
        public int StaticCount;
        public int TriggerCount;
        public int DynLightCount;
        public int LightCount;
        public int CameraCount;
        public int FlybyCount;

        public int ItemLimit;
        public int LightLimit;
        public string Name;
        public override string ToString() {
            return string.Format(statsFormat, Name, GetItemCountString(ItemLimit), MoveableCount, StaticCount, TriggerCount, GetDynLightCount(LightLimit), LightCount, CameraCount, FlybyCount);
        }
    }
}
