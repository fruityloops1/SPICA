﻿using SPICA.Serialization;
using SPICA.Serialization.Serializer;

using System.IO;

namespace SPICA.Formats.CtrGfx.Animation
{
    static class GfxAnimVector
    {
        public static uint SetVector(BinaryDeserializer Deserializer, GfxFloatKeyFrameGroup[] Vector)
        {
            long Position = Deserializer.BaseStream.Position;
            long pos = Deserializer.BaseStream.Position;

            uint Flags = GetFlagsFromElem(Deserializer, Position);

            uint ConstantMask = 1u;
            uint NotExistMask = 1u << Vector.Length;

            for (int Axis = 0; Axis < Vector.Length; Axis++)
            {
                Deserializer.BaseStream.Seek(Position, SeekOrigin.Begin);

                Position += 4;

                bool Constant = (Flags & ConstantMask) != 0;
                bool Exists   = (Flags & NotExistMask) == 0;

                if (Exists)
                {
                    Vector[Axis] = GfxFloatKeyFrameGroup.ReadGroup(Deserializer, Constant);
                }

                ConstantMask <<= 1;
                NotExistMask <<= 1;
            }

            Deserializer.BaseStream.Seek(pos + (4 * Vector.Length), SeekOrigin.Begin);

            return Flags;
        }

        public static void SetVector(BinaryDeserializer Deserializer, ref GfxFloatKeyFrameGroup Vector)
        {
            uint Flags = GetFlagsFromElem(Deserializer, Deserializer.BaseStream.Position);

            bool Constant = (Flags & 1) != 0;
            bool Exists   = (Flags & 2) == 0;

            if (Exists)
            {
                Vector = GfxFloatKeyFrameGroup.ReadGroup(Deserializer, Constant);
            }
        }

        public static uint GetFlagsFromElem(BinaryDeserializer Deserializer, long Position)
        {
            SeekToFlags(
                Deserializer.BaseStream,
                Deserializer.FileVersion,
                Position);

            uint Flags = Deserializer.Reader.ReadUInt32();

            Deserializer.BaseStream.Seek(Position, SeekOrigin.Begin);

            return Flags;
        }

        public static void WriteVector(BinarySerializer Serializer, GfxFloatKeyFrameGroup[] Vector, uint flags = 0)
        {
            uint ConstantMask = 1u;
            uint NotExistMask = 1u << Vector.Length;

            long Position = Serializer.BaseStream.Position;
            uint Flags = flags;

            //  Serializer.Writer.Write(0u);

            for (int ElemIndex = 0; ElemIndex < Vector.Length; ElemIndex++)
            {
                if (Vector[ElemIndex].KeyFrames.Count > 1)
                {
                    Serializer.Sections[(uint)GfxSectionId.Contents].Values.Add(new RefValue()
                    {
                        Value = Vector[ElemIndex],
                        Position = Serializer.BaseStream.Position
                    });

                    Serializer.Writer.Write(0u);
                }
                else if (Vector[ElemIndex].KeyFrames.Count == 0)
                {
                    Flags |= NotExistMask; Serializer.Writer.Write(0u);
                }
                else
                {
                    Flags |= ConstantMask; Serializer.Writer.Write(Vector[ElemIndex].KeyFrames[0].Value);
                }

                ConstantMask <<= 1;
                NotExistMask <<= 1;
            }


            SeekToFlags(
                 Serializer.BaseStream,
                 Serializer.FileVersion,
                 Position);

            Serializer.Writer.Write(flags);

            Serializer.BaseStream.Seek(Position + Vector.Length * 4, SeekOrigin.Begin);
        }

        public static void WriteVector(BinarySerializer Serializer, GfxFloatKeyFrameGroup Vector)
        {
            WriteVector(Serializer, new GfxFloatKeyFrameGroup[] { Vector });
        }

        public static void WriteFlagsToElem(BinarySerializer Serializer, long Position, uint Flags)
        {
            SeekToFlags(
                Serializer.BaseStream,
                Serializer.FileVersion,
                Position);

            Serializer.Writer.Write(Flags);
        }

        private static void SeekToFlags(Stream BaseStream, int Revision, long Position)
        {
            BaseStream.Seek(Position - 0xc, SeekOrigin.Begin);

            if (Revision < 0x05000000)
            {
                BaseStream.Seek(-8, SeekOrigin.Current);
            }
        }
    }
}
