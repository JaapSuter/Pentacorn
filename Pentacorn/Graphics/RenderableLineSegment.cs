using System;
using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class RenderableLineSegment : Renderable<LineSegment>
    {
        public RenderableLineSegment(RendererImpl rendererImpl)
            : base()
        {
            RenderableQuad = new RenderableQuad(rendererImpl);
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, LineSegment lineSegment)
        {
            if (lineSegment.Thickness < float.Epsilon)
                return;

            var thickFrom = lineSegment.Thickness * new Vector3(-lineSegment.Dir.Y, lineSegment.Dir.X, 0);
            var thickTo = thickFrom;

            var eps = 0.02f;

            if (Math.Abs(lineSegment.Dir.Z) > eps)
            {
                var v = Matrix.Invert(viewProject.View);
                var vp = v.Translation;

                var vptf = Vector3.Normalize(lineSegment.From - vp);
                var af = 1.0f - Math.Abs(Vector3.Dot(lineSegment.Dir, vptf));
                vptf = af < eps ? v.Up : vptf;

                var vptt = Vector3.Normalize(lineSegment.To - vp);
                var at = 1.0f - Math.Abs(Vector3.Dot(lineSegment.Dir, vptt));
                vptt = af < eps ? v.Up : vptt;

                thickFrom = lineSegment.Thickness * Vector3.Cross(lineSegment.Dir, vptf);
                thickTo = lineSegment.Thickness * Vector3.Cross(lineSegment.Dir, vptt);
            }

            var quad = new Quad(lineSegment.From - thickFrom, lineSegment.From + thickFrom, 
                                lineSegment.To - thickTo, lineSegment.To + thickTo, rendererImpl.WhiteTexel, lineSegment.Color);
            quad.Homography = lineSegment.Homography;

            RenderableQuad.Render(rendererImpl, viewProject, quad);
        }

        RenderableQuad RenderableQuad;
    }
}
