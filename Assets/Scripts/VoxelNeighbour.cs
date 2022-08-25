using Data;

public class VoxelNeighbour
{
    private readonly VoxelState _parent;
    public int Lenght => _neighbours.Length;

    public VoxelState this[int index]
    {
        get
        {
            if (_neighbours[index] == null)
            {
                _neighbours[index] = World.Instance.worldData.GetVoxel(_parent.GlobalPosition + VoxelData.FacesCheck[index]);
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

    private readonly VoxelState[] _neighbours = new VoxelState[6];

    private void ReturnNeighbour(int index)
    {
        if (_neighbours[index] == null) return;
        if (VoxelData.revFaceCheckIndex != null && _neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] != _parent)
            _neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] = _parent;
    }

    public VoxelNeighbour(VoxelState parentValue) => _parent = parentValue;
}