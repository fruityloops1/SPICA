using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace SPICA.Formats.CtrGfx.Model.Material
{
    public struct GfxFragLight
    {
        public GfxFragmentFlags Flags;

        [JsonConverter(typeof(StringEnumConverter))]
        public GfxTranslucencyKind TranslucencyKind;

        [JsonConverter(typeof(StringEnumConverter))]
        public GfxFresnelSelector FresnelSelector;

        public int BumpTexture;

        [JsonConverter(typeof(StringEnumConverter))]
        public GfxBumpMode BumpMode;

        public bool IsBumpRenormalize;

        internal byte[] GetBytes()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write((uint)Flags);
                Writer.Write((uint)TranslucencyKind);
                Writer.Write((uint)FresnelSelector);
                Writer.Write(BumpTexture);
                Writer.Write((uint)BumpMode);
                Writer.Write((byte)(IsBumpRenormalize ? 1 : 0));

                return MS.ToArray();
            }
        }
    }
}
