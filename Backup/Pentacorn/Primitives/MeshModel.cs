using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class MeshModel : IVisible
    {
        public MeshModel(string name, Matrix world)
        {
            World = world;
            Model = Program.Renderer.LeaseModel(name);            
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

        public Matrix World;
        public Model Model;
        public IViewProject ProjectorViewProject;
    }
}
