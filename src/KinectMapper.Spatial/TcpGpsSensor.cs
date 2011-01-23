using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;

namespace KinectMapper.Spatial
{
    public class TcpGpsSensor : IPositionSensor
    {
        public event EventHandler<PositionEventArgs> PositionReceived;

        public PositionData LastPosition { get; private set; }

        private BackgroundWorker worker;

        private TcpClient tcpClient;

        public string Hostname { get; set; }
        public int Port { get; set; }

        private bool started = false;
        private bool locked = false;

        public TcpGpsSensor(string hostname, int port)
        {
            this.worker = new BackgroundWorker();
            this.worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            this.worker.WorkerSupportsCancellation = true;

            this.tcpClient = new TcpClient();
            this.Hostname = hostname;
            this.Port = port;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!worker.CancellationPending)
            {
                if (locked)
                    continue;

                locked = true;

                byte[] buffer = new byte[256];
                this.tcpClient.Client.Receive(buffer);

                System.Text.UTF8Encoding  encoding=new System.Text.UTF8Encoding();
                string message = encoding.GetString(buffer).Trim();

                //super super amazingly crappy 'parsing' of the string. Please don't ever use this. Just for testing quickly...

                if (message.Contains("$GPGGA"))
                {
                    try
                    {
                        message = message.Substring(message.IndexOf("$GPGGA"), 61);
                        GPGGAString data = NMEA.ProcessGPGGA(message);

                        //at some point should integrate gyro/accelerometer to get pitch/yaw/roll information
                        PositionData position = new PositionData(data.Longitude, data.Latitude, data.Altitude, 0, 0, 0);
                        this.LastPosition = position;

                        if (this.started)
                        {
                            this.OnPositionRecieved(position);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                locked = false;
            }
        }

        public void Start()
        {
            this.started = true;
        }

        public void Stop()
        {
            this.started = false;
        }

        public void Connect()
        {
            this.tcpClient.Connect(this.Hostname, this.Port);
            this.worker.RunWorkerAsync();
        }

        public void Disconnect()
        {
            this.worker.CancelAsync();
            this.tcpClient.Close();
        }

        private void OnPositionRecieved(PositionData data)
        {
            if (this.PositionReceived != null)
            {
                this.PositionReceived(this, new PositionEventArgs(data));
            }
        }

    }
}
