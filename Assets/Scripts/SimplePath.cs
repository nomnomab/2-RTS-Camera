using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimplePath: MonoBehaviour {
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3[] _points;
    [SerializeField] private float _speed = 1f;

    private int _index = -1;
    private Vector3 _start, _end;
    private float _progress;

    private void Start() {
        _start = _target.position;
        _end = _points[0];
        _index = 0;
        _target.LookAt(_end);
    }

    private void Update() {
        if (_index < 0) {
            return;
        }
        
        _progress += Time.deltaTime * _speed;
        _target.position = Vector3.Lerp(_start, _end, _progress / Vector3.Distance(_start, _end));
        _target.LookAt(_end);

        if (Vector3.Distance(_target.position, _end) < 0.1f) {
            _start = _end;
            _index++;

            if (_index >= _points.Length) {
                _index = 0;
            }

            _end = _points[_index];
            _progress = 0;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        for (int index = 0; index < _points.Length; index++) {
            Vector3 point = _points[index];
            Gizmos.DrawSphere(point, 0.4f);

            int next = index + 1;
            Gizmos.color = next >= _points.Length ? Color.blue : Color.green;
            Gizmos.DrawLine(point, _points[next >= _points.Length ? 0 : next]);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimplePath))]
public class SimplePathEditor : Editor {
    private void OnSceneGUI() {
        SimplePath path = (SimplePath) target;

        SerializedProperty points = serializedObject.FindProperty("_points");

        for (int i = 0; i < points.arraySize; i++) {
            EditorGUI.BeginChangeCheck();
            
            SerializedProperty point = points.GetArrayElementAtIndex(i);
            Vector3 v = Handles.PositionHandle(point.vector3Value, Quaternion.identity);
            
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(path, "Changed Path Position");
                point.vector3Value = v;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif