using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using freenect;
using OpenTK;
using KinectMapper.IO;
using KinectMapper.Spatial;

namespace KinectMapper
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    Kinect kinect;

    bool _closed;

    //handler mutexes
    bool lockedRGB;
    bool lockedDepth;

    BackgroundWorker worker;

    Queue<Tuple<KinectRawCompositeFrame, DateTime>> kinectWriterQueue = new Queue<Tuple<KinectRawCompositeFrame, DateTime>>();
    KinectWriter kinectWriter;

    Queue<Tuple<PositionData, DateTime>> positionWriterQueue = new Queue<Tuple<PositionData, DateTime>>();
    PositiontWriter positionWriter;

    byte[] lastImageData;

    int frameRate = 1;
    int depthFrameCount = 0;
    int videoFrameCount = 0;

    IPositionSensor positionSensor;

    public MainWindow()
    {
      this.worker = new BackgroundWorker();
      this.worker.DoWork += new DoWorkEventHandler(worker_DoWork);

      string fileName = String.Format("{0}_{1}_{2}_{3}_{4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour,DateTime.Now.Minute);

      this.kinectWriter = new KinectWriter(@"C:\Temp\" + fileName + ".kinect");
      this.positionWriter = new PositiontWriter(@"C:\Temp\" + fileName + ".location");

      this.positionSensor = new TcpGpsSensor("127.0.0.1", 4352);
      this.positionSensor.PositionReceived += new EventHandler<PositionEventArgs>(positionSensor_PositionReceived);

      InitializeComponent();

        //startup logic from C# wrapper example
      if (Kinect.DeviceCount > 0)
      {
        kinect = new Kinect(0);

        kinect.Open();

        kinect.VideoCamera.DataBuffer = IntPtr.Zero;
        kinect.DepthCamera.DataBuffer = IntPtr.Zero;

        kinect.VideoCamera.DataReceived += VideoCamera_DataReceived;
        kinect.DepthCamera.DataReceived += DepthCamera_DataReceived;

        kinect.VideoCamera.Start();
        kinect.DepthCamera.Start();

        worker.RunWorkerAsync();
        this.positionSensor.Connect();
        this.positionSensor.Start();

        ThreadPool.QueueUserWorkItem(
          delegate
          {
            while (!_closed)
            {
              kinect.UpdateStatus();
              Kinect.ProcessEvents();

              Thread.Sleep(30);
            }
          });
      }
    }

    void positionSensor_PositionReceived(object sender, PositionEventArgs e)
    {
        this.positionWriterQueue.Enqueue(new Tuple<PositionData, DateTime>(e.Position, DateTime.Now));
    }

    void worker_DoWork(object sender, DoWorkEventArgs e)
    {
        while(!worker.CancellationPending)
        {
            if (kinectWriterQueue.Count > 0)
            {
                var item = kinectWriterQueue.Dequeue();
                this.kinectWriter.Write(new KinectRawCompositeFrame(item.Item1.depth, item.Item1.image), item.Item2.Ticks);
            }

            if (positionWriterQueue.Count > 0)
            {
                var item = positionWriterQueue.Dequeue();
                this.positionWriter.Write(item.Item1, item.Item2.Ticks);
            }

            //Thread.Sleep(10);
        }
    }

    void DepthCamera_DataReceived(object sender, DepthCamera.DataReceivedEventArgs e)
    {
      if (lockedDepth)
        return;

      if (depthFrameCount % (30 / frameRate) != 1)
      {
          depthFrameCount++;
          return;
      }

      lockedDepth = true;

      short[] image = new short[e.DepthMap.Width * e.DepthMap.Height];
      short[] depth = new short[e.DepthMap.Width * e.DepthMap.Height];
      int idx = 0;

      for (int i = 0; i < e.DepthMap.Width * e.DepthMap.Height * 2; i += 2)
      {
        short pixel = Marshal.ReadInt16(e.DepthMap.DataPointer, i);
        depth[idx] = pixel;
        // Convert to little endian.
        pixel = IPAddress.HostToNetworkOrder(pixel);
        image[idx++] = pixel;
      }

      this.Dispatcher.Invoke(
        new Action(
          delegate()
          {
              _depthImage.Source = BitmapSource.Create(
                e.DepthMap.Width,
                e.DepthMap.Height,
                96,
                96, PixelFormats.Gray16, null, image, e.DepthMap.Width * 2);
          }));

      this.kinectWriterQueue.Enqueue(new Tuple<KinectRawCompositeFrame, DateTime>(new KinectRawCompositeFrame(depth, lastImageData), DateTime.Now));

      depthFrameCount++;

      lockedDepth = false;
    }

    void VideoCamera_DataReceived(object sender, VideoCamera.DataReceivedEventArgs e)
    {
      if (lockedRGB)
        return;

      //video camera runs at double the fps of the depth so we always have image data available.
      if (videoFrameCount % (30 / (frameRate * 2)) != 1)
      {
          videoFrameCount++;
          return;
      }

      lockedRGB = true;

      lastImageData = e.Image.Data;

      this.Dispatcher.Invoke(
        new Action(
          delegate()
          {
            _colorImage.Source = BitmapSource.Create(
              e.Image.Width, 
              e.Image.Height, 
              96, 
              96, 
              PixelFormats.Rgb24, 
              null, 
              e.Image.Data, 
              e.Image.Width * 3);
          }));

      videoFrameCount++;

      lockedRGB = false;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      _closed = true;
    }
  }
}
