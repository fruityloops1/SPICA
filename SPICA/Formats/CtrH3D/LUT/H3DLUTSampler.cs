﻿using SPICA.Formats.Common;
using SPICA.PICA;
using SPICA.Serialization;
using SPICA.Serialization.Attributes;

namespace SPICA.Formats.CtrH3D.LUT
{
    [Inline]
    public class H3DLUTSampler : ICustomSerialization, INamed
    {
        [Padding(4)] public H3DLUTFlags Flags;

        private uint[] Commands;

        private string _Name;

        public string Name
        {
            get => _Name;
            set => _Name = value ?? throw Exceptions.GetNullException("Name");
        }

        [Ignore] private float[] _Deltas;

        [Ignore] private float[] _Table;

        public float[] Table
        {
            get => _Table;
            set
            {
                if (value == null)
                {
                    throw Exceptions.GetNullException("Table");
                }

                if (value.Length != 256)
                {
                    throw Exceptions.GetLengthNotEqualException("Table", 256);
                }
                for (int i = 0; i < value.Length; i++)
                {
                    if (i < _Table.Length - 1)
                        _Deltas[i] = _Table[i + 1] - _Table[i];
                }

                _Table = value;
            }
        }

        public H3DLUTSampler()
        {
            _Deltas = new float[256];
            _Table = new float[256];
        }

        public void CreateLerp(int startIndex, float startValue, int endIndex, float endValue)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                float weight = (float)(i - startIndex) / (float)(endIndex - startIndex);
                _Table[i] = Lerp(startValue, endValue, weight);
            }
        }

        static float Lerp(float a, float b, double weight) {
            return (float)(a * (1 - weight) + b * weight);
        }

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            uint Index = 0;

            PICACommandReader Reader = new PICACommandReader(Commands);

            while (Reader.HasCommand)
            {
                PICACommand Cmd = Reader.GetCommand();
                if (Cmd.Register == PICARegister.GPUREG_LIGHTING_LUT_INDEX)
                {
                    Index = Cmd.Parameters[0] & 0xff;
                }
                else if (
                    Cmd.Register >= PICARegister.GPUREG_LIGHTING_LUT_DATA0 &&
                    Cmd.Register <= PICARegister.GPUREG_LIGHTING_LUT_DATA7)
                {
                    foreach (uint Param in Cmd.Parameters)
                    {
                        _Table[Index] = (Param & 0xfff) / (float)0xfff;
                        _Deltas[Index] = (Param >> 12)  / (float)0x7ff;

                        Index++;
                    }
                }
            }
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            uint[] QuantizedValues = new uint[256];

            for (int Index = 0; Index < _Table.Length; Index++)
            {
                float Difference = _Deltas[Index];

                int Value = (int)(_Table[Index] * 0xfff);
                int Diff  = (int)(Difference    * 0x7ff);

                QuantizedValues[Index] = (uint)(Value | (Diff << 12)) & 0xffffff;
            }

            PICACommandWriter Writer = new PICACommandWriter();

            Writer.SetCommands(PICARegister.GPUREG_LIGHTING_LUT_DATA0, false, 0xf, QuantizedValues);

            Writer.WriteEnd();

            var commands = Commands.ToArray();

            Commands = Writer.GetBuffer();

            return false;
        }
    }
}
