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
        Trigger, NotWalkableFloor, Box, Monkey, Death, Portal,
        Beetle, TriggerTriggerer
    }

    public class HighlightState
    {
        private List<KeyValuePair<int, HighlightType>> _priorityList = new List<KeyValuePair<int, HighlightType>>
        {
            new KeyValuePair<int, HighlightType> (0, HighlightType.Trigger),
            new KeyValuePair<int, HighlightType> (1, HighlightType.NotWalkableFloor),
            new KeyValuePair<int, HighlightType> (2, HighlightType.Box),
            new KeyValuePair<int, HighlightType> (3, HighlightType.Monkey),
            new KeyValuePair<int, HighlightType> (4, HighlightType.Death),
            new KeyValuePair<int, HighlightType> (5, HighlightType.Portal),
            new KeyValuePair<int, HighlightType> (6, HighlightType.Beetle),
            new KeyValuePair<int, HighlightType> (7, HighlightType.TriggerTriggerer)
        };

        public HighlightState(HighlightType priorityType)
        {
            _priorityList = _priorityList.OrderByDescending((item) => item.Value == priorityType)
                                         .ThenBy((item) => item.Key)
                                         .ToList();
        }

        public Vector4? GetHighlightColor(Room room, int x, int z, bool probeThroughPortals, bool getFrameColor = false)
        {
            Vector4? color = null;
            Block block = room.GetBlockTry(x, z);
            if (block != null)
            {
                Block bottomBlock = room.ProbeLowestBlock(x, z, probeThroughPortals).Block;

                foreach (var highlight in _priorityList)
                {
                    if (getFrameColor)
                    {
                        if (highlight.Value == HighlightType.Beetle && bottomBlock.Flags.HasFlag(BlockFlags.Beetle))
                            color = Editor.ColorBeetle;
                        else if (highlight.Value == HighlightType.TriggerTriggerer && bottomBlock.Flags.HasFlag(BlockFlags.TriggerTriggerer))
                            color = Editor.ColorTriggerTriggerer;
                    }
                    else
                    {
                        switch (highlight.Value)
                        {
                            case HighlightType.Trigger:
                                if (bottomBlock.Triggers.Count != 0)
                                    color = Editor.ColorTrigger;
                                break;
                            case HighlightType.NotWalkableFloor:
                                if (bottomBlock.Flags.HasFlag(BlockFlags.NotWalkableFloor))
                                    color = Editor.ColorNotWalkable;
                                break;
                            case HighlightType.Box:
                                if (bottomBlock.Flags.HasFlag(BlockFlags.Box))
                                    color = Editor.ColorBox;
                                break;
                            case HighlightType.Monkey:
                                if (bottomBlock.Flags.HasFlag(BlockFlags.Monkey))
                                    color = Editor.ColorMonkey;
                                break;
                            case HighlightType.Death:
                                if (bottomBlock.Flags.HasFlag(BlockFlags.DeathFire) ||
                                    bottomBlock.Flags.HasFlag(BlockFlags.DeathElectricity) ||
                                    bottomBlock.Flags.HasFlag(BlockFlags.DeathLava))
                                    color = Editor.ColorDeath;
                                break;
                            case HighlightType.Portal:
                                if (block.FloorPortal != null || block.CeilingPortal != null || block.WallPortal != null)
                                    color = Editor.ColorPortal;
                                break;
                        }
                    }

                    if (color.HasValue)
                        break;
                }
            }

            return color;
        }
    }

    public class HighlightManager : IDisposable
    {
        private Editor _editor;

        private HighlightState _currentState;
        private HighlightState _previousState;

        private float _transitionValue = 0.0f;
        private float _transitionSpeed = 0.1f;
        private Timer _transitionAnimator = new Timer() { Interval = 1 };

        public HighlightManager(Editor editor)
        {
            _editor = editor;
            _currentState = new HighlightState(HighlightType.None);
            _previousState = new HighlightState(HighlightType.None);

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

            _editor.RaiseEvent(new Editor.ChangeHighlightEvent());
        }
    }
}
