using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class FaceMeshData
    {
        public string direction;
        public Vector3 normal;
        public VertData[] vertsData;
        public int[] triangels;
    }
}