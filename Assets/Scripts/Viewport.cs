using UnityEngine;

public struct Viewport {
    public float X { get; private set; }
    public float Z { get; private set; }
    public float Width { get; private set; }
    public float Depth { get; private set; }
    public Vector3 Center => new Vector3(X + Width / 2, 0, Z + Depth / 2);

    public Viewport(float x, float z, float width, float depth) {
        X = x;
        Z = z;
        Width = width;
        Depth = depth;
    }
        
    public void UpdatePosition(float x, float z) {
        X = x;
        Z = z;
    }
        
    public void UpdateSize(float width, float depth) {
        Width = width;
        Depth = depth;
    }
}