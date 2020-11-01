using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor {
    public class StatisticSummary {
        public Statistics LevelStats = new Statistics();
        public Statistics RoomStats = new Statistics();
        public int? BoxCount;
        public int? OverlapCount;
        public int? TextureCount;
    }
    public class Statistics {
        public int MoveableCount;
        public int StaticCount;
        public int TriggerCount;
        public int DynLightCount;
        public int LightCount;
        public int CameraCount;
        public int FlybyCount;
    }
}
