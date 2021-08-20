using System.Collections.Generic;
using System.Numerics;
using TombLib.LevelData;
using Xunit;

namespace TombLib.Tests.LevelData.Instances
{
    public class ObjectGroupTests
    {
        [Fact]
        public void Constructor_SetsRoomAndPositionFromInitialObject_AddsInitialObject()
        {
            // Arrange
            var level = new Level();
            var room = new Room(level, 12, 12, Vector3.Zero);

            var position = new Vector3(100f, 200f, 300f);
            var obj = new MoveableInstance
            {
                Position = position,
                RotationY = 15f
            };

            room.AddObject(level, obj);

            // Act
            var objectGroup = new ObjectGroup(obj);

            // Assert
            Assert.Same(room, objectGroup.Room);
            Assert.Equal(position, objectGroup.Position);
            Assert.Equal(0f, objectGroup.RotationY);
            Assert.Single(objectGroup);
            Assert.Collection(
                objectGroup, 
                first => Assert.Same(obj, first)
                );
        }

        [Fact]
        public void SetPosition_SetsRelativeChildPositions()
        {
            // Arrange
            var position1 = new Vector3(100f, 200f, 300f);
            var position2 = new Vector3(105f, 205f, 305f);
            var position3 = new Vector3(95f, 195f, 295f);

            var obj1 = new MoveableInstance { Position = position1 };
            var obj2 = new LightInstance(LightType.Spot) { Position = position2 };
            var obj3 = new SinkInstance { Position = position3 };

            var objects = new List<PositionBasedObjectInstance> { obj1, obj2, obj3 };

            var objectGroup = new ObjectGroup(objects);

            // Act
            var targetPosition = new Vector3(50f, 60f, 70f);
            objectGroup.SetPosition(targetPosition);

            // Assert
            var expectedPosition2 = new Vector3(55f, 65f, 75f);
            var expectedPosition3 = new Vector3(45f, 55f, 65f);

            Assert.Equal(targetPosition, objectGroup.Position);
            Assert.Equal(targetPosition, obj1.Position);
            Assert.Equal(expectedPosition2, obj2.Position);
            Assert.Equal(expectedPosition3, obj3.Position);
        }

        [Fact]
        public void SetRotation_AdjustsChildRotationsByDifference_ForIRotatableYChildren()
        {
            // Arrange
            var rotation1 = 70f;
            var rotation2 = 20f;
            var rotation3 = 110f;

            var obj1 = new MoveableInstance { RotationY = rotation1 };
            var obj2 = new LightInstance(LightType.Spot) { RotationY = rotation2 };
            var obj3 = new MoveableInstance { RotationY = rotation3 };
            var obj4 = new SinkInstance();

            var objects = new List<PositionBasedObjectInstance> { obj1, obj2, obj3, obj4 };

            var objectGroup = new ObjectGroup(objects);

            // Act
            var targetRotation = 15f;
            objectGroup.RotationY = targetRotation;

            // Assert
            Assert.Equal(rotation1 + targetRotation, obj1.RotationY);
            Assert.Equal(rotation2 + targetRotation, obj2.RotationY);
            Assert.Equal(rotation3 + targetRotation, obj3.RotationY);
        }

        

        [Fact]
        public void RotateAsGroup_SetsRotation_AdjustsChildRotations_RotatesChildPositions()
        {
            /*
             * Assuming one dot is 1 unit of distance and object 1 is at X: -1, Z: 1
             * X axis is horizontal, axes increase top-left to bottom-right
             *
             * Before:
             * . . . . .
             * . . 2 . .
             * . . 1 . .
             * . . . . 3
             * . . . . .
             */

            // Arrange
            var rotation1 = 70f; var position1 = new Vector3(-1f, 1f, 1f);
            var rotation2 = 20f; var position2 = new Vector3(-1f, 2f, 0f);
            var rotation3 = 90f; var position3 = new Vector3(1f, 3f, 2f);

            var obj1 = new MoveableInstance { RotationY = rotation1, Position = position1 };
            var obj2 = new FlybyCameraInstance { RotationY = rotation2, Position = position2 };
            var obj3 = new LightInstance(LightType.Spot) { RotationY = rotation3, Position = position3 };

            var objects = new List<PositionBasedObjectInstance> { obj1, obj2, obj3 };

            var objectGroup = new ObjectGroup(objects);

            // Act
            var targetRotation = -90f;  // Clockwise
            objectGroup.RotateAsGroup(targetRotation);

            // Assert

            /*
             * After:
             * . . . . .
             * . . . . .
             * . . 1 2 .
             * . . . . .
             * . 3 . . .
             */
            var expectedRotation1 = 340f; var expectedPosition1 = position1;
            var expectedRotation2 = 290f; var expectedPosition2 = new Vector3(0f, 2f, 1f);
            var expectedRotation3 = 0f; var expectedPosition3 = new Vector3(-2f, 3f, 3f);

            Assert.Equal(expectedRotation1, obj1.RotationY);
            Assert.Equal(expectedRotation2, obj2.RotationY);
            Assert.Equal(expectedRotation3, obj3.RotationY);

            Assert.Equal(expectedPosition1, obj1.Position);
            Assert.Equal(expectedPosition2, obj2.Position);
            Assert.Equal(expectedPosition3, obj3.Position);

            Assert.Equal(targetRotation, objectGroup.RotationY);
        }


        [Fact]
        public void RotateAsGroup_DoesNotSetRotationIfChildNotIRotatableY()
        {
            /*
             * Assuming one dot is 1 unit of distance and object 1 is at X: -1, Z: 1
             * X axis is horizontal, axes increase top-left to bottom-right
             *
             * Before:
             * . . . . .
             * . . 2 . .
             * . . 1 . .
             * . . . . 3
             * . . . . .
             */

            // Arrange
            var rotation1 = 70f; var position1 = new Vector3(-1f, 1f, 1f);
            var rotation2 = 20f; var position2 = new Vector3(-1f, 2f, 0f);
            var position3 = new Vector3(1f, 3f, 2f);

            var obj1 = new MoveableInstance { RotationY = rotation1, Position = position1 };
            var obj2 = new MoveableInstance { RotationY = rotation2, Position = position2 };
            var obj3 = new SinkInstance() { Position = position3 };

            var objects = new List<PositionBasedObjectInstance> { obj1, obj2, obj3 };

            var objectGroup = new ObjectGroup(objects);

            // Act
            var targetRotation = -90f;  // Clockwise
            objectGroup.RotateAsGroup(targetRotation);

            // Assert

            /*
             * After:
             * . . . . .
             * . . . . .
             * . . 1 2 .
             * . . . . .
             * . 3 . . .
             */
            var expectedRotation1 = 340f; var expectedPosition1 = position1;
            var expectedRotation2 = 290f; var expectedPosition2 = new Vector3(0f, 2f, 1f);
            var expectedPosition3 = new Vector3(-2f, 3f, 3f);

            Assert.Equal(expectedRotation1, obj1.RotationY);
            Assert.Equal(expectedRotation2, obj2.RotationY);

            Assert.Equal(expectedPosition1, obj1.Position);
            Assert.Equal(expectedPosition2, obj2.Position);
            Assert.Equal(expectedPosition3, obj3.Position);

            Assert.Equal(targetRotation, objectGroup.RotationY);
        }
    }
}
