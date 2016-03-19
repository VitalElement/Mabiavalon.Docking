using Perspex;
using Perspex.Controls;
using Perspex.Layout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabiavalon.Docking
{
    internal class Tiler
    {
        public static void Tile(IEnumerable<DockItem> dockItems, Size bounds)
        {
            if (dockItems == null) throw new ArgumentNullException(nameof(dockItems));

            var items = new Queue<DockItem>(dockItems.OrderBy(di => di.GetValue(Visual.ZIndexProperty)));

            var cellCountPerColumn = TilerCalculator.GetCellCountPerColumn(items.Count());
            var x = 0d;
            var cellWidth = bounds.Width / cellCountPerColumn.Length;
            foreach (var cellCount in cellCountPerColumn)
            {
                var y = 0d;
                var cellHeight = bounds.Height / cellCount;
                for (var cell = 0; cell < cellCount; cell++)
                {
                    var item = items.Dequeue();
                    Layout.SetFloatingItemState(item, WindowState.Normal);
                    item.SetValue(DockItem.XProperty, x);
                    item.SetValue(DockItem.YProperty, y);
                    item.SetValue(Layoutable.WidthProperty, cellWidth);
                    item.SetValue(Layoutable.HeightProperty, cellHeight);

                    y += cellHeight;
                }

                x += cellWidth;
            }
        }

        public static void TileHorizontally(IEnumerable<DockItem> dockItems, Size bounds)
        {
            if (dockItems == null) throw new ArgumentNullException(nameof(dockItems));

            var items = dockItems.ToList();

            var x = 0.0;
            var width = bounds.Width / items.Count;
            foreach (var dragablzItem in items)
            {
                Layout.SetFloatingItemState(dragablzItem, WindowState.Normal);
                dragablzItem.SetValue(DockItem.XProperty, x);
                dragablzItem.SetValue(DockItem.YProperty, 0d);
                x += width;
                dragablzItem.SetValue(Layoutable.WidthProperty, width);
                dragablzItem.SetValue(Layoutable.HeightProperty, bounds.Height);
            }
        }

        public static void TileVertically(IEnumerable<DockItem> dockItems, Size bounds)
        {
            if (dockItems == null) throw new ArgumentNullException(nameof(dockItems));

            var items = dockItems.ToList();

            var y = 0.0;
            var height = bounds.Height / items.Count;
            foreach (var dragablzItem in items)
            {
                Layout.SetFloatingItemState(dragablzItem, WindowState.Normal);
                dragablzItem.SetValue(DockItem.YProperty, y);
                dragablzItem.SetValue(DockItem.XProperty, 0d);
                y += height;
                dragablzItem.SetValue(Layoutable.HeightProperty, height);
                dragablzItem.SetValue(Layoutable.WidthProperty, bounds.Width);
            }
        }

    }
}
