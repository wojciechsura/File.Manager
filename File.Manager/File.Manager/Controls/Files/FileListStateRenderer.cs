using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListStateRenderer<TConcreteRenderer> : FileListRenderer
        where TConcreteRenderer : FileListStateRenderer<TConcreteRenderer>
    {
        private FileListRendererState<TConcreteRenderer> state;

        private void SetState(FileListRendererState<TConcreteRenderer> state)             
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (this.state != state)
            {
                this.state.OnLeave();
                this.state = state;
                this.state.OnEnter();
            }
        }

        protected FileListRendererState<TConcreteRenderer> State
        {
            get => state;
            set => SetState(value);
        }

        public FileListStateRenderer(IFileListRendererHost host, Func<TConcreteRenderer, FileListRendererState<TConcreteRenderer>> initialStateBuilder)
            : base(host)
        {
            state = initialStateBuilder((TConcreteRenderer)this) ?? throw new ArgumentNullException(nameof(initialStateBuilder));
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            state.OnKeyDown(e);
        }

        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            state.OnMouseDown(e);
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            state.OnMouseMove(e);
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            state.OnMouseUp(e);
        }

        public override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            state.OnMouseEnter(e);
        }

        public override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            state.OnMouseLeave(e);
        }
    }
}
