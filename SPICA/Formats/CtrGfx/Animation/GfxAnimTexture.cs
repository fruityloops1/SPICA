using SPICA.Serialization;
using SPICA.Serialization.Attributes;
using SPICA.Serialization.Serializer;
using SPICA.Formats.CtrGfx.Model.Material;
using Newtonsoft.Json;

namespace SPICA.Formats.CtrGfx.Animation
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class GfxAnimTexture : ICustomSerialization
    {
        [Ignore] private GfxFloatKeyFrameGroup[] Vector;

        [Ignore] private uint Flags;

        [Ignore] public GfxTextureReference[] TextureList;

        public GfxFloatKeyFrameGroup Texture => Vector[0];

        public GfxAnimTexture()
        {
            Vector = new GfxFloatKeyFrameGroup[]
            {
                new GfxFloatKeyFrameGroup(),
            };
            TextureList = new GfxTextureReference[0];
        }

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            Flags = GfxAnimVector.SetVector(Deserializer, Vector);
            uint numTextures = Deserializer.Reader.ReadUInt32();
            Deserializer.BaseStream.Seek(Deserializer.ReadPointer(), System.IO.SeekOrigin.Begin);

            long pos = Deserializer.BaseStream.Position;

            TextureList = new GfxTextureReference[numTextures];
            for (int i = 0; i < numTextures; i++)
            {
                Deserializer.BaseStream.Position = pos + (i * 4);
                Deserializer.BaseStream.Seek(Deserializer.ReadPointer(), System.IO.SeekOrigin.Begin);
                TextureList[i] = Deserializer.Deserialize<GfxTextureReference>();
            }

            Deserializer.BaseStream.Position = pos;
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            GfxAnimVector.WriteVector(Serializer, Vector,Flags);

            Serializer.Writer.Write(TextureList.Length);
            Serializer.Writer.Write(4);
            for (int i = 0; i < TextureList.Length; i++)
            {
                Serializer.Sections[(uint)GfxSectionId.Contents].Values.Add(new RefValue()
                {
                    Value = TextureList[i],
                    Position = Serializer.BaseStream.Position
                });
                Serializer.Writer.Write(0);
            }
            return true;
        }
    }
}
