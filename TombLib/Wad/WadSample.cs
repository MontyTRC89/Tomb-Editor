using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadSample : IEquatable<WadSample>
    {
        public string Name { get; private set; }
        public byte[] WaveData { get; private set; }
        public Hash Hash { get { return _hash; } }

        private Hash _hash;

        public WadSample(string name, byte[] data)
        {
            WaveData = data;
            Name = name;

            UpdateHash();
        }

        public WadSample(string name)
        {
            WaveData = null;
            Name = name;

            UpdateHash();
        }

        public bool Embedded { get { return (WaveData != null && WaveData.Length > 1); } }

        public Hash UpdateHash()
        {
            // If embedded, then hash is derived from wave data, otherwise simply from name
            if (Embedded)
                _hash = Hash.FromByteArray(WaveData);
            else
                _hash = Hash.FromByteArray(System.Text.UTF8Encoding.UTF8.GetBytes(Name));
            return _hash;
        }

        public bool Equals(WadSample other)
        {
            return (Hash == other.Hash);
        }

        public void SetData(byte[] buffer)
        {
            WaveData = buffer;
            UpdateHash();
        }

        public WadSample Clone()
        {
            return new WadSample(Name, (byte[])WaveData?.Clone() ?? null);
        }

        public void Play()
        {
            if (!Embedded) return;

            using (var stream = new MemoryStream(WaveData))
            {
                using (var player = new SoundPlayer(stream))
                {
                    player.Play();
                }
            }
        }

        public int Duration
        {
            get
            {
                if (!Embedded) return 0;

                using (var stream = new MemoryStream(WaveData))
                {
                    using (var wfr = new WaveFileReader(stream))
                    {
                        TimeSpan totalTime = wfr.TotalTime;
                        return totalTime.Milliseconds;
                    }
                }
            }
        }
    }
}
