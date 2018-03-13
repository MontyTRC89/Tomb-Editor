using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor
{
    public enum HighlightType
    {
        Wall,
        Trigger,
        NotWalkableFloor,
        Box,
        Monkey,
        Death,
        Climb,
        Portal,
        BorderWall,
        Floor,
        Ceiling,
        ForceFloorSolid,
        Beetle,
        TriggerTriggerer
    }

    public enum HighlightShape
    {
        Rectangle,
        TriangleXpZp,
        TriangleXpZn,
        TriangleXnZp,
        TriangleXnZn,
        Hatch,
        Frame,
        EdgeXn,
        EdgeXp,
        EdgeZp,
        EdgeZn,
    }

    public struct HighlightColor
    {
        public HighlightShape Shape;
        public Vector4 Color;
    }

    public class HighlightState
    {
        public static readonly Vector4 ColorPortal = new Vector4(0, 0, 0, 255) / 255.0f;
        public static readonly Vector4 ColorPortalFace = new Vector4(255, 255, 0, 255) / 255.0f;
        public static readonly Vector4 ColorFloor = new Vector4(0, 190, 190, 255) / 255.0f;
        public static readonly Vector4 ColorBorderWall = new Vector4(128, 128, 128, 255) / 255.0f;
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
        public static readonly Vector4 ColorForceSolidFloor = Vector4.Lerp(ColorFloor, new Vector4(0.0f, 0.0f, 0.0f, 1.0f), 0.1f);

        private List<HighlightType> _priorityList = Enum.GetValues(typeof(HighlightType)).Cast<HighlightType>().ToList();
        public HighlightType Priority => _priorityList[0];

        public HighlightState()
        {}

        public HighlightState(HighlightType priorityType)
        {
            _priorityList = _priorityList.OrderByDescending(item => item == priorityType).ToList();
        }

        public Vector4? GetHighlightColor(Room room, int x, int z, bool probeThroughPortals, HighlightShape shape, HashSet<HighlightType> typesToIgnore = null)
        {
            Block block = room.GetBlockTry(x, z);
            if (block == null)
                return null;

            bool checkIgnored = (typesToIgnore != null);

            Block bottomBlock = room.ProbeLowestBlock(x, z, probeThroughPortals).Block ?? Block.Empty;
            for(int i = 0; i < _priorityList.Count; i++)
            {
                if (checkIgnored && typesToIgnore.Contains(_priorityList[i]))
                    continue;

                switch(shape)
                {
                    case HighlightShape.Rectangle:
                        switch (_priorityList[i])
                        {
                            case HighlightType.Trigger:
                                if (bottomBlock.Triggers.Count != 0)
                                    return ColorTrigger;
                                break;
                            case HighlightType.NotWalkableFloor:
                                if (bottomBlock.HasFlag(BlockFlags.NotWalkableFloor))
                                    return ColorNotWalkable;
                                break;
                            case HighlightType.Box:
                                if (bottomBlock.HasFlag(BlockFlags.Box))
                                    return ColorBox;
                                break;
                            case HighlightType.Monkey:
                                if (bottomBlock.HasFlag(BlockFlags.Monkey))
                                    return ColorMonkey;
                                break;
                            case HighlightType.Death:
                                if (bottomBlock.HasFlag(BlockFlags.DeathFire) ||
                                    bottomBlock.HasFlag(BlockFlags.DeathElectricity) ||
                                    bottomBlock.HasFlag(BlockFlags.DeathLava))
                                    return ColorDeath;
                                break;
                            case HighlightType.BorderWall:
                                if (block.Type == BlockType.BorderWall)
                                    return ColorBorderWall;
                                break;
                            case HighlightType.Wall:
                                if (block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None)
                                    return ColorWall;
                                break;
                            case HighlightType.Floor:
                                if (!(block.FloorDiagonalSplit == DiagonalSplit.None && block.IsAnyWall) && block.FloorPortal == null)
                                    return ColorFloor;
                                break;
                            case HighlightType.Ceiling:
                                if (!(block.CeilingDiagonalSplit == DiagonalSplit.None && block.IsAnyWall) && block.CeilingPortal == null)
                                    return ColorFloor;
                                break;
                            case HighlightType.Portal:
                                if (block.IsAnyPortal)
                                    return ColorPortal;
                                break;
                        }
                        break;
                    case HighlightShape.Frame:
                        switch (_priorityList[i])
                        {
                            case HighlightType.Climb:
                                if (bottomBlock.HasFlag(BlockFlags.ClimbAny))
                                    return ColorClimb;
                                break;
                            case HighlightType.Beetle:
                                if (bottomBlock.HasFlag(BlockFlags.Beetle))
                                    return ColorBeetle;
                                break;
                            case HighlightType.TriggerTriggerer:
                                if (bottomBlock.HasFlag(BlockFlags.TriggerTriggerer))
                                    return ColorTriggerTriggerer;
                                break;
                        }
                        break;
                    case HighlightShape.Hatch:
                        if (block.ForceFloorSolid)
                            return ColorForceSolidFloor;
                        break;
                    case HighlightShape.TriangleXnZn:
                        if (block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XnZn)
                            return ColorWall;
                        break;
                    case HighlightShape.TriangleXnZp:
                        if (block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            return ColorWall;
                        break;
                    case HighlightShape.TriangleXpZn:
                        if (block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            return ColorWall;
                        break;
                    case HighlightShape.TriangleXpZp:
                        if (block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            return ColorWall;
                        break;
                    case HighlightShape.EdgeXn:
                    case HighlightShape.EdgeXp:
                    case HighlightShape.EdgeZp:
                    case HighlightShape.EdgeZn:
                        if (!bottomBlock.HasFlag(BlockFlags.ClimbAny))
                            if ((shape == HighlightShape.EdgeXn && bottomBlock.HasFlag(BlockFlags.ClimbNegativeX)) ||
                                (shape == HighlightShape.EdgeXp && bottomBlock.HasFlag(BlockFlags.ClimbPositiveX)) ||
                                (shape == HighlightShape.EdgeZp && bottomBlock.HasFlag(BlockFlags.ClimbPositiveZ)) ||
                                (shape == HighlightShape.EdgeZn && bottomBlock.HasFlag(BlockFlags.ClimbNegativeZ)))
                            return ColorClimb;
                        break;
                    default:
                        break;
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

        private float _transitionValue;
        private float _transitionSpeed = 0.4f;
        private Timer _transitionAnimator = new Timer() { Interval = 60 };

        private static readonly List<HighlightShape> _allShapes = Enum.GetValues(typeof(HighlightShape)).Cast<HighlightShape>().ToList();

        public HighlightManager(Editor editor)
        {
            _editor = editor;
            _currentState = new HighlightState();
            _previousState = new HighlightState();

            _transitionAnimator.Tick += UpdateTransitionAnimation;
        }

        public void Dispose()
        {
            if (_transitionAnimator.Enabled)
                _transitionAnimator.Stop();
            _transitionAnimator.Tick -= UpdateTransitionAnimation;
            _transitionAnimator.Dispose();
        }

        public void SetPriority(HighlightType type)
        {
            if (_currentState.Priority == type)
                return;

            _previousState = _currentState;
            _currentState = new HighlightState(type);

            _transitionValue = 0.0f;
            _transitionAnimator.Start();
        }

        public List<HighlightColor> GetColors(Room room, int x, int z, bool probeThroughPortals, HashSet<HighlightType> typesToIgnore = null, List<HighlightShape> shapesToList = null)
        {
            List<HighlightColor> colors = null;

            if(shapesToList == null)
                shapesToList = _allShapes;

            for (int i = 0; i < shapesToList.Count; i++)
            {
                Vector4? prevColor = _previousState.GetHighlightColor(room, x, z, probeThroughPortals, shapesToList[i], typesToIgnore);
                Vector4? currColor = _currentState.GetHighlightColor(room, x, z, probeThroughPortals, shapesToList[i], typesToIgnore);
                Vector4? finalColor = null;

                if (!prevColor.HasValue || !currColor.HasValue)
                    continue;
                else if (prevColor.Value == currColor.Value)
                    finalColor = currColor;
                else
                    finalColor = Vector4.Lerp(prevColor.Value, currColor.Value, _transitionValue);

                if (finalColor.HasValue)
                {
                    if (colors == null)
                        colors = new List<HighlightColor>();
                    colors.Add(new HighlightColor() { Color = finalColor.Value, Shape = shapesToList[i] });
                }
            }

            return colors;
        }

        private void UpdateTransitionAnimation(object sender, EventArgs e)
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
