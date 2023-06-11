/* Made by Oliver Beebe 2023 */
using UnityEngine;

#region Editor
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(BufferTimer))]
public class BufferTimerPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(new(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("threshold"), label);
        EditorGUI.EndProperty();
    }
}

#endif
#endregion

/// <summary> Encapsulates the entire functionality for a buffer and buffer timer. </summary>
[System.Serializable]
public class BufferTimer {

    [Tooltip("Can buffer for this many seconds.")]
    [SerializeField] private float threshold;
    private float timer = Mathf.Infinity;

    /// <summary> Reset the buffer. </summary>
    public void Reset() => timer = Mathf.Infinity;
    /// <summary> Buffer a boolean value with regards to deltaTime. </summary>
    /// <param name="reset"> Resets buffer if true. </param>
    /// <param name="deltaTime"> deltatTime to iterate the buffer timer. Defaults to Time.deltaTime. </param>
    /// <returns> If the buffer is currently evaluating to true. </returns>
    public bool Buffer(bool reset, float? deltaTime = null) => (timer = reset ? 0 : timer + (deltaTime ?? Time.deltaTime)) <= threshold;

    private class Example {

        [SerializeField] private float jumpVelocity;
        // this creates a serialized field for the jump buffer, allowing you to set how long the buffer is in the inspector.
        [SerializeField] private BufferTimer jumpBuffer;
        [SerializeField] private Rigidbody rigidbody;

        private bool onGround;

        private void Respawn() {
            /* if you need to respawn the player or restart them in some way
               you should call .Reset() on your buffer so that the player can't
               buffer something in between respawns. */
            jumpBuffer.Reset();
        }

        private void Update() {

            /* 
            simply call the buffer .Buffer() function with the bool you want to buffer

            you probably want to use an instantaneous variable (like GetKeyDown) vs a continuous one (like GetKey)
            since the buffer timer is reset whenever the input is true.

            it is best to store the results of the buffer into a local variable (as shown here)
            since calling the function multiple times could produce unexpected results with the timer.
            */
            bool canJump = jumpBuffer.Buffer(Input.GetKeyDown(KeyCode.Space));

            /* with this code, the player can press the jump button while not on the ground,
               and if they touch the ground within the time specified by the jumpBuffer,
               they will instantly jump once they land */
            if (canJump && onGround) {
                rigidbody.velocity = Vector3.up * jumpVelocity;
            }
        }
    }
}
