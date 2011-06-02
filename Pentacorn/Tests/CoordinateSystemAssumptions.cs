using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Tasks
{
    class CoordinateSystemAssumptions
    {
        public static void Run()
        {
            var left = -1;
            var right = 1;
            var bottom = -1;
            var top = 1;
            
            var near = 5.0f;
            var far = near + 30.0f;
            var xdiv = (right - left) / (far - near);
            var ydiv = (top - bottom) / (far - near);

            var world = Matrix.Identity;
            var view = Matrix.Identity;
            var proj = Matrix.CreatePerspectiveOffCenter(left, right, bottom, top, near, far);

            var width = 2;
            var height = 2;

            var port = new Viewport(left, bottom, width, height);

            // From MSDN et. al.
            //
            // projection space refers to the space after applying projection transformation from 
            // view space. After the projection transformation, visible content has x and y coordinates
            // ranging from -1 to 1, and z coordinates ranging from 0 to 1.
            //
            // Positive Z comes out of the screen, so negative near and far is into the screen.
            //
            // The viewport then mirrors Y so it goes down in screen space.
            Doing(v => port.Project(v, proj, view, world)).With(-1, -1, -near).ShouldBe(-1, 1, 0)
                                                          .With( 1, -1, -near).ShouldBe(1, 1, 0)
                                                          .With(-1, 1, -near).ShouldBe(-1, -1, 0)
                                                          .With(1, 1, -near).ShouldBe(1, -1, 0)
                                                          .With( 0, 0, -near).ShouldBe(0, 0, 0)
                                                          .With(0, 0, -far).ShouldBe(0, 0, 1);
            
            // Test project is inverse of unproject
            Doing(v => port.Unproject(v, proj, view, world)).With(-1, -1, 1).ShouldBe(-1 / near * far, 1 / near * far, -far);
            Doing(v => port.Project(v, proj, view, world)).With(-1 / near * far, 1 / near * far, -far).ShouldBe(-1, -1, 1);

            Doing(v => port.Project(v, proj, view, world)).With(-1 / near * far, -1 / near * far, -far).ShouldBe(-1, 1, 1)
                                                          .With(1 / near * far, 1 / near * far, -far).ShouldBe(1, -1, 1)
                                                          .With(1 / near * far, -1 / near * far, -far).ShouldBe(1, 1, 1);

            // Similar verification but now with values that are typical of usage.
            top = 10;
            left = -5;
            bottom = 0;
            right = 5;

            near = 1.0f;
            far = near + 100.0f;
            
            world = Matrix.Identity;
            view = Matrix.Identity;
            proj = Matrix.CreatePerspectiveOffCenter(left, right, bottom, top, near, far);

            width = 640;
            height = 480;
            port = new Viewport(0, 0, width, height);

            Doing(v => port.Project(v, proj, view, world))

                // Corners of the screen at near plane.
                .With(left, top, -near).ShouldBe(0, 0, 0)
                .With(right, top, -near).ShouldBe(width, 0, 0)
                .With(left, bottom, -near).ShouldBe(0, height, 0)
                .With(right, bottom, -near).ShouldBe(width, height, 0)

                // Center of the screen.
                .With(0, 0, -near).ShouldBe(width / 2, height, 0)
                .With(0, 0, -far).ShouldBe(width / 2, height, 1);

            Doing(v => port.Unproject(v, proj, view, world)).With(0, 0, 0).ShouldBe(left, top, -near)
                                                            .With(width, 0, 0).ShouldBe(right, top, -near)
                                                            .With(0, height, 0).ShouldBe(left, bottom, -near)
                                                            .With(width, height, 0).ShouldBe(right, bottom, -near);
        }
        
        private static void Becomes(Vector3 v3, Func<Vector3, Vector3> fun, Vector3 expected)
        {
            Console.WriteLine("{0} -> {1}", v3, fun(v3));
        }

        private static Operation Doing(Func<Vector3, Vector3> op)
        {
            return new Operation(op);
        }

        private class Result
        {
            public Result(Vector3 inp, Vector3 r, Operation op) { Inp = inp; Res = r; Op = op; }
            public const float Epsilon = 0.02f;

            public Operation ShouldBe(float x, float y, float z, float epsilon = Epsilon) { return ShouldBe(new Vector3(x, y, z), epsilon); }
            public Operation ShouldBe(Vector3 assumed, float epsilon = Epsilon)
            {
                var delta = assumed - Res;
                var dist = delta.Length();
                if (dist > epsilon)
                    throw new Exception("{0}({1}) = {2} != {3} [Distance: {4}, Delta {5}]".FormatWith(Op, Inp, Res, assumed, delta, dist));
                return Op;
            }

            private Vector3 Inp;
            private Operation Op;
            private Vector3 Res;
        }

        private class Operation
        {
            public Operation(Func<Vector3, Vector3> func) { Func = func; }

            public Result With(Vector3 v) { return new Result(v, Func(v), this); }
            public Result With(float x, float y, float z) { return With(new Vector3(x, y, z)); }

            private readonly Func<Vector3, Vector3> Func;
        }
    }
}
