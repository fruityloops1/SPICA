using Newtonsoft.Json;
using SPICA.Math3D;
using SPICA.Serialization;
using SPICA.Serialization.Attributes;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SPICA.Formats.CtrGfx.Animation
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class GfxAnimQuatTransform : ICustomSerialization
    {
        [Ignore] public readonly List<Vector3> Scales;
        [Ignore] public readonly List<Quaternion> Rotations;
        [Ignore] public readonly List<Vector3> Translations;

        [Ignore] public readonly List<uint> SFlags;
        [Ignore] public readonly List<uint> RFlags;
        [Ignore] public readonly List<uint> TFlags;

        public bool HasScale => Scales.Count > 0;
        public bool HasRotation => Rotations.Count > 0;
        public bool HasTranslation => Translations.Count > 0;

        [Ignore] private uint CurveRelPtr;

        [Ignore] private float StartFrame;

        public GfxAnimQuatTransform()
        {
            Scales = new List<Vector3>();
            Rotations = new List<Quaternion>();
            Translations = new List<Vector3>();
            SFlags = new List<uint>();
            RFlags = new List<uint>();
            TFlags = new List<uint>();
        }

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            Deserializer.BaseStream.Seek(-0xc, SeekOrigin.Current);

            var Flags = Deserializer.Reader.ReadUInt32();
            Deserializer.Reader.ReadBytes(8);

            uint[] Addresses = new uint[3];

            uint ConstantMask = (uint)GfxAnimQuatTransformFlags.IsScaleConstant;
            uint NotExistMask = (uint)GfxAnimQuatTransformFlags.IsScaleInexistent;

            Addresses[1] = Deserializer.ReadPointer();
            Addresses[2] = Deserializer.ReadPointer();
            Addresses[0] = Deserializer.ReadPointer();

            for (int ElemIndex = 0; ElemIndex < 3; ElemIndex++)
            {
                bool Constant = (Flags & ConstantMask) != 0;
                bool Exists = (Flags & NotExistMask) == 0;

                if (Exists)
                {
                    Deserializer.BaseStream.Seek(Addresses[ElemIndex], SeekOrigin.Begin);

                    StartFrame = Deserializer.Reader.ReadSingle();
                    float EndFrame = Deserializer.Reader.ReadSingle();
                    CurveRelPtr = Deserializer.Reader.ReadUInt32();
                    bool IsConstant = Deserializer.Reader.ReadUInt32() != 0;

                    int Count = IsConstant ? 1 : (int)(EndFrame - StartFrame + 1);

                    for (int Index = 0; Index < Count; Index++)
                    {
                        switch (ElemIndex)
                        {
                            case 0:
                                Scales.Add(Deserializer.Reader.ReadVector3());
                                SFlags.Add(Deserializer.Reader.ReadUInt32());
                                break;
                            case 1:
                                Rotations.Add(Deserializer.Reader.ReadQuaternion());
                                RFlags.Add(Deserializer.Reader.ReadUInt32());
                                break;
                            case 2:
                                Translations.Add(Deserializer.Reader.ReadVector3());
                                TFlags.Add(Deserializer.Reader.ReadUInt32());
                                break;
                        }
                    }
                }

                ConstantMask >>= 1;
                NotExistMask >>= 1;
            }
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            GfxAnimQuatTransformFlags Flags = 0;

            uint ConstantMask = (uint)GfxAnimQuatTransformFlags.IsScaleConstant;
            uint NotExistMask = (uint)GfxAnimQuatTransformFlags.IsScaleInexistent;

            long DescPosition = Serializer.BaseStream.Position;
            long DataPosition = DescPosition + 0xc;

            int[] indices = new int[]
            {
                1, 2, 0
            };

            for (int ElemIndex = 0; ElemIndex < 3; ElemIndex++)
            {
                IList Elem = null;
                IList FlagList = new uint[3];

                switch (ElemIndex)
                {
                    case 0: Elem =  Rotations; break;
                    case 1: Elem = Translations; break;
                    case 2: Elem = Scales; break;
                }

                switch (ElemIndex)
                {
                    case 0: FlagList = RFlags; break;
                    case 1: FlagList = TFlags; break;
                    case 2: FlagList = SFlags; break;
                }

                if (Elem.Count > 0)
                {
                    Serializer.BaseStream.Seek(DescPosition + ElemIndex * 4, SeekOrigin.Begin);

                    Serializer.WritePointer((uint)DataPosition);

                    Serializer.BaseStream.Seek(DataPosition, SeekOrigin.Begin);

                    Serializer.Writer.Write(StartFrame); //Start Frame
                    Serializer.Writer.Write((float)Elem.Count); //End Frame
                    Serializer.Writer.Write(CurveRelPtr); //Curve Relative Pointer?
                    Serializer.Writer.Write(Elem.Count == 1 ? 1 : 0); //Constant

                    int idx = 0;
                    foreach (object Vector in Elem)
                    {
                        if (Vector is Vector3)
                        {
                            Serializer.Writer.Write((Vector3)Vector);
                        }
                        else
                        {
                            Serializer.Writer.Write((Quaternion)Vector);
                        }
                        //TODO: Segment Flags
                        Serializer.Writer.Write((uint)FlagList[idx++]);
                    }

                    if (Elem.Count == 1) //Flag unused?
                    {
                       // Flags |= (GfxAnimQuatTransformFlags)(ConstantMask >> indices[ElemIndex]);
                    }

                    DataPosition = Serializer.BaseStream.Position;
                }
                else
                {
                    Flags |= (GfxAnimQuatTransformFlags)(NotExistMask >> indices[ElemIndex]);
                }
            }

            Serializer.BaseStream.Seek(DescPosition - 0xc, SeekOrigin.Begin);

            Serializer.Writer.Write((uint)Flags);

            Serializer.BaseStream.Seek(DataPosition, SeekOrigin.Begin);

            return true;
        }
    }
}
