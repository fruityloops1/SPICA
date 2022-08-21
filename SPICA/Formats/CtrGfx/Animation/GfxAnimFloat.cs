using SPICA.Serialization;
using SPICA.Serialization.Attributes;
using Newtonsoft.Json;

namespace SPICA.Formats.CtrGfx.Animation
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class GfxAnimFloat : ICustomSerialization
    {
        [Ignore] private GfxFloatKeyFrameGroup _Value;

        public GfxFloatKeyFrameGroup Value => _Value;

        public GfxAnimFloat()
        {
            _Value = new GfxFloatKeyFrameGroup();
        }

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            GfxAnimVector.SetVector(Deserializer, ref _Value);
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            GfxAnimVector.WriteVector(Serializer, _Value);

            return true;
        }
    }
}
