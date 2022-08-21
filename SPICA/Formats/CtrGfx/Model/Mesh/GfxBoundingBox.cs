using SPICA.Math3D;

using System.Numerics;

namespace SPICA.Formats.CtrGfx.Model.Mesh
{
    public class GfxBoundingBox
    {
        public uint Flags = 2147483648; //0x80

        public Vector3   Center;
        public Matrix3x3 Orientation;
        public Vector3   Size;
    }
}
