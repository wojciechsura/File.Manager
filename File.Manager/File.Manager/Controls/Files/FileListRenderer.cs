using File.Manager.BusinessLogic.Models.Files;
using File.Manager.BusinessLogic.ViewModels.Pane;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListRenderer
    {
        // Protected types ----------------------------------------------------

        protected class GlyphRunInfo
        {
            private readonly GlyphTypeface glyphTypeface;
            private readonly double fontSize;

            public GlyphRunInfo(GlyphTypeface glyphTypeface, double fontSize, double x)
            {
                this.glyphTypeface = glyphTypeface;
                this.fontSize = fontSize;
                StartX = x;
                CurrentX = x;
            }

            public void AddGlyph(char c)
            {
                if (glyphTypeface.CharacterToGlyphMap.TryGetValue(c, out ushort glyphIndex))
                {
                    var advanceWidth = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
                    GlyphIndices.Add(glyphIndex);
                    AdvanceWidths.Add(advanceWidth);

                    CurrentX += advanceWidth;
                }
            }

            public void Draw(DrawingContext drawingContext, float pixelsPerDip, Brush brush, double y)
            {
                if (GlyphIndices.Any() && AdvanceWidths.Any())
                {
                    var glyphRun = new GlyphRun(glyphTypeface,
                        bidiLevel: 0,
                        isSideways: false,
                        renderingEmSize: fontSize,
                        pixelsPerDip: pixelsPerDip,
                        glyphIndices: GlyphIndices,
                        baselineOrigin: new Point(Math.Round(StartX),
                            Math.Round(glyphTypeface.Baseline * fontSize + y)),
                        advanceWidths: AdvanceWidths,
                        glyphOffsets: null,
                        characters: null,
                        deviceFontName: null,
                        clusterMap: null,
                        caretStops: null,
                        language: null);

                    drawingContext.DrawGlyphRun(brush, glyphRun);
                }
            }

            public List<ushort> GlyphIndices { get; } = new List<ushort>();
            public List<double> AdvanceWidths { get; } = new List<double>();
            public double CurrentX { get; set; }
            public double StartX { get; }
        }

        // Private fields -----------------------------------------------------

        private FileListColumnCollection columns;
        private ICollectionView filesSource;

        private Typeface typeface;
        private GlyphTypeface glyphTypeface;

        // A workaround for ObservableCollection's Reset event
        // not carying information about actual change. There seems
        // to be no other way to properly remove/add event handlers
        // to items.
        private readonly List<IFileListItem> filesCache = new();

        // Private methods ----------------------------------------------------

        private void ResetFilesCache()
        {
            foreach (var item in filesCache)
            {
                item.PropertyChanged -= HandleItemPropertyChanged;
            }

            filesCache.Clear();

            if (filesSource != null)
            {
                foreach (var item in filesSource.Cast<IFileListItem>())
                {
                    item.PropertyChanged += HandleItemPropertyChanged;
                    filesCache.Add(item);
                }
            }
        }

        private void ApplyFilesCacheChange(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex;
                        foreach (var item in e.NewItems.Cast<IFileListItem>()) {
                            item.PropertyChanged += HandleItemPropertyChanged;
                            filesCache.Insert(index, item);
                            index++;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems.Cast<IFileListItem>())
                        {
                            item.PropertyChanged -= HandleItemPropertyChanged;
                            filesCache.RemoveAt(e.OldStartingIndex);
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (e.OldItems.Count != e.NewItems.Count)
                            throw new InvalidOperationException("Invalid Replace collection change!");

                        foreach (var item in e.OldItems.Cast<IFileListItem>())
                        {
                            item.PropertyChanged -= HandleItemPropertyChanged;
                        }

                        foreach (var item in e.NewItems.Cast<IFileListItem>())
                        {
                            item.PropertyChanged += HandleItemPropertyChanged;
                        }

                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            filesCache[e.NewStartingIndex + i] = (IFileListItem)e.NewItems[i];
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        if (e.OldItems.Count != e.NewItems.Count)
                            throw new InvalidOperationException("Invalid Move collection change!");

                        int oldIndex = e.OldStartingIndex;
                        int newIndex = e.NewStartingIndex;

                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            var item = filesCache[oldIndex + i];
                            filesCache.RemoveAt(oldIndex);
                            filesCache.Insert(newIndex, item);
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        ResetFilesCache();
                        break;
                    }
            }
        }

        private void SetColumns(FileListColumnCollection value)
        {
            if (columns != null)
            {
                columns.CollectionChanged -= HandleColumnsCollectionChanged;
                columns.ColumnWidthsChanged -= HandleColumnsWidthsChanged;
            }

            columns = value;

            if (columns != null)
            {
                columns.CollectionChanged += HandleColumnsCollectionChanged;
                columns.ColumnWidthsChanged += HandleColumnsWidthsChanged;
            }

            OnColumnsChanged();
        }

        private void SetFilesSource(ICollectionView value)
        {
            if (filesSource != null)
            {
                filesSource.CollectionChanged -= HandleFilesSourceCollectionChanged;
                filesSource.CurrentChanged -= HandleFilesSourceCurrentChanged;
            }

            filesSource = value;

            if (filesSource != null)
            {
                filesSource.CollectionChanged += HandleFilesSourceCollectionChanged;
                filesSource.CurrentChanged += HandleFilesSourceCurrentChanged;
            }

            ResetFilesCache();

            OnFilesSourceChanged();
        }

        private bool EnsureGlyphTypeface()
        {
            if (typeface != null && glyphTypeface != null)
                return true;

            typeface = new Typeface(host.FontFamily, host.FontStyle, host.FontWeight, host.FontStretch);
            if (typeface.TryGetGlyphTypeface(out glyphTypeface))
                return true;

            typeface = null;
            glyphTypeface = null;
            return false;
        }

        // Protected fields ---------------------------------------------------

        protected readonly IFileListRendererHost host;

        // Protected methods --------------------------------------------------

        protected FileListRenderer(IFileListRendererHost host)
        {
            this.host = host;
        }

        protected abstract void HandleColumnsWidthsChanged(object sender, EventArgs e);

        protected abstract void HandleColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        protected abstract void HandleFilesSourceCurrentChanged(object sender, EventArgs e);

        protected virtual void HandleFilesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ApplyFilesCacheChange(e);
        }

        protected virtual void OnColumnsChanged()
        {

        }

        protected virtual void OnFilesSourceChanged()
        {

        }

        protected virtual void HandleItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        protected void DrawText(DrawingContext drawingContext, Brush brush, string text, double x, double y)
        {
            if (!EnsureGlyphTypeface())
                return;

            var run = new GlyphRunInfo(glyphTypeface, host.FontSize, x);

            foreach (var c in text)
                run.AddGlyph(c);

            run.Draw(drawingContext, (float)host.PixelsPerDip, brush, y);
        }

        // Public methods -----------------------------------------------------
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

        public virtual void NotifyFontChanged()
        {
            typeface = null;
            glyphTypeface = null;

            host.RequestInvalidateVisual();
        }

        public abstract void Render(DrawingContext drawingContext);

        public abstract void UpdateScrollData();

        public abstract void NotifyMetricsChanged();

        public abstract void NotifyScrollPositionChanged();

        // Public properties --------------------------------------------------

        public FileListColumnCollection Columns
        {
            get => columns;
            set => SetColumns(value);
        }
        
        public ICollectionView FilesSource 
        {
            get => filesSource;
            set => SetFilesSource(value); 
        }
    }
}
