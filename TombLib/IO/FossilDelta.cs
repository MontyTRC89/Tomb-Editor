// From the FossilDelta library (29th of July 2018)
//    https://github.com/endel/FossilDelta
//
//
// License is 2 clause BSD:
//
// Copyright 2016 Endel Dreyer(C# port)
// Copyright 2014 Dmitry Chestnykh (JavaScript port)
// Copyright 2007 D.Richard Hipp(original C version)
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or
// without modification, are permitted provided that the
// following conditions are met:
//
//   1. Redistributions of source code must retain the above
//      copyright notice, this list of conditions and the
//      following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above
//      copyright notice, this list of conditions and the
//      following disclaimer in the documentation and/or other
//      materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE AUTHORS ``AS IS'' AND ANY EXPRESS
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.IN NO EVENT SHALL THE AUTHORS OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
// OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation
// are those of the authors and contributors and should not be interpreted
// as representing official policies, either expressed or implied, of anybody
// else.

using System;
using System.Collections.Generic;

namespace TombLib.IO
{
    public class FossilDelta
    {
        public static UInt16 NHASH = 16;

        public static byte[] Create(byte[] origin, int lenSrc, byte[] target, int lenOut)
        {
            var zDelta = new Writer();
            int i, lastRead = -1;

            zDelta.PutInt((uint)lenOut);
            zDelta.PutChar('\n');

            // If the source is very small, it means that we have no
            // chance of ever doing a copy command.  Just output a single
            // literal segment for the entire target and exit.
            if (lenSrc <= NHASH)
            {
                zDelta.PutInt((uint)lenOut);
                zDelta.PutChar(':');
                zDelta.PutArray(target, 0, lenOut);
                zDelta.PutInt(Checksum(target));
                zDelta.PutChar(';');
                return zDelta.ToArray();
            }

            // Compute the hash table used to locate matching sections in the source.
            int nHash = (int)lenSrc / NHASH;
            int[] collide = new int[nHash];
            int[] landmark = new int[nHash];
            for (i = 0; i < collide.Length; i++)
                collide[i] = -1;
            for (i = 0; i < landmark.Length; i++)
                landmark[i] = -1;
            int hv;
            RollingHash h = new RollingHash();
            for (i = 0; i < lenSrc - NHASH; i += NHASH)
            {
                h.Init(origin, i);
                hv = (int)(h.Value() % nHash);
                collide[i / NHASH] = landmark[hv];
                landmark[hv] = i / NHASH;
            }

            int _base = 0;
            int iSrc, iBlock;
            int bestCnt, bestOfst = 0, bestLitsz = 0;
            while (_base + NHASH < lenOut)
            {
                bestOfst = 0;
                bestLitsz = 0;
                h.Init(target, _base);
                i = 0; // Trying to match a landmark against zOut[_base+i]
                bestCnt = 0;
                while (true)
                {
                    int limit = 250;
                    hv = (int)(h.Value() % nHash);
                    iBlock = landmark[hv];
                    while (iBlock >= 0 && (limit--) > 0)
                    {
                        //
                        // The hash window has identified a potential match against
                        // landmark block iBlock.  But we need to investigate further.
                        //
                        // Look for a region in zOut that matches zSrc. Anchor the search
                        // at zSrc[iSrc] and zOut[_base+i].  Do not include anything prior to
                        // zOut[_base] or after zOut[outLen] nor anything after zSrc[srcLen].
                        //
                        // Set cnt equal to the length of the match and set ofst so that
                        // zSrc[ofst] is the first element of the match.  litsz is the number
                        // of characters between zOut[_base] and the beginning of the match.
                        // sz will be the overhead (in bytes) needed to encode the copy
                        // command.  Only generate copy command if the overhead of the
                        // copy command is less than the amount of literal text to be copied.
                        //
                        int cnt, ofst, litsz;
                        int j, k, x, y;
                        int sz;

                        // Beginning at iSrc, match forwards as far as we can.
                        // j counts the number of characters that match.
                        iSrc = iBlock * NHASH;
                        for (j = 0, x = iSrc, y = _base + i; x < lenSrc && y < lenOut; j++, x++, y++)
                        {
                            if (origin[x] != target[y])
                                break;
                        }
                        j--;

                        // Beginning at iSrc-1, match backwards as far as we can.
                        // k counts the number of characters that match.
                        for (k = 1; k < iSrc && k <= i; k++)
                        {
                            if (origin[iSrc - k] != target[_base + i - k])
                                break;
                        }
                        k--;

                        // Compute the offset and size of the matching region.
                        ofst = iSrc - k;
                        cnt = j + k + 1;
                        litsz = i - k;  // Number of bytes of literal text before the copy
                                        // sz will hold the number of bytes needed to encode the "insert"
                                        // command and the copy command, not counting the "insert" text.
                        sz = DigitCount(i - k) + DigitCount(cnt) + DigitCount(ofst) + 3;
                        if (cnt >= sz && cnt > bestCnt)
                        {
                            // Remember this match only if it is the best so far and it
                            // does not increase the file size.
                            bestCnt = cnt;
                            bestOfst = iSrc - k;
                            bestLitsz = litsz;
                        }

                        // Check the next matching block
                        iBlock = collide[iBlock];
                    }

                    // We have a copy command that does not cause the delta to be larger
                    // than a literal insert.  So add the copy command to the delta.
                    if (bestCnt > 0)
                    {
                        if (bestLitsz > 0)
                        {
                            // Add an insert command before the copy.
                            zDelta.PutInt((uint)bestLitsz);
                            zDelta.PutChar(':');
                            zDelta.PutArray(target, _base, _base + bestLitsz);
                            _base += bestLitsz;
                        }
                        _base += bestCnt;
                        zDelta.PutInt((uint)bestCnt);
                        zDelta.PutChar('@');
                        zDelta.PutInt((uint)bestOfst);
                        zDelta.PutChar(',');
                        if (bestOfst + bestCnt - 1 > lastRead)
                        {
                            lastRead = bestOfst + bestCnt - 1;
                        }
                        bestCnt = 0;
                        break;
                    }

                    // If we reach this point, it means no match is found so far
                    if (_base + i + NHASH >= lenOut)
                    {
                        // We have reached the end and have not found any
                        // matches.  Do an "insert" for everything that does not match
                        zDelta.PutInt((uint)(lenOut - _base));
                        zDelta.PutChar(':');
                        zDelta.PutArray(target, _base, _base + lenOut - _base);
                        _base = lenOut;
                        break;
                    }

                    // Advance the hash by one character. Keep looking for a match.
                    h.Next(target[_base + i + NHASH]);
                    i++;
                }
            }
            // Output a final "insert" record to get all the text at the end of
            // the file that does not match anything in the source.
            if (_base < lenOut)
            {
                zDelta.PutInt((uint)(lenOut - _base));
                zDelta.PutChar(':');
                zDelta.PutArray(target, _base, _base + lenOut - _base);
            }
            // Output the final checksum record.
            zDelta.PutInt(Checksum(target));
            zDelta.PutChar(';');
            return zDelta.ToArray();
        }

        public static byte[] Apply(byte[] origin, uint lenSrc, byte[] delta, uint lenDelta)
        {
            uint limit, total = 0;
            Reader zDelta = new Reader(delta);

            limit = zDelta.GetInt();
            if (zDelta.GetChar() != '\n')
                throw new Exception("size integer not terminated by \'\\n\'");

            Writer zOut = new Writer();
            while (zDelta.HaveBytes())
            {
                uint cnt, ofst;
                cnt = zDelta.GetInt();

                switch (zDelta.GetChar())
                {
                    case '@':
                        ofst = zDelta.GetInt();
                        if (zDelta.HaveBytes() && zDelta.GetChar() != ',')
                            throw new Exception("copy command not terminated by \',\'");
                        total += cnt;
                        if (total > limit)
                            throw new Exception("copy exceeds output file size");
                        if (ofst + cnt > lenSrc)
                            throw new Exception("copy extends past end of input");
                        zOut.PutArray(origin, (int)ofst, (int)(ofst + cnt));
                        break;

                    case ':':
                        total += cnt;
                        if (total > limit)
                            throw new Exception("insert command gives an output larger than predicted");
                        if (cnt > lenDelta)
                            throw new Exception("insert count exceeds size of delta");
                        zOut.PutArray(zDelta.a, (int)zDelta.pos, (int)(zDelta.pos + cnt));
                        zDelta.pos += cnt;
                        break;

                    case ';':
                        byte[] output = zOut.ToArray();
                        if (cnt != Checksum(output))
                            throw new Exception("bad checksum");
                        if (total != limit)
                            throw new Exception("generated size does not match predicted size");
                        return output;

                    default:
                        throw new Exception("unknown delta operator");
                }
            }
            throw new Exception("unterminated delta");
        }

        public static uint OutputSize(byte[] delta)
        {
            Reader zDelta = new Reader(delta);
            uint size = zDelta.GetInt();
            if (zDelta.GetChar() != '\n')
                throw new Exception("size integer not terminated by \'\\n\'");
            return size;
        }

        static int DigitCount(int v)
        {
            int i, x;
            for (i = 1, x = 64; v >= x; i++, x <<= 6)
            { }
            return i;
        }

        // Return a 32-bit checksum of the array.
        static uint Checksum(byte[] arr)
        {
            uint sum0 = 0, sum1 = 0, sum2 = 0, sum = 0,
            z = 0, N = (uint)arr.Length;

            while (N >= 16)
            {
                sum0 += (uint)arr[z + 0] + arr[z + 4] + arr[z + 8] + arr[z + 12];
                sum1 += (uint)arr[z + 1] + arr[z + 5] + arr[z + 9] + arr[z + 13];
                sum2 += (uint)arr[z + 2] + arr[z + 6] + arr[z + 10] + arr[z + 14];
                sum += (uint)arr[z + 3] + arr[z + 7] + arr[z + 11] + arr[z + 15];
                z += 16;
                N -= 16;
            }
            while (N >= 4)
            {
                sum0 += arr[z + 0];
                sum1 += arr[z + 1];
                sum2 += arr[z + 2];
                sum += arr[z + 3];
                z += 4;
                N -= 4;
            }

            sum += (sum2 << 8) + (sum1 << 16) + (sum0 << 24);
            switch (N & 3)
            {
                case 3:
                    sum += (uint)(arr[z + 2] << 8);
                    sum += (uint)(arr[z + 1] << 16);
                    sum += (uint)(arr[z + 0] << 24);
                    break;
                case 2:
                    sum += (uint)(arr[z + 1] << 16);
                    sum += (uint)(arr[z + 0] << 24);
                    break;
                case 1:
                    sum += (uint)(arr[z + 0] << 24);
                    break;
            }
            return sum;
        }

        private class Writer
        {
            static readonly uint[] zDigits = {
                    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D',
                    'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
                    'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '_', 'a', 'b', 'c', 'd', 'e',
                    'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
                    't', 'u', 'v', 'w', 'x', 'y', 'z', '~'
                };
            private List<byte> a;

            public Writer()
            {
                this.a = new List<byte>();
            }

            public void PutChar(char c)
            {
                this.a.Add((byte)c);
            }

            public void PutInt(uint v)
            {
                int i, j;
                uint[] zBuf = new uint[20];

                if (v == 0)
                {
                    this.PutChar('0');
                    return;
                }
                for (i = 0; v > 0; i++, v >>= 6)
                {
                    zBuf[i] = zDigits[v & 0x3f];
                }
                for (j = i - 1; j >= 0; j--)
                {
                    this.a.Add((byte)zBuf[j]);
                }
            }

            public void PutArray(byte[] a, int start, int end)
            {
                for (var i = start; i < end; i++)
                    this.a.Add(a[i]);
            }

            public byte[] ToArray()
            {
                return this.a.ToArray();
            }
        }

        private class Reader
        {
            static readonly int[] zValue = {
                    -1, -1, -1, -1, -1, -1, -1, -1,   -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1,   -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1,   -1, -1, -1, -1, -1, -1, -1, -1,
                    0,  1,  2,  3,  4,  5,  6,  7,    8,  9, -1, -1, -1, -1, -1, -1,
                    -1, 10, 11, 12, 13, 14, 15, 16,   17, 18, 19, 20, 21, 22, 23, 24,
                    25, 26, 27, 28, 29, 30, 31, 32,   33, 34, 35, -1, -1, -1, -1, 36,
                    -1, 37, 38, 39, 40, 41, 42, 43,   44, 45, 46, 47, 48, 49, 50, 51,
                    52, 53, 54, 55, 56, 57, 58, 59,   60, 61, 62, -1, -1, -1, 63, -1
                };

            public byte[] a;
            public uint pos;

            public Reader(byte[] array)
            {
                this.a = array;
                this.pos = 0;
            }

            public bool HaveBytes()
            {
                return this.pos < this.a.Length;
            }

            public byte GetByte()
            {
                byte b = this.a[this.pos];
                this.pos++;
                if (this.pos > this.a.Length)
                    throw new IndexOutOfRangeException("out of bounds");
                return b;
            }

            public char GetChar()
            {
                //  return String.fromCharCode(this.getByte());
                return (char)this.GetByte();
            }

            /**
             * Read bytes from *pz and convert them into a positive integer.  When
             * finished, leave *pz pointing to the first character past the end of
             * the integer.  The *pLen parameter holds the length of the string
             * in *pz and is decremented once for each character in the integer.
             */
            public uint GetInt()
            {
                uint v = 0;
                int c;
                while (this.HaveBytes() && (c = zValue[0x7f & this.GetByte()]) >= 0)
                {
                    v = (uint)((((Int32)v) << 6) + c);
                }
                this.pos--;
                return v;
            }
        }

        private class RollingHash
        {
            private UInt16 a;
            private UInt16 b;
            private UInt16 i;
            private byte[] z;
            static int ii = 0;

            public RollingHash()
            {
                this.a = 0;
                this.b = 0;
                this.i = 0;
                this.z = new byte[NHASH];
            }

            /**
             * Initialize the rolling hash using the first NHASH characters of z[]
             */
            public void Init(byte[] z, int pos)
            {
                UInt16 a = 0, b = 0, i, x;
                for (i = 0; i < NHASH; i++)
                {
                    x = z[pos + i];
                    a = (UInt16)((a + x) & 0xffff);
                    b = (UInt16)((b + (NHASH - i) * x) & 0xffff);
                    this.z[i] = (byte)x;
                }
                this.a = (UInt16)(a & 0xffff);
                this.b = (UInt16)(b & 0xffff);
                this.i = 0;
            }

            /**
             * Advance the rolling hash by a single character "c"
             */
            public void Next(byte c)
            {
                UInt16 old = this.z[this.i];
                this.z[this.i] = c;
                this.i = (UInt16)((this.i + 1) & (NHASH - 1));
                this.a = (UInt16)(this.a - old + c);
                this.b = (UInt16)(this.b - NHASH * old + this.a);
            }


            /**
             * Return a 32-bit hash value
             */
            public UInt32 Value()
            {
                RollingHash.ii++;
                return (UInt32)(((UInt32)(this.a & 0xffff)) | (((UInt32)(this.b & 0xffff)) << 16));
            }

            /*
             * Compute a hash on NHASH bytes.
             *
             * This routine is intended to be equivalent to:
             *    hash h;
             *    hash_init(&h, zInput);
             *    return hash_32bit(&h);
             */
            public static UInt32 Once(byte[] z)
            {
                UInt16 a, b, i;
                a = b = z[0];
                for (i = 1; i < NHASH; i++)
                {
                    a += z[i];
                    b += a;
                }
                return a | (((UInt32)b) << 16);
            }
        }
    }
}
