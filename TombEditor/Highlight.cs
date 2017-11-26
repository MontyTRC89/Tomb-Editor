using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SharpDX;
using TombEditor.Geometry;

namespace TombEditor
{
    public enum HighlightType
    {
        None,
        Trigger,
        NotWalkableFloor,
        Box,
        Monkey,
        Death,
        Portal,
        Beetle,
        TriggerTriggerer
    }

    public class HighlightState
    {
        public static readonly Vector4 ColorPortal = new Vector4(0, 0, 0, 255) / 255.0f;
        public static readonly Vector4 ColorPortalFace = new Vector4(255, 255, 0, 255) / 255.0f;
        public static readonly Vector4 ColorFloor = new Vector4(0, 190, 190, 255) / 255.0f;
        public static readonly Vector4 ColorWall = new Vector4(0, 160, 0, 255) / 255.0f;
        public static readonly Vector4 ColorWallUpper = new Vector4(0, 80, 0, 255) / 255.0f;
        public static readonly Vector4 ColorWallMiddle = new Vector4(0, 240, 0, 255) / 255.0f;
        public static readonly Vector4 ColorTrigger = new Vector4(200, 0, 200, 255) / 255.0f;
        public static readonly Vector4 ColorMonkey = new Vector4(255, 100, 100, 255) / 255.0f;
        public static readonly Vector4 ColorClimb = new Vector4(255, 180, 180, 255) / 255.0f;
        public static readonly Vector4 ColorBox = new Vector4(100, 100, 100, 255) / 255.0f;
        public static readonly Vector4 ColorDeath = new Vector4(20, 240, 20, 255) / 255.0f;
        public static readonly Vector4 ColorNotWalkable = new Vector4(0, 0, 150, 255) / 255.0f;
        public static readonly Vector4 ColorBeetle = new Vector4(100, 100, 100, 255) / 255.0f;
        public static readonly Vector4 ColorTriggerTriggerer = new Vector4(0, 0, 252, 255) / 255.0f;

        private List<HighlightType> _priorityList = new List<HighlightType>
        {
            HighlightType.Trigger,
            HighlightType.NotWalkableFloor,
            HighlightType.Box,
            HighlightType.Monkey,
            HighlightType.Death,
            HighlightType.Portal,
            HighlightType.Beetle,
            HighlightType.TriggerTriggerer
        };

        public HighlightState()
        {}

        public HighlightState(HighlightType priorityType)
        {
            _priorityList = _priorityList.OrderByDescending((item) => item == priorityType).ToList();
        }

        public Vector4? GetHighlightColor(Room room, int x, int z, bool probeThroughPortals, bool getFrameColor = false)
        {
            Block block = room.GetBlockTry(x, z);
            if (block == null)
                return null;

            Block bottomBlock = room.ProbeLowestBlock(x, z, probeThroughPortals).Block;
            foreach (var highlight in _priorityList)
            {
                if (getFrameColor)
                {
                    if (highlight == HighlightType.Beetle && bottomBlock.Flags.HasFlag(BlockFlags.Beetle))
                        return ColorBeetle;
                    else if (highlight == HighlightType.TriggerTriggerer && bottomBlock.Flags.HasFlag(BlockFlags.TriggerTriggerer))
                        return ColorTriggerTriggerer;
                }
                else
                {
                    switch (highlight)
                    {
                        case HighlightType.Trigger:
                            if (bottomBlock.Triggers.Count != 0)
                                return ColorTrigger;
                            break;
                        case HighlightType.NotWalkableFloor:
                            if (bottomBlock.Flags.HasFlag(BlockFlags.NotWalkableFloor))
                                return ColorNotWalkable;
                            break;
                        case HighlightType.Box:
                            if (bottomBlock.Flags.HasFlag(BlockFlags.Box))
                                return ColorBox;
                            break;
                        case HighlightType.Monkey:
                            if (bottomBlock.Flags.HasFlag(BlockFlags.Monkey))
                                return ColorMonkey;
                            break;
                        case HighlightType.Death:
                            if (bottomBlock.Flags.HasFlag(BlockFlags.DeathFire) ||
                                bottomBlock.Flags.HasFlag(BlockFlags.DeathElectricity) ||
                                bottomBlock.Flags.HasFlag(BlockFlags.DeathLava))
                                return ColorDeath;
                            break;
                        case HighlightType.Portal:
                            if (block.FloorPortal != null || block.CeilingPortal != null || block.WallPortal != null)
                                return ColorPortal;
                            break;
                    }
                }
            }
            return null;
        }
    }

    public class HighlightManager : IDisposable
    {
        private Editor _editor;

        public class ChangeHighlightEvent : IEditorEvent { }
        private HighlightState _currentState;
        private HighlightState _previousState;

        private float _transitionValue = 0.0f;
        private float _transitionSpeed = 0.1f;
        private Timer _transitionAnimator = new Timer() { Interval = 1 };

        public HighlightManager(Editor editor)
        {
            _editor = editor;
            _currentState = new HighlightState();
            _previousState = new HighlightState();

            _transitionAnimator.Elapsed += UpdateTransitionAnimation;
        }

        public void Dispose()
        {
            if (_transitionAnimator.Enabled)
                _transitionAnimator.Stop();
            _transitionAnimator.Elapsed -= UpdateTransitionAnimation;
            _transitionAnimator.Dispose();
        }

        public void SetPriority(HighlightType type)
        {
            _previousState = _currentState;
            _currentState = new HighlightState(type);

            _transitionValue = 0.0f;
            _transitionAnimator.Start();
        }

        public Vector4? GetColor(Room room, int x, int z, bool probeThroughPortals, bool getFrameColor = false)
        {
            var prevColor = _previousState.GetHighlightColor(room, x, z, probeThroughPortals, getFrameColor);
            var currColor = _currentState.GetHighlightColor(room, x, z, probeThroughPortals, getFrameColor);

            if (!prevColor.HasValue || !currColor.HasValue)
                return null;

            return Vector4.Lerp(prevColor.Value, currColor.Value, _transitionValue);
        }

        private void UpdateTransitionAnimation(object sender, ElapsedEventArgs e)
        {
            if (_transitionValue < 1.0f)
                _transitionValue += _transitionSpeed;

            if (_transitionValue >= 1.0f)
            {
                _transitionValue = 1.0f;
                _transitionAnimator.Enabled = false;
            }

            _editor.RaiseEvent(new ChangeHighlightEvent());
        }
    }
}
