using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;

namespace TombLib.Wad
{
    /* public class AnimationFile
     {
         // Properties
         public WadAnimation Animation { get; set; }
         public List<WadSample> Samples { get; set; }
         public List<WadSoundInfo> Sounds { get; set; }

         // Reserved chunks
         private static readonly byte[] MagicNumber = new byte[] { 0x41, 0x4E, 0x49, 0x4D };

         public AnimationFile()
         {
             Animation = new WadAnimation();
             Samples = new List<WadSample>();
             Sounds = new List<WadSoundInfo>();
         }

         public static AnimationFile LoadAnimationFile(string fileName)
         {
             return LoadAnimationFile(File.OpenRead(fileName));
         }

         public static AnimationFile LoadAnimationFile(Stream stream)
         {
             byte[] magicNumber = new byte[4];
             stream.Read(magicNumber, 0, 4);
             stream.Seek(-4, SeekOrigin.Current);


             var file = new AnimationFile();

             using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, stream))
             {
                 chunkIO.ReadChunks((id, chunkSize) =>
                 {
                     if (id == Wad2Chunks.SuggestedGameVersion)
                     {
                         wad.SuggestedGameVersion = (WadGameVersion)chunkIO.ReadChunkLong(chunkSize);
                         return true;
                     }
                     if (LoadTextures(chunkIO, id, wad, ref textures))
                         return true;
                     else if (LoadSamples(chunkIO, id, wad, ref samples, obsolete))
                         return true;
                     else if (LoadSoundInfos(chunkIO, id, wad, ref soundInfos, samples))
                         return true;
                     else if (LoadFixedSoundInfos(chunkIO, id, wad, soundInfos))
                         return true;
                     else if (LoadAdditionalSoundInfos(chunkIO, id, wad, soundInfos, samples))
                         return true;
                     else if (LoadSprites(chunkIO, id, wad, ref sprites))
                         return true;
                     else if (LoadSpriteSequences(chunkIO, id, wad, sprites))
                         return true;
                     else if (LoadMoveables(chunkIO, id, wad, soundInfos, textures))
                         return true;
                     else if (LoadStatics(chunkIO, id, wad, textures))
                         return true;
                     return false;
                 });
             }
         }

         private static bool LoadSamples(ChunkReader chunkIO, ChunkId idOuter, ref Dictionary<long, WadSample> outSamples, bool obsolete)
         {
             if (idOuter != Wad2Chunks.Samples)
                 return false;

             var samples = new Dictionary<long, WadSample>();
             long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.

             chunkIO.ReadChunks((id, chunkSize) =>
             {
                 if (id != Wad2Chunks.Sample)
                     return false;

                 string FilenameObsolete = null;
                 byte[] data = null;

                 chunkIO.ReadChunks((id2, chunkSize2) =>
                 {
                     if (id2 == Wad2Chunks.SampleIndex)
                         obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                     else if (id2 == Wad2Chunks.SampleFilenameObsolete)
                         FilenameObsolete = chunkIO.ReadChunkString(chunkSize2);
                     else if (id2 == Wad2Chunks.SampleData)
                         data = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                     else
                         return false;
                     return true;
                 });

                 if (data == null && !string.IsNullOrEmpty(FilenameObsolete))
                 {
                     string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Sounds\\TR4\\Samples", FilenameObsolete + ".wav");
                     data = File.ReadAllBytes(fullPath);
                 }

                 samples.Add(obsoleteIndex++, new WadSample(WadSample.ConvertSampleFormat(data,
                     sampleRate => obsolete ?
                         new WadSample.ResampleInfo { Resample = false, SampleRate = WadSample.GameSupportedSampleRate } :
                         new WadSample.ResampleInfo { Resample = true, SampleRate = sampleRate })));
                 return true;
             });

             outSamples = samples;
             return true;
         }

         private static bool LoadSoundInfo(ChunkReader chunkIO, Wad2 wad, Dictionary<long, WadSample> samples,
                                           out WadSoundInfoMetaData soundInfo, out long index)
         {
             var tempSoundInfo = new WadSoundInfoMetaData("Unnamed");
             long tempIndex = 0;
             chunkIO.ReadChunks((id2, chunkSize2) =>
             {
                 if (id2 == Wad2Chunks.SoundInfoIndex)
                     tempIndex = chunkIO.ReadChunkLong(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoVolume)
                     tempSoundInfo.Volume = chunkIO.ReadChunkFloat(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoRange)
                     tempSoundInfo.RangeInSectors = chunkIO.ReadChunkFloat(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoPitch)
                     tempSoundInfo.PitchFactor = chunkIO.ReadChunkFloat(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoChance)
                     tempSoundInfo.Chance = chunkIO.ReadChunkFloat(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoDisablePanning)
                     tempSoundInfo.DisablePanning = chunkIO.ReadChunkBool(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoRandomizePitch)
                     tempSoundInfo.RandomizePitch = chunkIO.ReadChunkBool(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoRandomizeVolume)
                     tempSoundInfo.RandomizeVolume = chunkIO.ReadChunkBool(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoLoopBehaviour)
                     tempSoundInfo.LoopBehaviour = (WadSoundLoopBehaviour)(3 & chunkIO.ReadChunkByte(chunkSize2));
                 else if (id2 == Wad2Chunks.SoundInfoName || id2 == Wad2Chunks.SoundInfoNameObsolete)
                     tempSoundInfo.Name = chunkIO.ReadChunkString(chunkSize2);
                 else if (id2 == Wad2Chunks.SoundInfoSampleIndex)
                     tempSoundInfo.Samples.Add(samples[chunkIO.ReadChunkInt(chunkSize2)]);
                 else
                     return false;
                 return true;
             });

             if (string.IsNullOrWhiteSpace(tempSoundInfo.Name))
                 throw new InvalidDataException("Sound name can't be empty");

             index = tempIndex;
             soundInfo = tempSoundInfo;

             return true;
         }

         private static bool LoadSoundInfos(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadSoundInfo> outSoundInfos, Dictionary<long, WadSample> samples)
         {
             if (idOuter != Wad2Chunks.SoundInfos)
                 return false;

             var soundInfos = new Dictionary<long, WadSoundInfo>();
             chunkIO.ReadChunks((id, chunkSize) =>
             {
                 if (id != Wad2Chunks.SoundInfo)
                     return false;

                 WadSoundInfoMetaData soundInfo;
                 long index;
                 LoadSoundInfo(chunkIO, wad, samples, out soundInfo, out index);
                 soundInfos.Add(index, new WadSoundInfo(soundInfo));

                 return true;
             });

             outSoundInfos = soundInfos;
             return true;
         }

     }*/
}
