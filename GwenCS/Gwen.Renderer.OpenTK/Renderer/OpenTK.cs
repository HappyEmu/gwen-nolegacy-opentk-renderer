﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Gwen.Renderer
{
    public class OpenTK : Base
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vertex
        {
            public float x, y;
            public float u, v;
            public float r, g, b, a;
        }

        private const int MaxVerts = 4096;
        private Color m_Color;
        private int m_VertNum;
        private readonly Vertex[] m_Vertices;
        private readonly int m_VertexSize;
        private int m_TotalVertNum;

        private readonly Dictionary<Tuple<String, Font>, TextRenderer> m_StringCache;
        private readonly Graphics m_Graphics; // only used for text measurement
        private int m_DrawCallCount;
        private bool m_ClipEnabled;
        private bool m_TextureEnabled;
        private static int m_LastTextureID;

        private bool m_WasBlendEnabled, m_WasDepthTestEnabled;
        private int m_PrevBlendSrc, m_PrevBlendDst, m_PrevAlphaFunc;
        private float m_PrevAlphaRef;
        private bool m_RestoreRenderState;
        private readonly NativeWindow _nativeWindow;
        private StringFormat m_StringFormat;

        private int vbo, vao;
        private readonly GLShader guiShader;

        public OpenTK(NativeWindow nativeWindow, bool restoreRenderState = true)
            : base()
        {
            m_Vertices = new Vertex[MaxVerts];
            m_VertexSize = Marshal.SizeOf(m_Vertices[0]);
            m_StringCache = new Dictionary<Tuple<String, Font>, TextRenderer>();
            m_Graphics = Graphics.FromImage(new Bitmap(1024, 1024, PixelFormat.Format32bppArgb));
            m_StringFormat = new StringFormat(StringFormat.GenericTypographic);
            m_StringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            m_RestoreRenderState = restoreRenderState;
            _nativeWindow = nativeWindow ?? throw new ArgumentNullException(nameof(nativeWindow));
            CreateBuffers();

            guiShader = new GLShader();
            guiShader.Load("gui");
        }

        private void CreateBuffers()
        {
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_VertexSize * MaxVerts), IntPtr.Zero, BufferUsageHint.StreamDraw); // Allocate

            // Vertex positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, m_VertexSize, 0);

            // Tex coords
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, m_VertexSize, 2 * sizeof(float));

            // Colors
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, m_VertexSize, 2 * (sizeof(float) + sizeof(float)));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public override void Dispose()
        {
            FlushTextCache();
            base.Dispose();
        }

        public override void Begin()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.UseProgram(guiShader.Program);

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            if (m_RestoreRenderState)
            {
                // Get previous parameter values before changing them.
                GL.GetInteger(GetPName.BlendSrc, out m_PrevBlendSrc);
                GL.GetInteger(GetPName.BlendDst, out m_PrevBlendDst);
                GL.GetInteger(GetPName.AlphaTestFuncQcom, out m_PrevAlphaFunc);
                GL.GetFloat(GetPName.AlphaTestRefQcom, out m_PrevAlphaRef);

                m_WasBlendEnabled = GL.IsEnabled(EnableCap.Blend);
                m_WasDepthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            }

            // Set default values and enable/disable caps.
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);

            m_VertNum = 0;
            m_TotalVertNum = 0;
            m_DrawCallCount = 0;
            m_ClipEnabled = false;
            m_TextureEnabled = false;
            m_LastTextureID = -1;
        }

        public override void End()
        {
            Flush();
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            if (m_RestoreRenderState)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                m_LastTextureID = 0;

                // Restore the previous parameter values.
                GL.BlendFunc((BlendingFactor)m_PrevBlendSrc, (BlendingFactor)m_PrevBlendDst);

                if (!m_WasBlendEnabled)
                {
                    GL.Disable(EnableCap.Blend);
                }

                if (m_WasDepthTestEnabled)
                {
                    GL.Enable(EnableCap.DepthTest);
                }
            }
        }

        /// <summary>
        /// Returns number of cached strings in the text cache.
        /// </summary>
        public int TextCacheSize => m_StringCache.Count;

        public int DrawCallCount => m_DrawCallCount;

        public int VertexCount => m_TotalVertNum;
        /// <summary>
        /// Clears the text rendering cache. Make sure to call this if cached strings size becomes too big (check TextCacheSize).
        /// </summary>
        public void FlushTextCache()
        {
            // todo: some auto-expiring cache? based on number of elements or age
            foreach (var textRenderer in m_StringCache.Values)
            {
                textRenderer.Dispose();
            }
            m_StringCache.Clear();
        }

        private unsafe void Flush()
        {
            if (m_VertNum == 0)
            {
                return;
            }

            //GL.InvalidateBufferData (vbo);
            GL.BufferSubData<Vertex>(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(m_VertNum * m_VertexSize), m_Vertices);

            GL.Uniform1(guiShader.Uniforms["uUseTexture"], m_TextureEnabled ? 1.0f : 0.0f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, m_VertNum);

            m_DrawCallCount++;
            m_TotalVertNum += m_VertNum;
            m_VertNum = 0;
        }

        public override void DrawFilledRect(Rectangle rect)
        {
            if (m_TextureEnabled)
            {
                Flush();
                m_TextureEnabled = false;
            }

            rect = Translate(rect);

            DrawRect(rect);
        }

        public override Color DrawColor
        {
            get => m_Color;
            set => m_Color = value;
        }

        public override void StartClip()
        {
            m_ClipEnabled = true;
        }

        public override void EndClip()
        {
            m_ClipEnabled = false;
        }

        public override void DrawTexturedRect(Texture t, Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
        {
            // Missing image, not loaded properly?
            if (null == t.RendererData)
            {
                DrawMissingImage(rect);
                return;
            }

            int tex = (int)t.RendererData;
            rect = Translate(rect);

            bool differentTexture = (tex != m_LastTextureID);
            if (!m_TextureEnabled || differentTexture)
            {
                Flush();
            }

            if (!m_TextureEnabled)
            {
                m_TextureEnabled = true;
            }

            if (differentTexture)
            {
                GL.BindTexture(TextureTarget.Texture2D, tex);
                m_LastTextureID = tex;
            }

            DrawRect(rect, u1, v1, u2, v2);
        }

        private void DrawRect(Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
        {
            if (m_VertNum + 4 >= MaxVerts)
            {
                Flush();
            }

            if (m_ClipEnabled)
            {
                // cpu scissors test

                if (rect.Y < ClipRegion.Y)
                {
                    int oldHeight = rect.Height;
                    int delta = ClipRegion.Y - rect.Y;
                    rect.Y = ClipRegion.Y;
                    rect.Height -= delta;

                    if (rect.Height <= 0)
                    {
                        return;
                    }

                    float dv = delta / (float)oldHeight;

                    v1 += dv * (v2 - v1);
                }

                if ((rect.Y + rect.Height) > (ClipRegion.Y + ClipRegion.Height))
                {
                    int oldHeight = rect.Height;
                    int delta = (rect.Y + rect.Height) - (ClipRegion.Y + ClipRegion.Height);

                    rect.Height -= delta;

                    if (rect.Height <= 0)
                    {
                        return;
                    }

                    float dv = delta / (float)oldHeight;

                    v2 -= dv * (v2 - v1);
                }

                if (rect.X < ClipRegion.X)
                {
                    int oldWidth = rect.Width;
                    int delta = ClipRegion.X - rect.X;
                    rect.X = ClipRegion.X;
                    rect.Width -= delta;

                    if (rect.Width <= 0)
                    {
                        return;
                    }

                    float du = delta / (float)oldWidth;

                    u1 += du * (u2 - u1);
                }

                if ((rect.X + rect.Width) > (ClipRegion.X + ClipRegion.Width))
                {
                    int oldWidth = rect.Width;
                    int delta = (rect.X + rect.Width) - (ClipRegion.X + ClipRegion.Width);

                    rect.Width -= delta;

                    if (rect.Width <= 0)
                    {
                        return;
                    }

                    float du = delta / (float)oldWidth;

                    u2 -= du * (u2 - u1);
                }
            }

            float cR = m_Color.R / 255f;
            float cG = m_Color.G / 255f;
            float cB = m_Color.B / 255f;
            float cA = m_Color.A / 255f;

            int vertexIndex = m_VertNum;
            m_Vertices[vertexIndex].x = (short)rect.X;
            m_Vertices[vertexIndex].y = (short)rect.Y;
            m_Vertices[vertexIndex].u = u1;
            m_Vertices[vertexIndex].v = v1;
            m_Vertices[vertexIndex].r = cR;
            m_Vertices[vertexIndex].g = cG;
            m_Vertices[vertexIndex].b = cB;
            m_Vertices[vertexIndex].a = cA;

            vertexIndex++;
            m_Vertices[vertexIndex].x = (short)(rect.X + rect.Width);
            m_Vertices[vertexIndex].y = (short)rect.Y;
            m_Vertices[vertexIndex].u = u2;
            m_Vertices[vertexIndex].v = v1;
            m_Vertices[vertexIndex].r = cR;
            m_Vertices[vertexIndex].g = cG;
            m_Vertices[vertexIndex].b = cB;
            m_Vertices[vertexIndex].a = cA;

            vertexIndex++;
            m_Vertices[vertexIndex].x = (short)(rect.X + rect.Width);
            m_Vertices[vertexIndex].y = (short)(rect.Y + rect.Height);
            m_Vertices[vertexIndex].u = u2;
            m_Vertices[vertexIndex].v = v2;
            m_Vertices[vertexIndex].r = cR;
            m_Vertices[vertexIndex].g = cG;
            m_Vertices[vertexIndex].b = cB;
            m_Vertices[vertexIndex].a = cA;

            vertexIndex++;
            m_Vertices[vertexIndex].x = (short)rect.X;
            m_Vertices[vertexIndex].y = (short)rect.Y;
            m_Vertices[vertexIndex].u = u1;
            m_Vertices[vertexIndex].v = v1;
            m_Vertices[vertexIndex].r = cR;
            m_Vertices[vertexIndex].g = cG;
            m_Vertices[vertexIndex].b = cB;
            m_Vertices[vertexIndex].a = cA;

            vertexIndex++;
            m_Vertices[vertexIndex].x = (short)(rect.X + rect.Width);
            m_Vertices[vertexIndex].y = (short)(rect.Y + rect.Height);
            m_Vertices[vertexIndex].u = u2;
            m_Vertices[vertexIndex].v = v2;
            m_Vertices[vertexIndex].r = cR;
            m_Vertices[vertexIndex].g = cG;
            m_Vertices[vertexIndex].b = cB;
            m_Vertices[vertexIndex].a = cA;

            vertexIndex++;
            m_Vertices[vertexIndex].x = (short)rect.X;
            m_Vertices[vertexIndex].y = (short)(rect.Y + rect.Height);
            m_Vertices[vertexIndex].u = u1;
            m_Vertices[vertexIndex].v = v2;
            m_Vertices[vertexIndex].r = cR;
            m_Vertices[vertexIndex].g = cG;
            m_Vertices[vertexIndex].b = cB;
            m_Vertices[vertexIndex].a = cA;

            m_VertNum += 6;
        }

        public override bool LoadFont(Font font)
        {
            Debug.Print(String.Format("LoadFont {0}", font.FaceName));
            font.RealSize = font.Size * Scale;
            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;

            if (sysFont != null)
            {
                sysFont.Dispose();
            }

            // apaprently this can't fail @_@
            // "If you attempt to use a font that is not supported, or the font is not installed on the machine that is running the application, the Microsoft Sans Serif font will be substituted."
            sysFont = new System.Drawing.Font(font.FaceName, font.Size);
            font.RendererData = sysFont;
            return true;
        }

        public override void FreeFont(Font font)
        {
            Debug.Print(String.Format("FreeFont {0}", font.FaceName));
            if (font.RendererData == null)
            {
                return;
            }

            Debug.Print(String.Format("FreeFont {0} - actual free", font.FaceName));
            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;
            if (sysFont == null)
            {
                throw new InvalidOperationException("Freeing empty font");
            }

            sysFont.Dispose();
            font.RendererData = null;
        }

        public override Point MeasureText(Font font, string text)
        {
            //Debug.Print(String.Format("MeasureText '{0}'", text));
            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;

            if (sysFont == null || Math.Abs(font.RealSize - font.Size * Scale) > 2)
            {
                FreeFont(font);
                LoadFont(font);
                sysFont = font.RendererData as System.Drawing.Font;
            }

            var key = new Tuple<String, Font>(text, font);

            if (m_StringCache.ContainsKey(key))
            {
                var tex = m_StringCache[key].Texture;
                return new Point(tex.Width, tex.Height);
            }

            SizeF TabSize = m_Graphics.MeasureString("....", sysFont); //Spaces are not being picked up, let's just use .'s.
            m_StringFormat.SetTabStops(0f, new float[] { TabSize.Width });

            SizeF size = m_Graphics.MeasureString(text, sysFont, Point.Empty, m_StringFormat);

            return new Point((int)Math.Round(size.Width), (int)Math.Round(size.Height));
        }

        public override void RenderText(Font font, Point position, string text)
        {
            //Debug.Print(String.Format("RenderText {0}", font.FaceName));

            // The DrawString(...) below will bind a new texture
            // so make sure everything is rendered!
            Flush();

            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;

            if (sysFont == null || Math.Abs(font.RealSize - font.Size * Scale) > 2)
            {
                FreeFont(font);
                LoadFont(font);
                sysFont = font.RendererData as System.Drawing.Font;
            }

            var key = new Tuple<String, Font>(text, font);

            if (!m_StringCache.ContainsKey(key))
            {
                // not cached - create text renderer
                Debug.Print(String.Format("RenderText: caching \"{0}\", {1}", text, font.FaceName));

                Point size = MeasureText(font, text);
                TextRenderer tr = new TextRenderer(size.X, size.Y, this);
                Brush b = new SolidBrush(DrawColor);
                tr.DrawString(text, sysFont, b, Point.Empty, m_StringFormat); // renders string on the texture
                b.Dispose();

                DrawTexturedRect(tr.Texture, new Rectangle(position.X, position.Y, tr.Texture.Width, tr.Texture.Height));

                m_StringCache[key] = tr;
            }
            else
            {
                TextRenderer tr = m_StringCache[key];
                DrawTexturedRect(tr.Texture, new Rectangle(position.X, position.Y, tr.Texture.Width, tr.Texture.Height));
            }
        }

        internal static void LoadTextureInternal(Texture t, Bitmap bmp)
        {
            // todo: convert to proper format
            PixelFormat lock_format = PixelFormat.Undefined;
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    lock_format = PixelFormat.Format32bppArgb;
                    break;
                case PixelFormat.Format24bppRgb:
                    lock_format = PixelFormat.Format32bppArgb;
                    break;
                default:
                    t.Failed = true;
                    return;
            }


            // Create the opengl texture
            GL.GenTextures(1, out int glTex);

            GL.BindTexture(TextureTarget.Texture2D, glTex);
            m_LastTextureID = glTex;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Sort out our GWEN texture
            t.RendererData = glTex;
            t.Width = bmp.Width;
            t.Height = bmp.Height;

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, lock_format);

            switch (lock_format)
            {
                case PixelFormat.Format32bppArgb:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, global::OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    break;
                default:
                    // invalid
                    break;
            }

            bmp.UnlockBits(data);
        }

        public override void LoadTexture(Texture t)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(t.Name);
            }
            catch (Exception)
            {
                t.Failed = true;
                return;
            }

            LoadTextureInternal(t, bmp);
            bmp.Dispose();
        }

        public override void LoadTextureStream(Texture t, System.IO.Stream data)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(data);
            }
            catch (Exception)
            {
                t.Failed = true;
                return;
            }

            LoadTextureInternal(t, bmp);
            bmp.Dispose();
        }

        public override void LoadTextureRaw(Texture t, byte[] pixelData)
        {
            Bitmap bmp;
            try
            {
                unsafe
                {
                    fixed (byte* ptr = &pixelData[0])
                    {
                        bmp = new Bitmap(t.Width, t.Height, 4 * t.Width, PixelFormat.Format32bppArgb, (IntPtr)ptr);
                    }
                }
            }
            catch (Exception)
            {
                t.Failed = true;
                return;
            }


            // Create the opengl texture
            GL.GenTextures(1, out int glTex);

            GL.BindTexture(TextureTarget.Texture2D, glTex);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Sort out our GWEN texture
            t.RendererData = glTex;

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, global::OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            bmp.Dispose();

            //[halfofastaple] Must rebind previous texture, to ensure creating a texture doesn't mess with the render flow.
            // Setting m_LastTextureID isn't working, for some reason (even if you always rebind the texture,
            // even if the previous one was the same), we are probably making draw calls where we shouldn't be?
            // Eventually the bug needs to be fixed (color picker in a window causes graphical errors), but for now,
            // this is fine.
            GL.BindTexture(TextureTarget.Texture2D, m_LastTextureID);
        }

        public override void FreeTexture(Texture t)
        {
            if (t.RendererData == null)
            {
                return;
            }

            int tex = (int)t.RendererData;
            if (tex == 0)
            {
                return;
            }

            GL.DeleteTextures(1, ref tex);
            t.RendererData = null;
        }

        public override unsafe Color PixelColor(Texture texture, uint x, uint y, Color defaultColor)
        {
            if (texture.RendererData == null)
            {
                return defaultColor;
            }

            int tex = (int)texture.RendererData;
            if (tex == 0)
            {
                return defaultColor;
            }

            Color pixel;

            GL.BindTexture(TextureTarget.Texture2D, tex);
            m_LastTextureID = tex;

            long offset = 4 * (x + y * texture.Width);
            byte[] data = new byte[4 * texture.Width * texture.Height];
            fixed (byte* ptr = &data[0])
            {
                GL.GetTexImage(TextureTarget.Texture2D, 0, global::OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)ptr);
                pixel = Color.FromArgb(data[offset + 3], data[offset + 0], data[offset + 1], data[offset + 2]);
            }

            //[???] Retrieving the entire texture for a single pixel read
            // is kind of a waste - maybe cache this pointer in the texture
            // data and then release later on? It's never called during runtime
            // - only during initialization.

            //[halfofastaple] RE: It's not really a waste if it's only done once on load.
            // Despite, it's worth looking into, just in case a user
            // wishes to hack their code together and use this function at
            // runtime
            return pixel;
        }

        public void Resize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            GL.UseProgram(guiShader.Program);
            GL.Uniform2(guiShader.Uniforms["uScreenSize"], (float)width, (float)height);
        }

        public override void SetCursor(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Cross:
                    _nativeWindow.Cursor = MouseCursor.Crosshair;
                    break;
                case CursorType.Hand:
                    _nativeWindow.Cursor = MouseCursor.Hand;
                    break;
                case CursorType.IBeam:
                    _nativeWindow.Cursor = MouseCursor.IBeam;
                    break;
                case CursorType.None:
                    _nativeWindow.Cursor = MouseCursor.Empty;
                    break;
                case CursorType.SizeAll:
                case CursorType.SizeNESW:
                case CursorType.SizeNS:
                    _nativeWindow.Cursor = MouseCursor.VResize;
                    break;
                case CursorType.SizeNWSE:
                case CursorType.SizeWE:
                    _nativeWindow.Cursor = MouseCursor.HResize;
                    break;
                default:
                    _nativeWindow.Cursor = MouseCursor.Default;
                    break;
            }
        }
    }
}
