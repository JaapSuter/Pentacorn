using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pentacorn.Graphics;

namespace Pentacorn
{
    class Scene : IVisible, ICollection<IVisible>
    {
        public IViewProject ViewProject { get; set; }
        
        public Scene(IViewProject viewProject)
        {
            Debug.Assert(viewProject != null);
            ViewProject = viewProject;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            Visibles.ToArray().Run(v => renderer.Render(ViewProject, v));
        }

        public void Add(Scene item)
        {
            Visibles.Add(item);
        }

        public void Add(IVisible item)
        {
            Visibles.Add(item);
        }

        public void Add(params IVisible[] items)
        {
            foreach (var item in items)
                Visibles.Add(item);
        }

        public void Add(IEnumerable<IVisible> items)
        {
            foreach (var item in items)
                Visibles.Add(item);
        }

        public void Clear()
        {
            Visibles.Clear();
        }

        public bool Contains(IVisible item)
        {
            return Visibles.Contains(item);
        }

        public void CopyTo(IVisible[] array, int arrayIndex)
        {
            Visibles.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Visibles.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IVisible item)
        {
            return Visibles.Remove(item);
        }

        public IEnumerator<IVisible> GetEnumerator()
        {
            return Visibles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Visibles.GetEnumerator();
        }

        private List<IVisible> Visibles = new List<IVisible>();
    }
}
