/* Made by Oliver Beebe 2023  */
using U = UnityEngine;
using UnityEngine;
using System;

/// <summary> Stores an amplitude, frequency, and offset combined with an internal timer to make easy oscillation effects. </summary>
public class OscillationEffect {

    /// <summary> Oscillation effect for 1 dimension. </summary>
    [Serializable] public class Float   : Base<float, FloatFunction> {
        public Float(Base<float, FloatFunction> b) : base(b) { }
        public Float(float amplitude, float frequency, float offset) : base(amplitude, frequency, offset) { }
    }
    /// <summary> Oscillation effect for 2 dimension. </summary>
    [Serializable] public class Vector2 : Base<U.Vector2, Vector2Function> {
        public Vector2(Base<U.Vector2, Vector2Function> b) : base(b) { }
        public Vector2(U.Vector2 amplitude, U.Vector2 frequency, U.Vector2 offset) : base(amplitude, frequency, offset) { }
    }
    /// <summary> Oscillation effect for 3 dimension. </summary>
    [Serializable] public class Vector3 : Base<U.Vector3, Vector3Function> {
        public Vector3(Base<U.Vector3, Vector3Function> b) : base(b) { }
        public Vector3(U.Vector3 amplitude, U.Vector3 frequency, U.Vector3 offset) : base(amplitude, frequency, offset) { }
    }
    /// <summary> I built the whole class, I'm gonna use the whole class. 4 dimensional Oscillation effects! </summary>
    [Serializable] public class Vector4 : Base<U.Vector4, Vector4Function> {
        public Vector4(Base<U.Vector4, Vector4Function> b) : base(b) { }
        public Vector4(U.Vector4 amplitude, U.Vector4 frequency, U.Vector4 offset) : base(amplitude, frequency, offset) { }
    }

    public class Base<DimensionType, FunctionType> where FunctionType : struct, IFunction<DimensionType> {

        private static float Position    (float amplitude, float frequency, float offset, float time) =>  Mathf.Sin(time * frequency + offset * Mathf.Deg2Rad) * amplitude;
        private static float Velocity    (float amplitude, float frequency, float offset, float time) =>  Mathf.Cos(time * frequency + offset * Mathf.Deg2Rad) * amplitude * frequency;
        private static float Acceleration(float amplitude, float frequency, float offset, float time) => -MathF.Sin(time * frequency + offset * Mathf.Deg2Rad) * amplitude * frequency * frequency;

        private static readonly FunctionType functionType;
        private float timer;

        /// <summary> Size of the effect. </summary>
        [SerializeField, Tooltip("Size of the effect.")]   public DimensionType amplitude;
        /// <summary> Speed of the effect. </summary>
        [SerializeField, Tooltip("Speed of the effect.")]  public DimensionType frequency;
        /// <summary> Timing of the effect. </summary>
        [SerializeField, Tooltip("Timing of the effect.")] public DimensionType offset;

        public Base(Base<DimensionType, FunctionType> b) {
            amplitude = b.amplitude;
            frequency = b.frequency;
            offset = b.offset;
        }
        public Base(DimensionType amplitude, DimensionType frequency, DimensionType offset) {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.offset = offset;
        }

        /// <summary> Reset the internal timer. </summary>
        public void Reset() => timer = 0f;

        /// <summary> Can iterate the internal timer by deltaTime and evalute the effect at that time. </summary>
        /// <param name="deltaTime"> The deltaTime by which to progress the internal timer. Defaults to Time.deltaTime. </param>
        public DimensionType Evaluate(float? deltaTime = null) => EvaluateAt(timer += deltaTime ?? Time.deltaTime);

        /// <summary> Evaluates the effect at a specific time. </summary>
        /// <param name="time"> The time at which to evaluate the effect. </param>
        public DimensionType EvaluateAt(float time) => functionType.Get(Position, amplitude, frequency, offset, time);

        /// <summary> Can iterate the internal timer and evaluate the velocity of the effect at that time. </summary>
        /// <param name="deltaTime"> The deltaTime by which to progress the internal timer. Defaults to Time.deltaTime. </param>
        public DimensionType Velocity(float? deltaTime = null) => VelocityAt(timer += deltaTime ?? Time.deltaTime);

        /// <summary> Evaluates the velocity of the effect at a specific time. </summary>
        /// <param name="time"> The time at which to evaluate the first derivative of the effect. </param>
        public DimensionType VelocityAt(float time) => functionType.Get(Velocity, amplitude, frequency, offset, time);

        /// <summary> Can iterate the internal timer and evaluate the acceleration of the effect at that time. </summary>
        /// <param name="deltaTime"> The deltaTime by which to progress the internal timer. Defaults to Time.deltaTime. </param>
        public DimensionType Acceleration(float? deltaTime = null) => AccelerationAt(timer += deltaTime ?? Time.deltaTime);

        /// <summary> Evaluates the acceleration of the effect at a specific time. </summary>
        /// <param name="time"> The time at which to evaluate the first derivative of the effect. </param>
        public DimensionType AccelerationAt(float time) => functionType.Get(Acceleration, amplitude, frequency, offset, time);
    }

    public delegate float Function(float amplitude, float frequency, float offset, float time);
    public interface IFunction<T> { T Get(Function function, T amplitude, T frequency, T offset, float time); }

    public struct FloatFunction : IFunction<float> {
        public float Get(Function function, float amplitude, float frequency, float offset, float time) =>
            function(amplitude, frequency, offset, time);
    }

    public struct Vector2Function : IFunction<U.Vector2> {
        public U.Vector2 Get(Function function, U.Vector2 amplitude, U.Vector2 frequency, U.Vector2 offset, float time) => new(
            function(amplitude.x, frequency.x, offset.x, time),
            function(amplitude.y, frequency.y, offset.y, time));
    }

    public struct Vector3Function : IFunction<U.Vector3> {
        public U.Vector3 Get(Function function, U.Vector3 amplitude, U.Vector3 frequency, U.Vector3 offset, float time) => new(
            function(amplitude.x, frequency.x, offset.x, time),
            function(amplitude.y, frequency.y, offset.y, time),
            function(amplitude.z, frequency.z, offset.z, time));
    }

    public struct Vector4Function : IFunction<U.Vector4> {
        public U.Vector4 Get(Function function, U.Vector4 amplitude, U.Vector4 frequency, U.Vector4 offset, float time) => new(
            function(amplitude.x, frequency.x, offset.x, time),
            function(amplitude.y, frequency.y, offset.y, time),
            function(amplitude.z, frequency.z, offset.z, time),
            function(amplitude.w, frequency.w, offset.w, time));
    }

    private class Example {

        // this creates a serialized field for a float oscillation effect
        [SerializeField] private OscillationEffect.Float cameraRunBob;
        [SerializeField] private Transform cameraTransform;

        private void Update() {
            // this causes the camera's position to oscillate on the y axis
            cameraTransform.position = UnityEngine.Vector3.up * cameraRunBob.Evaluate();
        }
    }
}
