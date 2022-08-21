using SPICA.Serialization;
using SPICA.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPICA.Formats.CtrH3D.Model.Mesh
{
    public struct H3DSubMeshCulling 
    {
        //TODO
        public int Unk1;
        public int Unk2;
        public int Unk3;
        public int Unk4;

         public List<SubMeshCullingFace> SubMeshes;

        public int Unk5;

        [Ignore] public ushort MaxIndex
        {
            get
            {
                ushort face = 0;
                foreach (var SM in SubMeshes)
                {
                    face = Math.Max(SM.Indices.Max(x => x), face);
                }
                return face;
            }
        }
    }

    [Inline]
    public class SubMeshCullingFace : ICustomSerialization
    {
        private uint BufferAddress;
        private uint BufferCount;

        [Ignore]
        public ushort[] Indices;

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            long Position = Deserializer.BaseStream.Position;

            Indices = new ushort[BufferCount];

            Deserializer.BaseStream.Seek(BufferAddress & 0x7fffffff, SeekOrigin.Begin);

            for (int Index = 0; Index < BufferCount; Index++)
                Indices[Index] = Deserializer.Reader.ReadUInt16();

            Deserializer.BaseStream.Seek(Position, SeekOrigin.Begin);
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            throw new Exception("Sub mesh culling does not support saving yet!");

            return false;
        }
    }
}
