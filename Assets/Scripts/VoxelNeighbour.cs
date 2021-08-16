public class VoxelNeighbour
{
    public readonly VoxelState parent;
    public int Lenght { get { return _neighbours.Length; } }

    public VoxelState this[int index]
    {
        get
        {
            if (_neighbours[index] == null)
            {
                _neighbours[index] = World.Instance.worldData.GetVoxel(parent.globalPosition + VoxelData.facesCheck[index]);
                ReturnNeighbour(index);
            }
            return _neighbours[index];
        }
        set
        {
            _neighbours[index] = value;
            ReturnNeighbour(index);
        }
    }

    private VoxelState[] _neighbours = new VoxelState[6];

    private void ReturnNeighbour(int index)
    {
        if (_neighbours[index] == null) return;
        if (_neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] != parent)
            _neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] = parent;
    }

    public VoxelNeighbour(VoxelState parenteValue) => parent = parenteValue;
}