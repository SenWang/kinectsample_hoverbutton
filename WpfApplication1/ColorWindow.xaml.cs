using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace WpfApplication1
{

    public partial class ColorWindow : Window
    {
        KinectSensor kinect;
        public ColorWindow(KinectSensor sensor) : this()
        {
            kinect = sensor;
        }

        public ColorWindow()
        {
            InitializeComponent();
            Loaded += ColorWindow_Loaded;
            Unloaded += ColorWindow_Unloaded;
        }
        void ColorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                kinect.ColorStream.Disable();
                kinect.SkeletonStream.Disable();
                kinect.Stop();
                kinect.ColorFrameReady -= myKinect_ColorFrameReady;
                kinect.SkeletonFrameReady -= mykinect_SkeletonFrameReady;
            }
        }
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        void ColorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                ColorImageStream colorStream = kinect.ColorStream;

                _ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,colorStream.FrameHeight, 96, 96,PixelFormats.Bgr32, null);
                _ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,colorStream.FrameHeight);
                _ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorData.Source = _ColorImageBitmap;

                kinect.ColorStream.Enable();
                kinect.ColorFrameReady += myKinect_ColorFrameReady;
                kinect.SkeletonStream.Enable();
                kinect.SkeletonFrameReady += mykinect_SkeletonFrameReady;
                kinect.Start();

                //if(AcceptButton.uxSBHold!=null)
                //    AcceptButton.uxSBHold.Completed += new EventHandler(uxSBHold_Completed);
            }
        }

        //void uxSBHold_Completed(object sender, EventArgs e)
        //{
        //    MessageBox.Show("OK");
        //}

        Skeleton[] FrameSkeletons ;
        void mykinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skframe = e.OpenSkeletonFrame())
            {
                if (skframe != null)
                {
                    FrameSkeletons = new Skeleton[skframe.SkeletonArrayLength];
                    skframe.CopySkeletonDataTo(FrameSkeletons);
                    for (int i = 0; i < FrameSkeletons.Length; i++)
                    {
                        if (FrameSkeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            ColorImagePoint cpl = MapToColorImage(FrameSkeletons[i].Joints[JointType.HandLeft]);
                            DrawHand(cpl);
                        }
                    }
                }
            }
        }
        ColorImagePoint MapToColorImage(Joint jp)
        {
            ColorImagePoint cp = kinect.MapSkeletonPointToColor(jp.Position, kinect.ColorStream.Format);
            return cp ;
        }

        void DrawHand(ColorImagePoint cp)
        {
            double newLeft = cp.X - LeftHand.Width / 2 ;
            double newTop = cp.Y - LeftHand.Height / 2;
            Canvas.SetLeft(LeftHand, newLeft);
            Canvas.SetTop(LeftHand, newTop);
            if (newLeft <100  && newTop < 100)
                AcceptButton.Hovering();
            else
            {
                AcceptButton.IsChecked = false;
                AcceptButton.Release();
            }             
        }



        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    _ColorImageBitmap.WritePixels(_ColorImageBitmapRect, pixelData,_ColorImageStride, 0);
                }
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("OK");
        }
    }
}
