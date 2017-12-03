/****************************************************************************
*                                                                           *
* SM64Paint - A vertex painting tool for SM64                               *
* https://www.YouTube.com/Trenavix/                                         *
* Copyright (C) 2017 Trenavix. All rights reserved.                         *
*                                                                           *
* License:                                                                  *
* GNU/GPLv2 http://www.gnu.org/licenses/gpl-2.0.html                        *
*                                                                           *
****************************************************************************/

using OpenTK;
using System;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;

public class LevelScripts
{
    public static readonly UInt32 LVLSCRIPTSTART = 0x108A10;
    public static uint SelectedLevel = 12;//castle grounds default
    public static bool ExitDecode = false;
    public static bool GeoFound = false;
    public static uint[] GeoLayoutOffsets = new uint[0];
    static uint NewSegment;
    static uint NewSegAddr;
    static UInt32 JumpSegAddr;
    static uint JumpAddr;
    static uint BeginMIO0;
    static List<int> PrevAddr = new List<int>();
    static List<uint> LoadAddresses = new List<uint>();
    public static uint ExtCollisionPointer = 0; //Needed to repoint for extended roms
    public static uint Ext0EBankEnd = 0;

    public static void ParseLevelScripts(ROM SM64ROM, UInt32 offset)
    {
        GeoLayoutOffsets = new uint[0];
        DecodeLevelScripts(SM64ROM, offset);
    }

    public static void DecodeLevelScripts(ROM SM64ROM, UInt32 offset)
    {
        ROMManager.ReadytoLoad = false;
        for (UInt32 i = offset; i < SM64ROM.getEndROMAddr();)
        {
            uint increment = SM64ROM.getByte(i + 1);
            /*using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"e:\test4.txt", true))
            {
                file.Write(i.ToString("x") + ": ");
                for (uint j = i; j < i + increment; j++)
                {
                    file.Write(SM64ROM.getByte(j).ToString("x") + " ");
                }
                file.WriteLine("\n");
                file.Close();
            }*/
            if (increment == 0 || SM64ROM.getByte(i) > 0x3C || ExitDecode) return;
            switch (SM64ROM.getByte(i))
            {
                case 0x00:
                case 0x01:
                    NewSegment = SM64ROM.getByte(i + 3);
                    NewSegAddr = SM64ROM.ReadFourBytes(i + 4);
                    SM64ROM.setSegment(NewSegment, NewSegAddr);
                    //if (NewSegment != 0x0E) break;
                    JumpSegAddr = SM64ROM.ReadFourBytes(i + 12);
                    JumpAddr = SM64ROM.readSegmentAddr(JumpSegAddr);
                    
                    //i = JumpAddr -increment;
                    LevelScripts.DecodeLevelScripts(SM64ROM, JumpAddr);
                    break;
                case 0x02:
                    if (!GeoFound) break;
                    ExitDecode = true;
                    GeoFound = false;
                    ROMManager.ReadytoLoad = true;
                    return;
                case 0x03:
                    //delayframes
                    break;
                case 0x04:
                    //delayframes
                    break;
                case 0x05:
                    JumpSegAddr = SM64ROM.ReadFourBytes(i + 4);
                    //i = SM64ROM.readSegmentAddr(JumpSegAddr)-increment;
                    LevelScripts.DecodeLevelScripts(SM64ROM, SM64ROM.readSegmentAddr(JumpSegAddr));
                    break;
                case 0x06:
                    JumpSegAddr = SM64ROM.ReadFourBytes(i + 4);
                    //i = SM64ROM.readSegmentAddr(JumpSegAddr) - increment;
                    LevelScripts.DecodeLevelScripts(SM64ROM, SM64ROM.readSegmentAddr(JumpSegAddr));
                    break;
                case 0x07:
                    return;
                case 0x0C:
                    if (SM64ROM.getByte(i + 7) != SelectedLevel+4) break; //num is level ID
                    JumpSegAddr = SM64ROM.ReadFourBytes(i + 8);
                    LevelScripts.DecodeLevelScripts(SM64ROM, SM64ROM.readSegmentAddr(JumpSegAddr));
                    return;
                case 0x16:
                    //Loads directly to ram, no segment
                    break;
                case 0x17:
                    NewSegment = SM64ROM.getByte(i + 3);
                    NewSegAddr = SM64ROM.ReadFourBytes(i + 4);
                    SM64ROM.setSegment(NewSegment, NewSegAddr);
                    if (NewSegment == 0x0E) Ext0EBankEnd = SM64ROM.ReadFourBytes(i + 8);
                    break;
                case 0x18:
                    //MIO0 segment
                case 0x1A:
                    //MIO0 segment
                    NewSegment = SM64ROM.getByte(i + 3);
                    BeginMIO0 = SM64ROM.ReadFourBytes(i + 4);
                    NewSegAddr = SM64ROM.ReadFourBytes(BeginMIO0 + 12) + BeginMIO0;
                    SM64ROM.setSegment(NewSegment, NewSegAddr);
                    break;
                case 0x1F:
                    //LEVEL MODEL
                    uint SegAddr = SM64ROM.ReadFourBytes(i + 4);
                    if (SM64ROM.getByte(i + 4) == 0x0E || SM64ROM.getByte(i + 4) == 0x19) //Level bank
                    {
                        Array.Resize(ref GeoLayoutOffsets, GeoLayoutOffsets.Length+1);
                        GeoLayoutOffsets[SM64ROM.getByte(i + 2)-1] = SM64ROM.readSegmentAddr(SegAddr);
                        GeoFound = true;
                        if (SelectedLevel == 21) return; // End Cake Loop evasion
                        break;
                    }
                    break;
                case 0x21:
                    //obj model w/o geolayout
                    break;
                case 0x22:
                    //obj model w/ geolayout
                    break;
                case 0x2E:
                    ExtCollisionPointer = i; //Needed to repoint for extended roms
                    break;
                default:
                    break;
            }
            i += increment;
        }
    }

    public static uint getGeoAddress(uint index)
    {
        return GeoLayoutOffsets[index];
    }

    public static String[] getLevelList()
    {
        String[] List = new String[33];
        List[0] = "Haunted House";
        List[1] = "Cool Cool Mountain";
        List[2] = "Inside Castle";
        List[3] = "Hazy Maze Cave";
        List[4] = "Shifting Sand Land";
        List[5] = "Bob-Omb Battlefield";
        List[6] = "Snow Man's Land";
        List[7] = "Wet Dry World";
        List[8] = "Jolly Roger Bay";
        List[9] = "Tiny Huge Island";
        List[10] = "Tick Tock Clock";
        List[11] = "Rainbow Ride";
        List[12] = "Castle Grounds";
        List[13] = "Bowser Course 1";
        List[14] = "Vanish Cap";
        List[15] = "Bowser Course 2";
        List[16] = "Secret Aquarium";
        List[17] = "Bowser Course 2";
        List[18] = "Lethal Lava Land";
        List[19] = "Dire Dire Docks";
        List[20] = "Whomp's Fortress";
        List[21] = "End Cake Picture";
        List[22] = "Castle Courtyard";
        List[23] = "Princess' Secret Slide";
        List[24] = "Metal Cap";
        List[25] = "Wing Cap";
        List[26] = "Bowser Battle 1";
        List[27] = "Rainbow Clouds Bonus";
        List[28] = "Unused";
        List[29] = "Bowser Battle 2";
        List[30] = "Bowser Battle 3";
        List[31] = "Unused";
        List[32] = "Tall Tall Mountain";
        return List;
    }

}

