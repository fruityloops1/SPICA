using SPICA.Formats.Common;
using SPICA.Math3D;

using System.Numerics;
using Newtonsoft.Json;

namespace SPICA.Formats.CtrGfx.Model
{
    public class GfxBone : INamed
    {
        private string _Name;

        public string Name
        {
            get => _Name;
            set => _Name = value ?? throw Exceptions.GetNullException("Name");
        }

        public GfxBoneFlags Flags;

        public int Index;
        public int ParentIndex;

        [JsonIgnore]
        public GfxBone Parent;
        [JsonIgnore]
        public GfxBone Child;
        [JsonIgnore]
        public GfxBone PrevSibling;
        [JsonIgnore]
        public GfxBone NextSibling;

        public Vector3 Scale;
        public Vector3 Rotation;
        public Vector3 Translation;

        public Matrix3x4 LocalTransform;
        public Matrix3x4 WorldTransform;
        public Matrix3x4 InvWorldTransform;

        public GfxBillboardMode BillboardMode;

        public GfxDict<GfxMetaData> MetaData;

        public void UpdateMatrices()
        {
            LocalTransform = new Matrix3x4(CalculateLocalMatrix());
            WorldTransform = new Matrix3x4(CalculateWorldMatrix());
        }


        /// <summary>
        /// Updates the current bone transform flags.
        /// These flags determine what matrices can be ignored for matrix updating.
        /// </summary>
        public void UpdateTransformFlags()
        {
            GfxBoneFlags flags = this.Flags;

            //Reset transform flags
            flags &= ~GfxBoneFlags.IsTranslationZero;
            flags &= ~GfxBoneFlags.IsScaleVolumeOne;
            flags &= ~GfxBoneFlags.IsRotationZero;
            flags &= ~GfxBoneFlags.IsScaleUniform;

            //SRT checks to update matrices
            if (this.Translation == Vector3.Zero)
                flags |= GfxBoneFlags.IsTranslationZero;
            if (this.Scale == Vector3.One)
                flags |= GfxBoneFlags.IsScaleVolumeOne;
            if (this.Rotation == Vector3.Zero)
                flags |= GfxBoneFlags.IsRotationZero;
            //Extra scale flags
            if (this.Scale.X == this.Scale.Y && this.Scale.X == this.Scale.Z)
                flags |= GfxBoneFlags.IsScaleUniform;

            this.Flags = flags;
        }

        public Matrix4x4 CalculateWorldMatrix()
        {
            Matrix4x4 transform = CalculateLocalMatrix();
            if (Parent != null)
                return transform * Parent.CalculateWorldMatrix();
            return transform;
        }

        public Matrix4x4 CalculateLocalMatrix()
        {
            return Matrix4x4.CreateTranslation(Translation) *
                (Matrix4x4.CreateRotationX(Rotation.X) *
                Matrix4x4.CreateRotationY(Rotation.Y) *
                Matrix4x4.CreateRotationZ(Rotation.Z)) *
                Matrix4x4.CreateScale(Scale);
        }
    }
}
