using OpenTK.Graphics.OpenGL;

using SPICA.Formats.CtrH3D.Texture;
using SPICA.PICA.Commands;
using System;

namespace SPICA.Rendering
{
    public class Texture : IDisposable
    {
        public string Name;

        public int Id;

        private TextureTarget Target;

        public Texture(H3DTexture Texture)
        {
            Name = Texture.Name;

            Id = GL.GenTexture();

            Target = Texture.IsCubeTexture
                ? TextureTarget.TextureCubeMap
                : TextureTarget.Texture2D;

            if (Texture.IsCubeTexture)
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, Id);

                for (int Face = 0; Face < 6; Face++)
                {
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + Face,
                        0,
                        PixelInternalFormat.Rgba,
                        (int)Texture.Width,
                        (int)Texture.Height,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        Texture.ToRGBA(Face));
                }
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, Id);

                var maxMips = CalculateMipCount(Texture.Width, Texture.Height, Texture.Format) + 1;
                var mipCount = Texture.MipmapSize;
                if (Texture.MipmapSize > maxMips)
                    mipCount = 1;

                mipCount = 1;

                //Load mipmaps
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mipCount - 1);
                //Force load a filter for mipmaps. Materials will later reconfigure with the right filter to use
                if (mipCount > 1)
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);

                for (int i = 0; i < mipCount; i++)
                {
                    uint mipwidth = (uint)Math.Max(1, Texture.Width >> i);
                    uint mipheight = (uint)Math.Max(1, Texture.Height >> i);

                    GL.TexImage2D(TextureTarget.Texture2D,
                        i,
                        PixelInternalFormat.Rgba,
                        (int)mipwidth,
                        (int)mipheight,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        Texture.ToMipRGBA(i));
                }
            }
        }

        public void Bind(int Unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + Unit);

            GL.BindTexture(Target, Id);
        }

        private bool Disposed;

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposed)
            {
                GL.DeleteTexture(Id);

                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private uint CalculateMipCount(int Width, int Height, PICA.Commands.PICATextureFormat Format)
        {
            int MipmapNum = 0;
            int num = Math.Max(Height, Width);

            uint width = (uint)Width;
            uint height = (uint)Height;

            uint Pow2RoundDown(uint Value)
            {
                return IsPow2(Value) ? Value : Pow2RoundUp(Value) >> 1;
            }

            bool IsPow2(uint Value)
            {
                return Value != 0 && (Value & (Value - 1)) == 0;
            }

            uint Pow2RoundUp(uint Value)
            {
                Value--;

                Value |= (Value >> 1);
                Value |= (Value >> 2);
                Value |= (Value >> 4);
                Value |= (Value >> 8);
                Value |= (Value >> 16);

                return ++Value;
            }

            while (true)
            {
                num >>= 1;

                width = width / 2;
                height = height / 2;

                width = Pow2RoundDown(width);
                height = Pow2RoundDown(height);

                Console.WriteLine($"{MipmapNum} wh {width} X {height}");

                if (Format == PICATextureFormat.ETC1)
                {
                    if (width < 16 || height < 16)
                        break;
                }
                else if (width < 8 || height < 8)
                    break;

                if (num > 0)
                    ++MipmapNum;
                else
                    break;
            }
            return (uint)MipmapNum;
        }
    }
}
