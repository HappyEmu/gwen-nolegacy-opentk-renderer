using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gwen.Renderer
{
    public class GLShader : IDisposable
    {
        public int Program { get; set; }
        public int VertexShader { get; set; }
        public int FragmentShader { get; set; }

        private UniformDictionary _uniforms;
        public UniformDictionary Uniforms { get => _uniforms; set { return; } }

        public GLShader()
        {
            Program = 0;
            VertexShader = 0;
            FragmentShader = 0;
        }

        public void Load(string shaderName)
        {
            Load(shaderName, shaderName);
        }

        public void Apply()
        {
            GL.UseProgram(Program);
        }

        public void Load(string vertexShaderName, string fragmentShaderName)
        {
            string vSource = vShaderSource;
            string fSource = fShaderSource;

            int vShader = GL.CreateShader(ShaderType.VertexShader);
            int fShader = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vShader, vSource);
            GL.ShaderSource(fShader, fSource);
            // Compile shaders
            GL.CompileShader(vShader);
            GL.CompileShader(fShader);
            Debug.WriteLine(GL.GetShaderInfoLog(vShader));
            Debug.WriteLine(GL.GetShaderInfoLog(fShader));

            int program = GL.CreateProgram();
            // Link and attach shaders to program
            GL.AttachShader(program, vShader);
            GL.AttachShader(program, fShader);

            GL.LinkProgram(program);
            Debug.WriteLine(GL.GetProgramInfoLog(program));

            Program = program;
            VertexShader = vShader;
            FragmentShader = fShader;
            _uniforms = new UniformDictionary(Program);
        }

        public class UniformDictionary
        {
            private readonly Dictionary<string, int> _data;
            private readonly int _program;

            public UniformDictionary(int program)
            {
                _data = new Dictionary<string, int>();
                _program = program;
            }

            public int this[string key]
            {
                get
                {
                    if (!_data.ContainsKey(key))
                    {
                        int uniformLocation = GL.GetUniformLocation(_program, key);
                        _data.Add(key, uniformLocation);
                    }

                    int loc = -1;
                    _data.TryGetValue(key, out loc);

                    return loc;
                }
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(Program);
        }

        private const string vShaderSource = @"
#version 420

layout(location=0) in vec2 in_screen_coords;
layout(location=1) in vec2 in_uv;
layout(location=2) in vec4 in_color;

out vec2 frag_uv;
out vec4 frag_color;

uniform vec2 uScreenSize = vec2(1280, 720);

void main(void)
{
	frag_uv = in_uv;
	frag_color = in_color;

	vec2 ndc_position = 2.0 * (in_screen_coords / uScreenSize) - 1.0;
	ndc_position.y *= -1.0;

	gl_Position = vec4(ndc_position, 0.0, 1);
}";


        private const string fShaderSource = @"
#version 420

in vec2 frag_uv;
in vec4 frag_color;

layout(binding=0) uniform sampler2D tex;

out vec4 out_frag_color;

uniform float uUseTexture = 0.0;

void main(void)
{
	vec4 texColor = texture(tex, frag_uv);
	out_frag_color = texColor * uUseTexture + frag_color * (1.0 - uUseTexture);
}";

    }
}

