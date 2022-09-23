using SPICA.Formats.Common;
using SPICA.Serialization.Attributes;
using System;

namespace SPICA.Formats.CtrGfx.Shader
{
    [TypeChoice(0x80000002u, typeof(GfxShader))]
    public class GfxShader : GfxObject
    {
        public byte[] ShaderData;

        public uint[] CommandsA;

        public GfxShaderInfo[] ShaderInfos;

        public uint[] CommandsB;

        [Inline, FixedLength(16)] public byte[] Padding;

        public GfxShader()
        {
            this.Header.MagicNumber = 0x52444853;
        }
    }

    public class GfxShaderInfo
    {
        [Inline, FixedLength(136)] public byte[] Data;
    }
}
