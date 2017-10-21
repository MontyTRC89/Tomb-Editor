using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using TombLib.Utils;
using TombLib.Graphics;
using SharpDX.Toolkit.Graphics;
using System.Drawing;

namespace TombLib.Wad
{
    public class ComparerWadTextures : IComparer<WadTexture>
    {
        public int Compare(WadTexture x, WadTexture y)
        {
            if (x == null || y == null)
                return 0;

            return -x.Height.CompareTo(y.Height);
        }
    }

    public partial class Wad2 : IDisposable
    {
        public Dictionary<Hash, WadTexture> Textures { get; private set; }
        public Dictionary<Hash, WadMesh> Meshes { get; private set; }
        public SortedDictionary<uint, WadMoveable> Moveables { get; private set; }
        public SortedDictionary<uint, WadStatic> Statics { get; private set; }
        public SortedDictionary<ushort, WadSoundInfo> SoundInfo { get; private set; }
        public Dictionary<Hash, WadSound> WaveSounds { get; private set; }
        public List<WadSpriteSequence> SpriteSequences { get; private set; }
        public Dictionary<Hash, WadSprite> SpriteTextures { get; private set; }
        public string FileName { get; set; }

        // Data for rendering
        public GraphicsDevice GraphicsDevice { get; set; }
        public Texture2D DirectXTexture { get; private set; }
        public SortedDictionary<uint, SkinnedModel> DirectXMoveables { get; } = new SortedDictionary<uint, SkinnedModel>();
        public SortedDictionary<uint, StaticModel> DirectXStatics { get; } = new SortedDictionary<uint, StaticModel>();
        public List<WadTexture> PackedTextures { get; set; } = new List<WadTexture>();

        // Size of the atlas
        public const int TextureAtlasSize = 2048;

        private static string[] _officialSoundNames = new string[] { "LARA_FEET", "LARA_CLIMB2", "LARA_NO", "LARA_SLIPPING", "LARA_LAND",
            "LARA_CLIMB1", "LARA_DRAW", "LARA_HOLSTER", "LARA_FIRE", "LARA_RELOAD", "LARA_RICOCHET", "PUSH_BLOCK_END", "METAL_SCRAPE_LOOP",
            "SMALL_SCARAB_FEET", "BIG_SCARAB_ATTACK", "BIG_SCARAB_DEATH", "BIG_SCARAB_FLYING", "LARA_WET_FEET", "LARA_WADE", "RUMBLE_LOOP",
            "METAL_SCRAPE_LOOP", "CRICKET_LOOP", "WOOD_BRIDGE_FALL", "STARGATE_SWIRL", "LARA_KNEES_SHUFFLE", "PUSH_SPX_SWITCH", "LARA_CLIMB3",
            "Don't_Use_This_Space", "LARA_SHIMMY2", "LARA_JUMP", "LARA_FALL", "LARA_INJURY", "LARA_ROLL", "LARA_SPLASH", "LARA_GETOUT", "LARA_SWIM",
            "LARA_BREATH", "LARA_BUBBLES", "SPINNING_PUZZLE", "LARA_KEY", "COG_RESAW_LIBRARY", "LARA_GENERAL_DEATH", "LARA_KNEES_DEATH",
            "LARA_UZI_FIRE", "LARA_UZI_STOP", "LARA_SHOTGUN", "LARA_BLOCK_PUSH1", "LARA_BLOCK_PUSH2", "SARLID_PALACES", "LARA_SHOTGUN_SHELL",
            "UNDERWATER_DOOR", "LARA_BLKPULL", "LARA_FLOATING", "LARA_FALLDETH", "LARA_GRABHAND", "LARA_GRABBODY", "LARA_GRABFEET", "RATCHET_3SHOT",
            "RATCHET_1SHOT", "WATER_LOOP_NOTINUSE", "UNDERWATER", "UNDERWATER_SWITCH", "LARA_PICKUP", "PUSHABLE_SOUND", "DOOR_GENERAL",
            "HELICOPTER_LOOP", "ROCK_FALL_CRUMBLE", "ROCK_FALL_LAND", "PENDULUM_BLADES", "STALEGTITE", "LARA_THUD", "GENERIC_SWOOSH",
            "GENERIC_HEAVY_THUD", "CROC_FEET", "SWINGING_FLAMES", "STONE_SCRAPE", "BLAST_CIRCLE", "BAZOOKA_FIRE", "HECKLER&KOCH_FIRE",
            "WATERFALL_LOOP", "CROC_ATTACK", "CROC_DEATH", "PORTCULLIS_UP", "PORTCULLIS_DOWN", "DOUBLE_DOORS_BANG", "DOUBLE_DOORS_CREAK",
            "PETES_PYRA_STONE", "PETES_PYRA_PNEU", "AHMET_DIE", "AHMET_ATTACK", "AHMET_HANDS", "AHMET_FEET", "AHMET_SWIPE", "AHMET_WAIT", "GUIDE_JUMP",
            "GENERAL_FOOTSTEPS1", "GUIDE_LAND_USENOT", "POUR", "SCALE1", "SCALE2", "BEETLARA_WINDUP", "BEETLE_CLK_WHIRR", "BEETLE_CLK_EXP",
            "MINE_EXP_OVERLAY", "HECKLER&KOCH_STOP", "EXPLOSION1", "EXPLOSION2_VOLWAS80", "EARTHQUAKE_LOOP", "MENU_ROTATE", "MENU_SELECT",
            "Menu_Empty", "MENU_CHOOSE", "TICK_TOCK", "Menu_Empty", "MENU_COMBINE", "Menu_Empty", "MENU_MEDI", "LARA_CLIMB_WALLS_NOISE", "WATER_LOOP",
            "VONCROY_JUMP", "LOCUSTS_LOOP", "DESSERT_EAGLE_FIRE", "BOULDER_FALL", "LARA_MINI_LOAD", "LARA_MINI_LOCK", "LARA_MINI_FIRE", "GATE_OPENING",
            "LARA_ELECTRIC_LOOP", "LARA_ELECTRIC_CRACKLES", "BLOOD_LOOP", "BIKE_START", "BIKE_IDLE", "BIKE_ACCELERATE", "BIKE_MOVING",
            "BIKE_SIDE_IMPACT", "BIKE_FRONT_IMPACT", "SOFT_WIND_LOOP", "BIKE_LAND", "CROCGOD_ROAR", "CROCGOD_WINGS", "CROCGOD_LAND",
            "CROCGOD_FIRE_ROAR", "BIKE_STOP", "GENERIC_BODY_SLAM", "HECKER&KOCH_OVERLAY", "LARA_SPIKE_DEATH", "LARA_DEATH3", "ROLLING_BALL",
            "BLK_PLAT_RAISE&LOW", "RUMBLE_NEXTDOOR", "LOOP_FOR_SMALL_FIRES", "CHAINS_LIBRARY", "JEEP_START", "JEEP_IDLE", "JEEP_ACCELERATE",
            "JEEP_MOVE", "JEEP_STOP", "BATS_1", "ROLLING_DOOR", "LAUNCHER_1", "LAUNCHER_2", "TRAPDOOR_OPEN", "TRAPDOOR_CLOSE", "Empty",
            "BABOON_STAND_WAIT", "BABOON_ATTACK_LOW", "BABOON_ATTACK_JUMP", "BABOON_JUMP", "BABOON_DEATH", "BAT_SQUEAL_FULL", "BAT_SQK", "BAT_FLAP",
            "SPHINX_NOSE_RASP", "SPHINX_WALK", "SPHINX_NOISE", "DOG_HOWL", "DOG_HIT_GROUND", "FOUNTAIN_LOOP", "DOG_FOOT_1", "DOG_JUMP", "DOG_BITE",
            "DOG_DEATH", "THUNDER_RUMBLE", "THUNDER_CRACK", "WRAITH_WHISPERS", "Empty", "Empty", "SKEL_FOOTSTEPS", "SKEL_ATTACK",
            "GENERIC_SWORD_SWOOSH", "SKEL_SWORD_CLANG", "SKEL_STICK_GROUND", "GEN_PULL_SWORD", "SKEL_LAND_HEAVY", "GUIDE_SCARE", "JEEP_DOOR_OPEN",
            "JEEP_DOOR_CLOSE", "ELEC_ARCING_LOOP", "ELEC_ONE_SHOT", "Empty", "LIBRARY_COG_LOOP", "JEEP_SIDE_IMPACT", "JEEP_FRONT_IMPACT", "JEEP_LAND",
            "SPINNING_GEM_SLOTS", "RUMMBLE", "WARTHOG_HEADBUTT", "WARTHOG_DEATH", "SET_SPIKE_TIMER", "WARTHOG_SQUEAL", "WARTHOG_FEET", "WARTHOG_GRUNT",
            "SAVE_CRYSTAL", "HORSE_RICOCHETS", "METAL_SHUTTERS_SMASH", "GEM_DROP_ON_FLOOR", "SCORPION_SCREAM", "SCORPION_FEET", "SCORPION_CLAWS",
            "SCORPION_TAIL_WHIP", "SCORPION_SMALL_FEET", "METAL GATE OPEN", "HORSE_TROTTING", "KN_TEMPLAR_WALK", "KN_TEMPLAR_GURGLES",
            "KN_SWORD_SCRAPE", "KN_TEMPLAR_ATTACK", "KN_SWORD_CLANG", "KN_SWORD_SWOOSH", "MUMMY_ATTACK", "MUMMY_WALK", "MUMMY_GURGLES",
            "MUMMY_TAKE_HIT", "SMALL_FAN", "LARGE_FAN", "LARA_CROSSBOW", "SMALL_CREATURE_FEET", "SAS_GADGIE_DIE", "WATER_FLUSHES", "GUID_ZIPPO",
            "LEAP_SWITCH", "OLD_SWITCH", "DEMIGODS_FEET", "DEMIGODS_BULL_SNORT", "DEMIGODS_BULL_HAMMER", "DEMIGODS_S_WAVE_RUMB", "DEMIGOD_WEAP_SWOOSH",
            "DEMIGOD_FALCON_SQUEAL", "DEMIGOD_FALCON_PLAS", "DEMIGOD_RISE", "DEMI_TUT_PLASMA_SPRAY", "DEMI_SIREN_SWAVE", "DEMIGODS_TUT_GROWL",
            "JOBY_ELECTRIC_INSERT", "BAD_LAND", "DOOR_GEN_THUD", "BAD_GRUNTS", "BAD_DIE", "BAD_JUMP", "BAD_TROOP_STUN", "BAD_SWORDAWAY",
            "BAD_TROOP_UZI", "BAD_SWORD_RICO", "BAD_TROOP_UZI_END", "TROOP_SCORP_CRIES", "SAS_TROOP_FEET", "GENERIC_NRG_CHARGE", "SAS_MG_FIRE",
            "HAMMER_HEAD_WADE", "SMALL_SWITCH", "Empty", "SIREN_WING_FLAP", "SIREN_NOIZES", "SIREN_ATTACK", "SIREN_DEATH", "SIREN_GEN_NOISES",
            "SETT_SIREN_PLASMA", "HAMMER_HEAD_ATK", "SMALL_DOOR_SUBWAY", "TRAIN_DOOR_OPEN", "TRAIN_DOOR_CLOSE", "VONCROY_KNIFE_SWISH",
            "TRAIN_UNLINK_BREAK", "OBJ_BOX_HIT", "OBJ_BOX_HIT_CHANCE", "OBJ_GEM_SMASH", "CATBLADES_DRAW", "SWIRLY_LONG_MOVE_SFX", "FOOTSTEPS_MUD",
            "HORSEMAN_HORSE_NEIGH", "FOOTSTEPS_GRAVEL", "FOOTSTEPS_SAND_&_GRASS", "FOOTSTEPS_WOOD", "FOOTSTEPS_MARBLE", "FOOTSTEPS_METAL",
            "GEN_SPHINX_DOORTHD", "SETT_PLASMA_1", "SETT_BOLT_1", "SETT_FEET", "SETT_NRG_CHARGE", "SETT_NRG_CHARGE2", "HORSEMAN_TAKEHIT",
            "HORSEMAN_WALK", "HORSEMAN_GRUNT", "HORSEMAN_FALL", "HORSEMAN_DIE", "MAPPER_SWITCH_ON", "MAPPER_OPEN", "MAPPER_LAZER", "MAPPER_MOVE",
            "MAPPER_CLUNK", "BLADES_DRAW", "BLADES_CLASH_LOUD", "BLADES_CLASH_QUIET", "HAMMER_TRAP_BANG", "DOOR_BIG_STONE", "SETT_BIG_ROAR",
            "BABOON_CHATTER", "BABOON_ROLL", "SWOOSH_SWIRLY_DOUBLE", "DOOR_SETTDOOR_SQK", "DOOR_SETTDOOR_CLANK", "SETT_JUMP_ATTACK", "JOBY_BLOCK",
            "SETT_TAKE_HIT", "DART_SPITT", "LARA_CROWBAR_GEM", "CROWBAR_DOOR_OPEN", "LARA_LEVER_GEN_SQKS", "HORSEMAN_GETUP", "EXH_BASKET_OPEN",
            "EXH_MUMCOFF_OPE1", "EXH_MUMCOFF_OPE2", "EXH_MUM_JOLT", "EXH_MUMHEAD_SPIN", "EXH_MUMMY_RAHHH", "EXH_ROLLER_BLINDS", "LARA_LEVER_PART1",
            "LARA_LEVER_PART2", "LARA_POLE_CLIMB", "LARA_POLE_LOOP", "TRAP_SPIKEBALL_SPK", "LARA_PULLEY", "TEETH_SPIKES", "SAND_LOOP",
            "LARA_USE_OBJECT", "LIBRARY_COG_SQKS", "HIT_ROCK", "LARA_NO_FRENCH", "LARA_NO_JAPAN", "LARA_CROW_WRENCH", "LARA_ROPE_CREAK", "BOWLANIM",
            "SPHINX_DOOR_WOODCRACK", "BEETLE_CLK_WHIRR", "MAPPER_PYRAMID_OPEN", "LIGHT_BEAM_JOBY", "GUIDE_FIRE_LIGHT", "AUTOGUNS", "PULLEY_ANDY",
            "STEAM", "JOBY_GARAGE_DOOR", "JOBY_WIND", "SANDHAM_IN_THE_HOUSE", "SANDHAM_CONVEYS", "CRANKY_GRAPE_CRUSH", "BIKE_HIT_OBJECTS",
            "BIKE_HIT_ENEMIES", "FLAME_EMITTER", "LARA_CLICK_SWITCH" };

        public void Dispose()
        {
            DirectXTexture?.Dispose();
            DirectXTexture = null;

            foreach (var obj in DirectXMoveables.Values)
                obj.Dispose();
            DirectXMoveables.Clear();

            foreach (var obj in DirectXStatics.Values)
                obj.Dispose();
            DirectXStatics.Clear();
        }

        public Wad2()
        {
            Textures = new Dictionary<Hash, WadTexture>();
            Meshes = new Dictionary<Hash, WadMesh>();
            Moveables = new SortedDictionary<uint, WadMoveable>();
            Statics = new SortedDictionary<uint, WadStatic>();
            SoundInfo = new SortedDictionary<ushort, WadSoundInfo>();
            SpriteSequences = new List<WadSpriteSequence>();
            SpriteTextures = new Dictionary<Hash, WadSprite>();
            WaveSounds = new Dictionary<Hash, WadSound>();
        }

       // public static string[] OfficialSoundNames { get { return _officialSoundNames; } }

       /* public static ushort[] MandatorySounds
        {
            get
            {
                return new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 17, 18, 19, 24, 26, 27, 28, 29, 30, 31, 32,
                                      33, 32, 35, 36, 37, 39, 41, 42, 43, 44, 45, 46, 47, 49, 51, 52, 53, 54,
                                      55, 56, 70, 71, 72, 75, 77, 78, 82, 83, 85, 95, 105, 
                                      107, 108, 109, 111, 112, 114, 116, 117, 118, 121,
                                      123, 124, 125, 136, 143, 145, 150, 182, 189, 192,
                                      215, 220, 235, 251, 255, 269, 285, 288, 290, 291, 
                                      292, 294, 315, 326, 327, 339, 340, 345, 347, 348, 
                                      349, 350, 351, 369 };
            }
        }*/

        public void RebuildTextureAtlas()
        {
            if (DirectXTexture != null) DirectXTexture.Dispose();

            // Pack the textures in a single atlas
            PackedTextures = new List<WadTexture>();

            for (int i = 0; i < Textures.Count; i++)
            {
                PackedTextures.Add(Textures.ElementAt(i).Value);
            }

            PackedTextures.Sort(new ComparerWadTextures());

            RectPackerSimpleStack packer = new RectPackerSimpleStack(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in PackedTextures)
            {
                var point = packer.TryAdd(texture.Width, texture.Height);
                texture.PositionInPackedTexture = new Vector2(point.Value.X, point.Value.Y);
            }

            // Copy the page in a temp bitmap. 
            // I generate a texture atlas, putting all texture pages inside 2048x2048 pixel textures.
            var tempBitmap = ImageC.CreateNew(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in PackedTextures)
            {
                int startX = (int)texture.PositionInPackedTexture.X;
                int startY = (int)texture.PositionInPackedTexture.Y;

                for (int y = 0; y < texture.Height; y++)
                {
                    for (int x = 0; x < texture.Width; x++)
                    {
                        var color = texture.Image.GetPixel(x, y);
                        tempBitmap.SetPixel(startX + x, startY + y, color);
                    }
                }
            }

            // Create the DirectX texture atlas
            DirectXTexture = TextureLoad.Load(GraphicsDevice, tempBitmap);
            tempBitmap.Save("E:\\andrea1.png");
        }

        public void PrepareDataForDirectX()
        {
            Dispose();

            // Rebuild the texture atlas and covert it to a DirectX texture
            RebuildTextureAtlas();
            
            // Create movable models
            for (int i = 0; i < Moveables.Count; i++)
            {
                WadMoveable mov = Moveables.ElementAt(i).Value;
                DirectXMoveables.Add(mov.ObjectID, SkinnedModel.FromWad2(GraphicsDevice, this, mov, PackedTextures));
            }

            // Create static meshes
            for (int i = 0; i < Statics.Count; i++)
            {
                WadStatic staticMesh = Statics.ElementAt(i).Value;
                DirectXStatics.Add(staticMesh.ObjectID, StaticModel.FromWad2(GraphicsDevice, this, staticMesh, PackedTextures));
            }

            // Prepare sprites
            for (int i = 0; i < SpriteTextures.Count; i++)
            {
                var sprite = SpriteTextures.ElementAt(i).Value;
                sprite.DirectXTexture = TextureLoad.Load(GraphicsDevice, sprite.Image);
            }
        }
    }
}
