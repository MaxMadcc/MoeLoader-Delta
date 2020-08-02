﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MoeLoaderDelta
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void PART_VerticalScrollBar_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ScrollBar sc)
                {
                    ScrollViewer sv = sc.TemplatedParent as ScrollViewer;
                    double pos = e.GetPosition(sc).Y;
                    if (pos > sc.Value / sv.ExtentHeight * sc.ActualHeight + sc.Track.Thumb.ActualHeight)
                        sv.PageDown();
                    else if (pos < sc.Value / sv.ExtentHeight * sc.ActualHeight)
                        sv.PageUp();
                    //sv.ScrollToVerticalOffset(e.GetPosition(sc).Y / sc.ActualHeight * sv.ExtentHeight);
                }
            }
        }

        private void PART_HorizontalScrollBar_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ScrollBar sc)
                {
                    ScrollViewer sv = sc.TemplatedParent as ScrollViewer;
                    //sv.ScrollToHorizontalOffset(e.GetPosition(sc).X / sc.ActualWidth * sv.ExtentWidth);
                    double pos = e.GetPosition(sc).X;
                    if (pos > sc.Value / sv.ExtentWidth * sc.ActualWidth + sc.Track.Thumb.ActualWidth)
                        sv.PageRight();
                    else if (pos < sc.Value / sv.ExtentWidth * sc.ActualWidth)
                        sv.PageLeft();
                }
            }
        }
    }
}
