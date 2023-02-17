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

        public virtual void OnKeyDown(KeyEventArgs e)
        {

        }

        public virtual void OnMouseDown(MouseButtonEventArgs e)
        {

        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {

        }

        public virtual void OnMouseUp(MouseButtonEventArgs e)
        {

        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {

        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {

        }

        public virtual void OnMouseDoubleClick(MouseButtonEventArgs e)
        {

        }

        public virtual void OnMouseWheel(MouseWheelEventArgs e)
        {

        }
    }
}
