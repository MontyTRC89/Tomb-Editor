using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using System.IO;
using System.Media;
using TombLib.Wad;
using NLog;

namespace TombEditor
{
    public partial class FormSound : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private SoundSourceInstance _soundSource;
        private Wad _wad;

        private static string[] officialSoundNames = new string[] { "LARA_FEET", "LARA_CLIMB2", "LARA_NO", "LARA_SLIPPING", "LARA_LAND",
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

        public FormSound(SoundSourceInstance soundSource, Wad wad)
        {
            _soundSource = soundSource;
            _wad = wad;

            InitializeComponent();
            
            using (StreamReader reader = new StreamReader(new FileStream("Sounds\\Sounds.txt", FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                List<RowSoundSample> rows = new List<RowSoundSample>();
                short id = 0;
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split(':');
                    string file = "";

                    if (tokens.Length > 1)
                    {
                        string temp = tokens[1].Trim(' ', '\t');
                        tokens = temp.Split(' ', '\t');
                        file = tokens[0];
                    }
                    
                    rows.Add(new RowSoundSample { ID = id++, File = file });
                }

                lstSamples.SetObjects(rows);
                tbSound.Text = rows.FirstOrDefault(row => row.ID == _soundSource.SoundId).Name;
            }

            cbBit1.Checked = (_soundSource.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_soundSource.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_soundSource.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_soundSource.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_soundSource.CodeBits & (1 << 4)) != 0;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void butOK_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;

            _soundSource.SoundId = row.ID;
            byte codeBits = 0;
            codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 1) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 2) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 3) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 4) : 0);
            _soundSource.CodeBits = codeBits;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;

            if (row.File != "")
            {
                try
                {
                    SoundPlayer player = new SoundPlayer("Sounds\\Samples\\" + row.File + ".wav");
                    player.Play();
                }
                catch (Exception exc)
                {
                    logger.Warn(exc, "Unable to play sample \"" + row.File + "\"");
                }
            }
        }

        private void lstSamples_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;
            tbSound.Text = row.Name;
        }

        private struct RowSoundSample
        {
            public short ID { get; set; }
            public string Name => officialSoundNames[ID];
            public string File { get; set; }
        }
    }
}
