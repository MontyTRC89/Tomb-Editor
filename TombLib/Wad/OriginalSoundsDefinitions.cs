using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class OriginalSoundsDefinitions
    {
        public static List<OriginalSound> Sounds { get; private set; }

        public static List<string> SoundNames
        {
            get
            {
                return Sounds.Select(x => x.Name).ToList();
            }
        }

        public static void LoadSounds(Stream stream)
        {
            Sounds = new List<OriginalSound>();

            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var s = reader.ReadLine().Trim();

                    var sound = new OriginalSound();

                    sound.Range = 10;

                    int endOfSoundName = s.IndexOf(':');
                    if (endOfSoundName == -1)
                    {
                        sound.Name = s;
                        sound.Unused = true;
                        Sounds.Add(sound);
                        continue;
                    }

                    sound.Name = s.Substring(0, s.IndexOf(':'));

                    var tokens = s.Split(' ', '\t');
                    if (tokens.Length == 1)
                    {
                        sound.Unused = true;
                        Sounds.Add(sound);
                        continue;
                    }

                    int currentToken = 1;

                    do
                    {
                        var token = tokens[currentToken];

                        if (token == "")
                        {
                            currentToken++;
                            continue;
                        }

                        short volume;
                        short chance;

                        if (token.Length == 1)
                        {
                            if (token == "P")
                            {
                                sound.FlagP = true;
                                sound.Flags |= (0x2000);
                            }

                            if (token == "V")
                            {
                                sound.FlagV = true;
                                sound.Flags |= (0x4000);
                            }

                            if (token == "N")
                            {
                                sound.FlagN = true;
                                sound.Flags |= (0x1000);
                            }

                            if (token == "L")
                            {
                                sound.FlagL = true;
                                sound.Flags |= (0x03);
                            }

                            if (token == "R")
                            {
                                sound.FlagP = true;
                                sound.Flags |= (0x02);
                            }

                            if (token == "W")
                            {
                                sound.FlagP = true;
                                sound.Flags |= (0x01);
                            }
                        }
                        else if (!token.StartsWith("VOL") || !Int16.TryParse(token.Substring(3), out volume))
                        {
                            if (!token.StartsWith("CH") || !Int16.TryParse(token.Substring(2), out chance))
                            {
                                if (!token.StartsWith("PIT"))
                                {
                                    if (!token.StartsWith("RAD"))
                                    {
                                        if (token[0] == '#')
                                        {
                                            sound.WadLetters.Add(token.Substring(1));
                                            if (token[1] == 'g') sound.MandatorySound = true;
                                        }
                                        else
                                        {
                                            // If I'm here, then it's a sample
                                            sound.Samples.Add(token);
                                        }
                                    }
                                    else
                                    {
                                        sound.Range = Int16.Parse(token.Substring(3));
                                    }
                                }
                                else
                                {
                                    sound.Pitch = Int16.Parse(token.Substring(3));
                                }
                            }
                            else
                            {
                                sound.Chance = Int16.Parse(token.Substring(2));
                            }
                        }
                        else
                        {
                            sound.Volume = Int16.Parse(token.Substring(3));
                        }

                        currentToken++;
                    }
                    while (currentToken < tokens.Length);

                    Sounds.Add(sound);
                }
            }
        }
    }
}
