using UnityEngine;

[System.Serializable]
public class VertData
{
    public Vector3 position;
    public Vector2 uv;

    public VertData(Vector3 pos, Vector2 uvValue)
    {
        position = pos;
        uv = uvValue;
    }


    public Vector3 GetRotatePosition(Vector3 angle)
    {
        Vector3 center = new Vector3(.5f, .5f, .5f);
        Vector3 direction = position - center;
        direction = Quaternion.Euler(angle) * direction;
        return direction + center;
    }
}
