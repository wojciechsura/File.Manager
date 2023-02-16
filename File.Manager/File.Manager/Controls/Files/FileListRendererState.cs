using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListRendererState<TRenderer>
        where TRenderer : FileListStateRenderer<TRenderer>
    {
        protected readonly TRenderer renderer;

        public FileListRendererState(TRenderer renderer)
        {
            this.renderer = renderer;
        }

        public virtual void OnEnter()
        {

        }
        public virtual void OnLeave() 
        { 
        
        }

        public abstract void OnKeyDown(KeyEventArgs e);
        public abstract void OnMouseDown(MouseButtonEventArgs e);
        public abstract void OnMouseMove(MouseEventArgs e);
        public abstract void OnMouseUp(MouseButtonEventArgs e);
        public abstract void OnMouseEnter(MouseEventArgs e);
        public abstract void OnMouseLeave(MouseEventArgs e);        
    }
}
