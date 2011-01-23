using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KinectMapper.Spatial
{
    public class PositionReader
    {
        private BinaryReader reader;
        public int Count { get; private set; }

        public List<Tuple<long, long>> Index = new List<Tuple<long, long>>();

        public PositionReader(Stream fileStream)
        {
            this.reader = new BinaryReader(fileStream);
            this.createIndexes();
        }

        private void createIndexes()
        {
            this.reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (this.reader.BaseStream.Position < this.reader.BaseStream.Length)
            {
                try
                {
                    long position = this.reader.BaseStream.Position;
                    long ticks = this.reader.ReadInt64();
                    this.reader.BaseStream.Position += 6 * sizeof(double);                    
                    this.Index.Add(new Tuple<long, long>(ticks, position));
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    break;
                }

                this.Count = this.Index.Count;
            }
        }

        public Tuple<PositionData,long> ReadPositionAt(int id)
        {
            long position = this.Index[id].Item2;
            this.reader.BaseStream.Seek(position, SeekOrigin.Begin);

            long ticks = this.reader.ReadInt64();
            double X = this.reader.ReadDouble();
            double Y = this.reader.ReadDouble();
            double Z = this.reader.ReadDouble();
            double Roll = this.reader.ReadDouble();
            double Pitch = this.reader.ReadDouble();
            double Yaw = this.reader.ReadDouble();

            return new Tuple<PositionData,long>(new PositionData(X,Y,Z,Pitch,Yaw,Roll),ticks);
        }
    }
}
