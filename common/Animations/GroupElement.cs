using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections;
using StorybrewCommon.Storyboarding;

namespace StorybrewCommon.Animations
{
	public abstract class GroupElement
	{
		protected KeyframedValue<Vector2> _moveKeyframes;
		protected KeyframedValue<double> _rotateKeyframes;
		protected KeyframedValue<double> _scaleKeyframes;

		protected readonly Vector2 DEFAULT_MOVE = Vector2.Zero;
		protected const double DEFAULT_ROTATE = 0.0;
		protected const double DEFAULT_SCALE = 1.0;

		const double DEFAULT_MOVE_TOLERANCE = 0.01;
		const double DEFAULT_TIME = 0;
		const double KEYFRAME_TIMESTEP = 16; // TODO: Probably make this modifiable and related to the BPM

		public GroupElement()
		{
			_moveKeyframes = new KeyframedValue<Vector2>(InterpolatingFunctions.Vector2, DEFAULT_MOVE);
			_rotateKeyframes = new KeyframedValue<double>(InterpolatingFunctions.Double, DEFAULT_ROTATE);
			_scaleKeyframes = new KeyframedValue<double>(InterpolatingFunctions.Double, DEFAULT_SCALE);
		}

		public void Move(double time, Vector2 position)
		{
			if (_moveKeyframes.Count > 0) _moveKeyframes.Add(time);
			_moveKeyframes.Add(time, position);
		}

		public void Move(double startTime, double endTime, Vector2 startPosition, Vector2 endPosition)
		{
			if (_moveKeyframes.Count > 0) _moveKeyframes.Add(startTime);
			_moveKeyframes.Add(startTime, startPosition);
			_moveKeyframes.Add(endTime, endPosition);
		}

		public void Move(OsbEasing easing, double startTime, double endTime, Vector2 startPosition, Vector2 endPosition)
		{
			if (_moveKeyframes.Count > 0) _moveKeyframes.Add(startTime);
			_moveKeyframes.Add(startTime, startPosition);

			var tempKeyframes = new KeyframedValue<Vector2>(InterpolatingFunctions.Vector2, DEFAULT_MOVE);
			var easeFunction = EasingFunctions.ToEasingFunction(easing);
			for (double time = startTime; time < endTime; time += KEYFRAME_TIMESTEP)
			{
				tempKeyframes.Add(time, InterpolatingFunctions.Vector2(startPosition, endPosition,
				  easeFunction((time - startTime) / (endTime - startTime))));
			}
			tempKeyframes.Add(endTime, endPosition);
			tempKeyframes.Simplify2dKeyframes(DEFAULT_MOVE_TOLERANCE, (v) => { return v; });
			tempKeyframes.TransferKeyframes(_moveKeyframes);
		}

		public void Rotate(double time, double rotation)
		{
			if (_rotateKeyframes.Count > 0) _rotateKeyframes.Add(time);
			_rotateKeyframes.Add(time, rotation);
		}

		public void Rotate(double startTime, double endTime, double startRotation, double endRotation)
		{
			if (_rotateKeyframes.Count > 0) _rotateKeyframes.Add(startTime);
			_rotateKeyframes.Add(startTime, startRotation);
			_rotateKeyframes.Add(endTime, endRotation);
		}

		public void Rotate(OsbEasing easing, double startTime, double endTime, double startRotation, double endRotation)
		{
			if (_rotateKeyframes.Count > 0) _rotateKeyframes.Add(startTime);
			_rotateKeyframes.Add(startTime, startRotation);

			var tempKeyframes = new KeyframedValue<double>(InterpolatingFunctions.Double, DEFAULT_ROTATE);
			var easeFunction = EasingFunctions.ToEasingFunction(easing);
			for (double time = startTime; time < endTime; time += KEYFRAME_TIMESTEP)
			{
				tempKeyframes.Add(time, InterpolatingFunctions.Double(startRotation, endRotation,
				  easeFunction((time - startTime) / (endTime - startTime))));
			}
			tempKeyframes.Add(endTime, endRotation);
			// tempKeyframes.SimplifyKeyframes(DEFAULT_ROTATE_TOLERANCE, (v) => {return v;});
			tempKeyframes.TransferKeyframes(_rotateKeyframes);
		}

		public void Scale(double time, double scale)
		{
			if (_scaleKeyframes.Count > 0) _scaleKeyframes.Add(time);
			_scaleKeyframes.Add(time, scale);
		}

		public void Scale(double startTime, double endTime, double startScale, double endScale)
		{
			if (_scaleKeyframes.Count > 0) _scaleKeyframes.Add(startTime);
			_scaleKeyframes.Add(startTime, startScale);
			_scaleKeyframes.Add(endTime, endScale);
		}

		public void Scale(OsbEasing easing, double startTime, double endTime, double startScale, double endScale)
		{
			if (_scaleKeyframes.Count > 0) _scaleKeyframes.Add(startTime);
			_scaleKeyframes.Add(startTime, startScale);

			var tempKeyframes = new KeyframedValue<double>(InterpolatingFunctions.Double, DEFAULT_ROTATE);
			var easeFunction = EasingFunctions.ToEasingFunction(easing);
			for (double time = startTime; time < endTime; time += KEYFRAME_TIMESTEP)
			{
				tempKeyframes.Add(time, InterpolatingFunctions.Double(startScale, endScale,
				  easeFunction((time - startTime) / (endTime - startTime))));
			}
			tempKeyframes.Add(endTime, endScale);
			// tempKeyframes.SimplifyKeyframes(DEFAULT_ROTATE_TOLERANCE, (v) => {return v;});
			tempKeyframes.TransferKeyframes(_scaleKeyframes);
		}

		public abstract void Draw(KeyframedValue<Vector2> parentMoveKeyframes, KeyframedValue<double> parentRotateKeyframes, KeyframedValue<double> parentScaleKeyframes);
		public abstract void Draw();

		protected KeyframedValue<Vector2> MergeMove(KeyframedValue<Vector2> parentMoveKeyframes, KeyframedValue<double> parentRotateKeyframes, KeyframedValue<double> parentScaleKeyframes)
		{
			KeyframedValue<Vector2> mergeMoveKeyframes = new KeyframedValue<Vector2>(InterpolatingFunctions.Vector2, DEFAULT_MOVE);

			var moveEnumerator = _moveKeyframes.GetEnumerator();
			var parentMoveEnumerator = parentMoveKeyframes.GetEnumerator();
			var parentRotateEnumerator = parentRotateKeyframes.GetEnumerator();
			var parentScaleEnumerator = parentScaleKeyframes.GetEnumerator();

			Keyframe<Vector2> moveStart, moveEnd;
			Keyframe<Vector2> parentMoveStart, parentMoveEnd;
			Keyframe<double> parentRotateStart, parentRotateEnd;
			Keyframe<double> parentScaleStart, parentScaleEnd;
			bool moveCheck, parentMoveCheck, parentRotateCheck, parentScaleCheck;

			InitializeStartEnd(ref moveEnumerator, out moveStart, out moveEnd, out moveCheck, DEFAULT_MOVE);
			InitializeStartEnd(ref parentMoveEnumerator, out parentMoveStart, out parentMoveEnd, out parentMoveCheck, DEFAULT_MOVE);
			InitializeStartEnd(ref parentRotateEnumerator, out parentRotateStart, out parentRotateEnd, out parentRotateCheck, DEFAULT_ROTATE);
			InitializeStartEnd(ref parentScaleEnumerator, out parentScaleStart, out parentScaleEnd, out parentScaleCheck, DEFAULT_SCALE);

			double previousTime = double.NaN;
			do
			{
				double time = double.PositiveInfinity;
				if (moveCheck && moveEnd.Time < time) time = moveEnd.Time;
				if (parentMoveCheck && parentMoveEnd.Time < time) time = parentMoveEnd.Time;
				if (parentRotateCheck && parentRotateEnd.Time < time) time = parentRotateEnd.Time;
				if (parentScaleCheck && parentScaleEnd.Time < time) time = parentScaleEnd.Time;

				if (!double.IsNaN(previousTime)) time = Math.Min(previousTime + KEYFRAME_TIMESTEP, time);
				previousTime = time;

				var move = Interpolate<Vector2>(moveStart, moveEnd, time, InterpolatingFunctions.Vector2);
				var parentMove = Interpolate<Vector2>(parentMoveStart, parentMoveEnd, time, InterpolatingFunctions.Vector2);
				var parentRotate = Interpolate<double>(parentRotateStart, parentRotateEnd, time, InterpolatingFunctions.Double);
				var parentScale = Interpolate<double>(parentScaleStart, parentScaleEnd, time, InterpolatingFunctions.Double);
				mergeMoveKeyframes.Add(time, MoveFormula(move, parentMove, parentRotate, parentScale));

				bool isRepeat = false;
				CheckNextKeyframe<Vector2>(ref moveEnumerator, ref moveStart, ref moveEnd, ref moveCheck, ref move, ref time, ref isRepeat);
				CheckNextKeyframe<Vector2>(ref parentMoveEnumerator, ref parentMoveStart, ref parentMoveEnd, ref parentMoveCheck, ref parentMove, ref time, ref isRepeat);
				CheckNextKeyframe<double>(ref parentRotateEnumerator, ref parentRotateStart, ref parentRotateEnd, ref parentRotateCheck, ref parentRotate, ref time, ref isRepeat);
				CheckNextKeyframe<double>(ref parentScaleEnumerator, ref parentScaleStart, ref parentScaleEnd, ref parentScaleCheck, ref parentScale, ref time, ref isRepeat);
				if (isRepeat) mergeMoveKeyframes.Add(time, MoveFormula(move, parentMove, parentRotate, parentScale));
			}
			while (moveCheck || parentMoveCheck || parentRotateCheck || parentScaleCheck);

			mergeMoveKeyframes.Simplify2dKeyframes(DEFAULT_MOVE_TOLERANCE, (v) => { return v; });

			return mergeMoveKeyframes;
		}

		protected KeyframedValue<double> MergeRotate(KeyframedValue<double> parentRotateKeyframes)
		{
			KeyframedValue<double> mergeRotateKeyframes = new KeyframedValue<double>(InterpolatingFunctions.Double, DEFAULT_ROTATE);

			var rotateEnumerator = _rotateKeyframes.GetEnumerator();
			var parentRotateEnumerator = parentRotateKeyframes.GetEnumerator();

			Keyframe<double> rotateStart, rotateEnd;
			Keyframe<double> parentRotateStart, parentRotateEnd;
			bool rotateCheck, parentRotateCheck;

			InitializeStartEnd(ref rotateEnumerator, out rotateStart, out rotateEnd, out rotateCheck, DEFAULT_ROTATE);
			InitializeStartEnd(ref parentRotateEnumerator, out parentRotateStart, out parentRotateEnd, out parentRotateCheck, DEFAULT_ROTATE);

			do
			{
				double time = double.PositiveInfinity;
				if (rotateCheck && rotateEnd.Time < time) time = rotateEnd.Time;
				if (parentRotateCheck && parentRotateEnd.Time < time) time = parentRotateEnd.Time;

				var rotate = Interpolate<double>(rotateStart, rotateEnd, time, InterpolatingFunctions.Double);
				var parentRotate = Interpolate<double>(parentRotateStart, parentRotateEnd, time, InterpolatingFunctions.Double);
				mergeRotateKeyframes.Add(time, RotateFormula(rotate, parentRotate));

				bool isRepeat = false;
				CheckNextKeyframe<double>(ref rotateEnumerator, ref rotateStart, ref rotateEnd, ref rotateCheck, ref rotate, ref time, ref isRepeat);
				CheckNextKeyframe<double>(ref parentRotateEnumerator, ref parentRotateStart, ref parentRotateEnd, ref parentRotateCheck, ref parentRotate, ref time, ref isRepeat);
				if (isRepeat) mergeRotateKeyframes.Add(time, RotateFormula(rotate, parentRotate));
			}
			while (rotateCheck || parentRotateCheck);

			return mergeRotateKeyframes;
		}

		protected KeyframedValue<double> MergeScale(KeyframedValue<double> parentScaleKeyframes)
		{
			KeyframedValue<double> mergeScaleKeyframes = new KeyframedValue<double>(InterpolatingFunctions.Double, DEFAULT_SCALE);

			var scaleEnumerator = _scaleKeyframes.GetEnumerator();
			var parentScaleEnumerator = parentScaleKeyframes.GetEnumerator();

			Keyframe<double> scaleStart, scaleEnd;
			Keyframe<double> parentScaleStart, parentScaleEnd;
			bool scaleCheck, parentScaleCheck;

			// Initialize:
			// _Start, _End <- first element (or default)
			InitializeStartEnd(ref scaleEnumerator, out scaleStart, out scaleEnd, out scaleCheck, DEFAULT_SCALE);
			InitializeStartEnd(ref parentScaleEnumerator, out parentScaleStart, out parentScaleEnd, out parentScaleCheck, DEFAULT_SCALE);

			do
			{
				// For each _, time <- Min(_End.Time) only if _Check is true
				// Basically, take the earliest endTime
				double time = double.PositiveInfinity;
				if (scaleCheck && scaleEnd.Time < time) time = scaleEnd.Time;
				if (parentScaleCheck && parentScaleEnd.Time < time) time = parentScaleEnd.Time;

				// Interpolate values then add to KeyframedValues
				var scale = Interpolate<double>(scaleStart, scaleEnd, time, InterpolatingFunctions.Double);
				var parentScale = Interpolate<double>(parentScaleStart, parentScaleEnd, time, InterpolatingFunctions.Double);
				mergeScaleKeyframes.Add(time, ScaleFormula(scale, parentScale));

				// Check for overlapping keyframes
				// Also updates _Enumerator, _Start, _End, _Check, _Scale, _ParentScale, _isRepeat
				bool isRepeat = false;
				CheckNextKeyframe<double>(ref scaleEnumerator, ref scaleStart, ref scaleEnd, ref scaleCheck, ref scale, ref time, ref isRepeat);
				CheckNextKeyframe<double>(ref parentScaleEnumerator, ref parentScaleStart, ref parentScaleEnd, ref parentScaleCheck, ref parentScale, ref time, ref isRepeat);
				if (isRepeat) mergeScaleKeyframes.Add(time, ScaleFormula(scale, parentScale));
			}
			while (scaleCheck || parentScaleCheck);

			return mergeScaleKeyframes;
		}

		private void InitializeStartEnd<T>(ref IEnumerator<Keyframe<T>> enumerator, out Keyframe<T> start, out Keyframe<T> end, out bool check, T defaultValue)
		{
			if (enumerator.MoveNext())
			{
				start = enumerator.Current;
				end = start;
				check = true;
			}
			else
			{
				start = new Keyframe<T>(DEFAULT_TIME, defaultValue);
				end = start;
				check = false;
			}
		}

		private void CheckNextKeyframe<T>(ref IEnumerator<Keyframe<T>> enumerator, ref Keyframe<T> start, ref Keyframe<T> end, ref bool check, ref T current, ref double time, ref bool isRepeat) where T : IEquatable<T>
		{
			if (check && time == end.Time)
			{
				GetNextKeyframe<T>(ref enumerator, ref start, ref end, ref check);
				if (!current.Equals(start.Value))
				{
					current = start.Value;
					isRepeat = true;
				}
			}
		}

		private void GetNextKeyframe<T>(ref IEnumerator<Keyframe<T>> enumerator, ref Keyframe<T> start, ref Keyframe<T> end, ref bool check)
		{
			if (!check) return;
			while (true)
			{
				if (enumerator.MoveNext())
				{
					var current = enumerator.Current;
					if (current.Time > end.Time)
					{
						start = end;
						end = current;
						return;
					}
					end = current;
				}
				else
				{
					start = end;
					check = false;
					return;
				}
			}
		}

		private T Interpolate<T>(Keyframe<T> start, Keyframe<T> end, double time, Func<T, T, double, T> interpolate)
		{
			if (time < start.Time) return start.Value;
			if (time >= end.Time) return end.Value;
			return interpolate(start.Value, end.Value, (time - start.Time) / (end.Time - start.Time));
		}

		private Vector2 MoveFormula(Vector2 move, Vector2 parentMove, double parentRotate, double parentScale)
		{
			float cosR = (float)Math.Cos(parentRotate);
			float sinR = (float)Math.Sin(parentRotate);
			Vector2 pos = (float)parentScale * move;
			return new Vector2(
			  pos.X * cosR + pos.Y * -sinR + parentMove.X,
			  pos.X * sinR + pos.Y * cosR + parentMove.Y
			);
		}

		private double RotateFormula(double rotate, double parentRotate) 
		{ 
			return rotate + parentRotate; 
		}

		private double ScaleFormula(double scale, double parentScale) 
		{ 
			return scale * parentScale; 
		}
	}
}