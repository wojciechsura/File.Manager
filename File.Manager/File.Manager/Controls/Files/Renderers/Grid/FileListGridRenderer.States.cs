using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.Controls.Files.Renderers.Grid
{
    internal partial class FileListGridRenderer
    {
        internal class IdleState : FileListRendererState<FileListGridRenderer>
        {
			private void CleanHoverInfo()
			{
				if (renderer.mouseHover != null)
				{
					renderer.mouseHover = null;
					renderer.host.RequestInvalidateVisual();
				}
			}

			private void UpdateHoverInfo(PixelPoint point)
			{
				var mouseHit = renderer.metrics.GetMouseHit(point);

				if (mouseHit is FileListGridRendererMetrics.HeaderMouseHit headerHit)
				{
					if (renderer.mouseHover is not ColumnHoverInfo columnHover || columnHover.ColumnIndex != headerHit.Column)
					{
						renderer.mouseHover = new ColumnHoverInfo(headerHit.Column);
						renderer.host.RequestInvalidateVisual();
					}
				}
				else
				{					
					CleanHoverInfo();
				}
			}

			private PixelPoint GetMousePosition(MouseEventArgs e)
			{
				var mousePoint = e.GetPosition(renderer.host.InputElement);
				var mousePixelPoint = new PixelPoint((int)mousePoint.X, (int)mousePoint.Y);
				return mousePixelPoint;
			}

			public IdleState(FileListGridRenderer renderer)
				: base(renderer)
			{

			}

			public override void OnKeyDown(KeyEventArgs e)
			{
				if (e.Key == Key.Up)
				{
					if (renderer.FilesSource != null)
					{
						if (renderer.FilesSource.CurrentItem == null)
						{
							renderer.FilesSource.MoveCurrentToLast();
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
						else if (renderer.FilesSource.CurrentPosition > 0)
						{
							renderer.FilesSource.MoveCurrentToPrevious();
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
					}

					e.Handled = true;
				}
				else if (e.Key == Key.PageUp)
				{
					if (renderer.FilesSource != null)
					{
						if (renderer.FilesSource.CurrentItem == null)
						{
							renderer.FilesSource.MoveCurrentToLast();
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
						else if (renderer.FilesSource.CurrentPosition > 0)
						{
							// Rounding down on purpose
							int itemsInPage = Math.Max(1, renderer.metrics.Item.ItemArea.Height / renderer.metrics.Item.ItemHeight);

							renderer.FilesSource.MoveCurrentToPosition(Math.Max(0, renderer.FilesSource.CurrentPosition - itemsInPage));
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
					}
				}
				else if (e.Key == Key.Home)
				{
					if (renderer.FilesSource != null)
					{
						renderer.FilesSource.MoveCurrentToFirst();
						renderer.EnsureFocusedItemVisible();
						renderer.host.RequestInvalidateVisual();
					}
				}
				else if (e.Key == Key.Down)
				{
					if (renderer.FilesSource != null)
					{
						if (renderer.FilesSource.CurrentItem == null)
						{
							renderer.FilesSource.MoveCurrentToFirst();
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
						else if (renderer.FilesSource.CurrentPosition < renderer.FilesSource.Cast<object>().Count() - 1)
						{
							renderer.FilesSource.MoveCurrentToNext();
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
					}

					e.Handled = true;
				}
				else if (e.Key == Key.PageDown)
				{
					if (renderer.FilesSource != null)
					{
						if (renderer.FilesSource.CurrentItem == null)
						{
							renderer.FilesSource.MoveCurrentToFirst();
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
						else if (renderer.FilesSource.CurrentPosition < renderer.FilesSource.Cast<object>().Count() - 1)
						{
							int itemCount = renderer.FilesSource.Cast<object>().Count();
							// Rounding down on purpose
							int itemsInPage = Math.Max(1, renderer.metrics.Item.ItemArea.Height / renderer.metrics.Item.ItemHeight);

							renderer.FilesSource.MoveCurrentToPosition(Math.Min(itemCount - 1, renderer.FilesSource.CurrentPosition + itemsInPage));
							renderer.EnsureFocusedItemVisible();
							renderer.host.RequestInvalidateVisual();
						}
					}
				}
				else if (e.Key == Key.End)
				{
					if (renderer.FilesSource != null)
					{
						renderer.FilesSource.MoveCurrentToLast();
						renderer.EnsureFocusedItemVisible();
						renderer.host.RequestInvalidateVisual();
					}
				}
				else if (e.Key == Key.Space)
				{
					if (renderer.FilesSource != null && renderer.FilesSource.CurrentItem != null)
					{
						IFileListItem item = (IFileListItem)renderer.FilesSource.CurrentItem;
						item.IsSelected = !item.IsSelected;
						renderer.EnsureFocusedItemVisible();
						renderer.host.RequestInvalidateVisual();
					}
				}
				else if (e.Key == Key.Insert)
				{
					if (renderer.FilesSource != null && renderer.FilesSource.CurrentItem != null)
					{
						IFileListItem item = (IFileListItem)renderer.FilesSource.CurrentItem;
						item.IsSelected = !item.IsSelected;

						if (renderer.FilesSource.CurrentPosition < renderer.FilesSource.Cast<object>().Count() - 1)
							renderer.FilesSource.MoveCurrentToNext();

						renderer.EnsureFocusedItemVisible();
						renderer.host.RequestInvalidateVisual();
					}
				}
				else if (e.Key == Key.Enter)
				{
					if (renderer.FilesSource != null && renderer.FilesSource.CurrentItem != null)
					{
						renderer.host.RequestExecuteCurrentItem();
					}
				}
			}

			public override void OnMouseDown(MouseButtonEventArgs e)
            {
				CleanHoverInfo();
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                PixelPoint mousePixelPoint = GetMousePosition(e);
                UpdateHoverInfo(mousePixelPoint);
            }

            public override void OnMouseUp(MouseButtonEventArgs e)
            {
				PixelPoint mousePixelPoint = GetMousePosition(e);
				UpdateHoverInfo(mousePixelPoint);
            }

            public override void OnMouseEnter(MouseEventArgs e)
            {
				PixelPoint mousePixelPoint = GetMousePosition(e);
				UpdateHoverInfo(mousePixelPoint);
            }

            public override void OnMouseLeave(MouseEventArgs e)
            {
				CleanHoverInfo();
            }
        }
    }
}
