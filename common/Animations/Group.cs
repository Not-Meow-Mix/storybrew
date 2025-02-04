using OpenTK;
using System.Collections.Generic;

namespace StorybrewCommon.Animations
{
    public class Group : GroupElement
    {
        List<GroupElement> _groupElementList;

        public Group() : base()
        {
            _groupElementList = new List<GroupElement>();
        }

        public void Add(GroupElement groupElement)
        {
            _groupElementList.Add(groupElement);
        }

        public override void Draw(KeyframedValue<Vector2> parentMoveKeyframes, KeyframedValue<double> parentRotateKeyframes, KeyframedValue<double> parentScaleKeyframes)
        {
            var mergedMoveKeyframes = MergeMove(parentMoveKeyframes, parentRotateKeyframes, parentScaleKeyframes);
            var mergedRotateKeyframes = MergeRotate(parentRotateKeyframes);
            var mergedScaleKeyframes = MergeScale(parentScaleKeyframes);
            foreach (var groupElement in _groupElementList)
            {
                groupElement.Draw(mergedMoveKeyframes, mergedRotateKeyframes, mergedScaleKeyframes);
            }
        }
        public override void Draw()
        {
            foreach (var groupElement in _groupElementList)
            {
                groupElement.Draw(_moveKeyframes, _rotateKeyframes, _scaleKeyframes);
            }
        }
    }
}
