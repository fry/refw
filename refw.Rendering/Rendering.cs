using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace refw.Rendering
{
    public class RenderingBase {
        public bool IsEnabled = false;
        public List<CubeEntry> Cubes = new List<CubeEntry>();
        public List<LineEntry> Lines = new List<LineEntry>();
        public List<LabelEntry> Labels = new List<LabelEntry>();
        public List<RectangleEntry> Rectangles = new List<RectangleEntry>();
        public List<CircleEntry> Circles = new List<CircleEntry>();

        public IEnumerable<RenderEntry> RenderObjects {
            get {
                return Cubes.Concat<RenderEntry>(Lines).Concat(Labels).Concat(Rectangles).Concat(Circles);
            }
        }

        private StateBlock state_old;
        public Device Device { private set; get; }
        private IntPtr device_ptr;
        private VertexDeclaration vert_decl_nocolor;
        protected refw.D3D.EndSceneDetour detour;

        private VertexBuffer vb_cube;
        private Font font;

        public abstract class RenderEntry {
            public string Tag;
        }

        public class CubeEntry : RenderEntry {
            public Vector3 Position;
            public int Color = System.Drawing.Color.White.ToArgb();
            public Vector3 Scale = new Vector3(1, 1, 1);
        }

        public class LineEntry : RenderEntry {
            public Vector3 Start;
            public Vector3 End;
            public int Color = System.Drawing.Color.White.ToArgb();
        }

        public class LabelEntry : RenderEntry {
            public string Text;
            public Vector3 Position;
            public int Color = System.Drawing.Color.White.ToArgb();
        }

        public class RectangleEntry : RenderEntry {
            public Vector3 Start = Vector3.Zero;
            public Vector3 End = new Vector3(1, 1, 1);
            public int Color = System.Drawing.Color.White.ToArgb();
            public Vector3 Position = Vector3.Zero;
            public Quaternion Rotation = Quaternion.Identity;
            public Vector3 Scale = new Vector3(1, 1, 1);
            public int? FillColor = null;
        }

        public class CircleEntry : RenderEntry {
            public Vector3 Position;
            public Quaternion Rotation = Quaternion.Identity;
            public int Steps = 10;
            public float Radius = 1.0f;
            public Color4 Color = new Color4(System.Drawing.Color.White);
            public Color4? FillColor = null;
        }

        protected virtual bool IsReady() {
            return true;
        }

        protected virtual bool GetCameraMatrices(out Matrix projection, out Matrix view) {
            projection = Matrix.Identity;
            view = Matrix.Identity;

            return false;
        }

        public void Setup(refw.D3D.EndSceneDetour detour) {
            this.detour = detour;

            detour.OnFrame += new Action<TimeSpan>(DirectX_OnFrame);
            detour.OnCreateDevice += new Action(DirectX_OnCreateDevice);
            detour.OnResetDevice += new Action<IntPtr>(DirectX_OnResetDevice);
        }

        void DirectX_OnResetDevice(IntPtr obj) {
            DisposeReferences();
        }

        void DisposeReferences() {
            if (Device != null) {
                Device.Dispose();
                state_old.Dispose();
                vb_cube.Dispose();
                vert_decl_nocolor.Dispose();
                font.Dispose();
                Device = null;
                vert_decl_nocolor = null;
                vb_cube = null;
                state_old = null;
                font = null;
            }
        }

        void DirectX_OnCreateDevice() {
            DisposeReferences();

            device_ptr = detour.Direct3DDevice9;
            Device = Device.FromPointer(device_ptr);

            state_old = new StateBlock(Device, StateBlockType.All);

            vb_cube = new VertexBuffer(Device, Marshal.SizeOf(typeof(Vector3)) * 3 * 12, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
            vb_cube.Lock(0, 0, LockFlags.None).WriteRange(new[] {
						new Vector3(-1f, -1f, -1f), // front
						new Vector3(+1f, -1f, -1f),
						new Vector3(+1f, -1f, +1f),

						new Vector3(+1f, -1f, +1f),
						new Vector3(-1f, -1f, +1f),
						new Vector3(-1f, -1f, -1f),

						new Vector3(-1f, +1f, -1f), // back
						new Vector3(+1f, +1f, -1f),
						new Vector3(+1f, +1f, +1f),

						new Vector3(+1f, +1f, +1f),
						new Vector3(-1f, +1f, +1f),
						new Vector3(-1f, +1f, -1f),

						new Vector3(-1f, -1f, -1f), // left
						new Vector3(-1f, -1f, +1f),
						new Vector3(-1f, +1f, +1f),
						
						new Vector3(-1f, +1f, +1f),
						new Vector3(-1f, +1f, -1f),
						new Vector3(-1f, -1f, -1f),

						new Vector3(+1f, -1f, -1f), // right
						new Vector3(+1f, -1f, +1f),
						new Vector3(+1f, +1f, +1f),

						new Vector3(+1f, +1f, +1f),
						new Vector3(+1f, +1f, -1f),
						new Vector3(+1f, -1f, -1f),

						new Vector3(-1f, -1f, -1f), // top
						new Vector3(+1f, -1f, -1f),
						new Vector3(+1f, +1f, -1f),
						
						new Vector3(+1f, +1f, -1f),
						new Vector3(-1f, +1f, -1f),
						new Vector3(-1f, -1f, -1f),

						new Vector3(-1f, -1f, +1f), // bottom
						new Vector3(+1f, -1f, +1f),
						new Vector3(+1f, +1f, +1f),
						
						new Vector3(+1f, +1f, +1f),
						new Vector3(-1f, +1f, +1f),
						new Vector3(-1f, -1f, +1f)
					});
            vb_cube.Unlock();

            var vertexElems = new[] {
        		new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        		//new VertexElement(0, 16, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
				VertexElement.VertexDeclarationEnd
        	};

            vert_decl_nocolor = new VertexDeclaration(Device, vertexElems);

            font = new Font(Device, new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 14));
        }

        void DirectX_OnFrame(TimeSpan delta) {
            if (!IsEnabled || !IsReady())
                return;

            // Attempt to recreate the device if we removed it; WoW does weird device recreating after resetting it on a lost device
            if (Device == null || Device.Disposed) {
                DirectX_OnCreateDevice();
                return;
            }

            // If the device is currently in the lost state don't continue
            var cooplevel = Device.TestCooperativeLevel();
            if (cooplevel.IsFailure)
                return;

            // Store WoW's render states
            state_old.Capture();

            var device = Device;

            // Clean up render states
            device.PixelShader = null;
            device.VertexShader = null;
            device.SetTexture(0, null);
            device.SetRenderState(RenderState.Lighting, true);
            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
            device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);

            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.SetRenderState(RenderState.ColorWriteEnable, ColorWriteEnable.All);
            device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);

            Matrix mat_view, mat_proj;

            if (GetCameraMatrices(out mat_proj, out mat_view)) {
                device.SetTransform(TransformState.Projection, mat_proj);
                device.SetTransform(TransformState.View, mat_view);

                device.VertexDeclaration = vert_decl_nocolor;

                // Draw cubes
                device.SetStreamSource(0, vb_cube, 0, 12);
                foreach (var cube in Cubes) {
                    device.SetTransform(TransformState.World, Matrix.Scaling(cube.Scale) * Matrix.Translation(cube.Position));
                    device.SetRenderState(RenderState.Ambient, cube.Color);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
                }

                // Draw lines
                device.SetTransform(TransformState.World, Matrix.Identity);
                Vector3[] line_data = new Vector3[2];
                foreach (var line in Lines) {
                    line_data[0] = line.Start;
                    line_data[1] = line.End;

                    device.SetRenderState(RenderState.Ambient, line.Color);
                    device.DrawUserPrimitives(PrimitiveType.LineList, 1, line_data);
                }

                // Draw rectangles
                Vector3[] rectangle_data = new Vector3[5];
                foreach (var rect in Rectangles) {
                    rectangle_data[0] = rect.Start;
                    rectangle_data[1] = rect.Start + Vector3.UnitX * (rect.End.X - rect.Start.X);
                    rectangle_data[2] = rect.Start + Vector3.UnitX * (rect.End.X - rect.Start.X) + Vector3.UnitY * (rect.End.Y - rect.Start.Y);
                    rectangle_data[3] = rect.Start + Vector3.UnitY * (rect.End.Y - rect.Start.Y);
                    rectangle_data[4] = rect.Start;

                    var world_matrix = Matrix.RotationQuaternion(rect.Rotation) * Matrix.Scaling(rect.Scale) * Matrix.Translation(rect.Position);
                    device.SetTransform(TransformState.World, world_matrix);

                    if (rect.FillColor.HasValue) {

                        device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                        device.SetRenderState(RenderState.Ambient, System.Drawing.Color.White.ToArgb());
                        device.DrawUserPrimitives(PrimitiveType.TriangleFan, 4, rectangle_data);
                        device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    }

                    device.SetRenderState(RenderState.Ambient, rect.Color);
                    device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, rectangle_data);
                }

                // Draw circles
                /*var mat = new Material();
                mat.Ambient = new Color4(System.Drawing.Color.White);
                device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
                device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Material);*/

                foreach (var circle in Circles) {
                    var circle_points = new Vector3[circle.Steps + 2];
                    // A point in the center of the circle, to use when the triangle is filled
                    circle_points[0] = Vector3.Zero;
                    for (int i = 0; i < circle.Steps; i++) {
                        var angle = ((float)i / circle.Steps) * Math.PI * 2;
                        circle_points[i + 1] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
                    }
                    circle_points[circle_points.Length - 1] = circle_points[1];

                    var world_matrix = Matrix.RotationQuaternion(circle.Rotation) * Matrix.Scaling(new Vector3(circle.Radius, circle.Radius, 1)) * Matrix.Translation(circle.Position);
                    device.SetTransform(TransformState.World, world_matrix);

                    /*if (circle.FillColor.HasValue) {
                        //mat.Diffuse = new Color4(0.5f, 1.0f, 0.0f, 0.0f); //circle.FillColor.Value;
                        mat.Ambient = circle.FillColor.Value;
                        device.Material = mat;
                        device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                        device.DrawUserPrimitives(PrimitiveType.TriangleFan, circle.Steps, circle_points);
                        device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    }*/

                    //mat.Ambient = circle.Color;
                    //device.Material = mat;
                    device.SetRenderState(RenderState.Ambient, circle.Color.ToArgb());
                    device.DrawUserPrimitives(PrimitiveType.LineStrip, circle_points.Length - 2, circle_points.Skip(1).ToArray());
                }

                // Draw labels
                var viewport = Device.Viewport;
                var viewproj = Matrix.Multiply(mat_view, mat_proj);
                foreach (var label in Labels) {
                    var transformed = Vector3.Project(label.Position, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinZ, viewport.MaxZ, viewproj);
                    if (transformed.Z <= 1.0f)
                        font.DrawString(null, label.Text, (int)transformed.X, (int)transformed.Y, new Color4(label.Color));
                }

                //device.Material = new Material();
            }

            // Restore render states
            state_old.Apply();
        }
    }
}
