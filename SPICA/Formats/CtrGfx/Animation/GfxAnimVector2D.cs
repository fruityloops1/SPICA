using SPICA.Serialization;
using SPICA.Serialization.Attributes;
using Newtonsoft.Json;

namespace SPICA.Formats.CtrGfx.Animation
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class GfxAnimVector2D : ICustomSerialization
    {
        [Ignore] private GfxFloatKeyFrameGroup[] Vector;

        public GfxFloatKeyFrameGroup X => Vector[0];
        public GfxFloatKeyFrameGroup Y => Vector[1];

        public GfxAnimVector2D()
        {
            Vector = new GfxFloatKeyFrameGroup[]
            {
                new GfxFloatKeyFrameGroup(),
                new GfxFloatKeyFrameGroup()
            };
        }

        [Ignore] private uint Flags;

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            Flags = GfxAnimVector.SetVector(Deserializer, Vector);
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            GfxAnimVector.WriteVector(Serializer, Vector, Flags);

            return true;
        }
    }
}
