using OpenTK;
using System;
using StorybrewCommon.Storyboarding;

namespace StorybrewCommon.Animations
{
    public class GroupSprite : GroupElement
    {
        public OsbSprite Sprite { get; }
        Vector2 _scaleBase;
        double _startTimeBase, _endTimeBase;

        public GroupSprite(OsbSprite sprite, double startTime, double endTime, double scaleBase = DEFAULT_SCALE) : this(sprite, startTime, endTime, (float)scaleBase * Vector2.One) { }
        public GroupSprite(OsbSprite sprite, double startTime, double endTime, Vector2 scaleBase) : base()
        {
            Sprite = sprite;
            _scaleBase = scaleBase;
            _startTimeBase = startTime;
            _endTimeBase = endTime;
        }

        private class KeyframePair<T>
        {
            public double StartTime, EndTime;
            public T StartValue, EndValue;
            public KeyframePair(double StartTime, double EndTime, T StartValue, T EndValue)
            {
                this.StartTime = StartTime;
                this.EndTime = EndTime;
                this.StartValue = StartValue;
                this.EndValue = EndValue;
            }
        };

        private KeyframePair<T> DrawData<T>(Keyframe<T> start, Keyframe<T> end, Func<T, T, double, T> f)
        {
            if (start.Time <= _endTimeBase && end.Time >= _startTimeBase)
            {
                var startTime = (start.Time >= _startTimeBase) ? start.Time : _startTimeBase;
                var endTime = (end.Time <= _endTimeBase) ? end.Time : _endTimeBase;
                var startValue = (start.Time >= _startTimeBase) ? start.Value :
                  f(start.Value, end.Value, (_startTimeBase - start.Time) / (end.Time - start.Time));
                var endValue = (end.Time <= _endTimeBase) ? end.Value :
                  f(start.Value, end.Value, (_endTimeBase - start.Time) / (end.Time - start.Time));
                return new KeyframePair<T>(startTime, endTime, startValue, endValue);
            }
            return null;
        }

        public override void Draw(KeyframedValue<Vector2> parentMoveKeyframes, KeyframedValue<double> parentRotateKeyframes, KeyframedValue<double> parentScaleKeyframes)
        {
            MergeMove(parentMoveKeyframes, parentRotateKeyframes, parentScaleKeyframes).ForEachPair(
              (start, end) => {
                  var keyframePair = DrawData<Vector2>(start, end, InterpolatingFunctions.Vector2);
                  if (keyframePair != null)
                  {
                      Sprite.Move(keyframePair.StartTime, keyframePair.EndTime, keyframePair.StartValue, keyframePair.EndValue);
                  }
              });
            MergeRotate(parentRotateKeyframes).ForEachPair(
              (start, end) => {
                  var keyframePair = DrawData<double>(start, end, InterpolatingFunctions.Double);
                  if (keyframePair != null)
                  {
                      Sprite.Rotate(keyframePair.StartTime, keyframePair.EndTime, keyframePair.StartValue, keyframePair.EndValue);
                  }
              });
            MergeScale(parentScaleKeyframes).ForEachPair(
              (start, end) => {
                  var keyframePair = DrawData<double>(start, end, InterpolatingFunctions.Double);
                  if (keyframePair != null)
                  {
                      Sprite.ScaleVec(keyframePair.StartTime, keyframePair.EndTime,
                    (float)keyframePair.StartValue * _scaleBase, (float)keyframePair.EndValue * _scaleBase);
                  }
              });
        }
        public override void Draw()
        {
            _moveKeyframes.ForEachPair(
              (start, end) => {
                  var keyframePair = DrawData<Vector2>(start, end, InterpolatingFunctions.Vector2);
                  if (keyframePair != null)
                  {
                      Sprite.Move(keyframePair.StartTime, keyframePair.EndTime, keyframePair.StartValue, keyframePair.EndValue);
                  }
              });
            _rotateKeyframes.ForEachPair(
              (start, end) => {
                  var keyframePair = DrawData<double>(start, end, InterpolatingFunctions.Double);
                  if (keyframePair != null)
                  {
                      Sprite.Rotate(keyframePair.StartTime, keyframePair.EndTime, keyframePair.StartValue, keyframePair.EndValue);
                  }
              });
            _scaleKeyframes.ForEachPair(
              (start, end) => {
                  var keyframePair = DrawData<double>(start, end, InterpolatingFunctions.Double);
                  if (keyframePair != null)
                  {
                      Sprite.ScaleVec(keyframePair.StartTime, keyframePair.EndTime,
                    (float)keyframePair.StartValue * _scaleBase, (float)keyframePair.EndValue * _scaleBase);
                  }
              });
        }
    }
}
