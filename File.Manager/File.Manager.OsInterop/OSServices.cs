﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace File.Manager.OsInterop
{
    public class OSServices
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            public string szDisplayName;
            public string szTypeName;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        public static class KnownFolderId
        {
            public static readonly Guid AccountPictures = new(0x008CA0B1, 0x55B4, 0x4C56, 0xB8, 0xA8, 0x4D, 0xE4, 0xB2, 0x99, 0xD3, 0xBE);
            public static readonly Guid AddNewPrograms = new(0xDE61D971, 0x5EBC, 0x4F02, 0xA3, 0xA9, 0x6C, 0x82, 0x89, 0x5E, 0x5C, 0x04);
            public static readonly Guid AdminTools = new(0x724EF170, 0xA42D, 0x4FEF, 0x9F, 0x26, 0xB6, 0x0E, 0x84, 0x6F, 0xBA, 0x4F);
            public static readonly Guid AllAppMods = new(0x7AD67899, 0x66AF, 0x43BA, 0x91, 0x56, 0x6A, 0xAD, 0x42, 0xE6, 0xC5, 0x96);
            public static readonly Guid AppCaptures = new(0xEDC0FE71, 0x98D8, 0x4F4A, 0xB9, 0x20, 0xC8, 0xDC, 0x13, 0x3C, 0xB1, 0x65);
            public static readonly Guid AppDataDesktop = new(0xB2C5E279, 0x7ADD, 0x439F, 0xB2, 0x8C, 0xC4, 0x1F, 0xE1, 0xBB, 0xF6, 0x72);
            public static readonly Guid AppDataDocuments = new(0x7BE16610, 0x1F7F, 0x44AC, 0xBF, 0xF0, 0x83, 0xE1, 0x5F, 0x2F, 0xFC, 0xA1);
            public static readonly Guid AppDataFavorites = new(0x7CFBEFBC, 0xDE1F, 0x45AA, 0xB8, 0x43, 0xA5, 0x42, 0xAC, 0x53, 0x6C, 0xC9);
            public static readonly Guid AppDataProgramData = new(0x559D40A3, 0xA036, 0x40FA, 0xAF, 0x61, 0x84, 0xCB, 0x43, 0x0A, 0x4D, 0x34);
            public static readonly Guid ApplicationShortcuts = new(0xA3918781, 0xE5F2, 0x4890, 0xB3, 0xD9, 0xA7, 0xE5, 0x43, 0x32, 0x32, 0x8C);
            public static readonly Guid AppsFolder = new(0x1E87508D, 0x89C2, 0x42F0, 0x8A, 0x7E, 0x64, 0x5A, 0x0F, 0x50, 0xCA, 0x58);
            public static readonly Guid AppUpdates = new(0xA305CE99, 0xF527, 0x492B, 0x8B, 0x1A, 0x7E, 0x76, 0xFA, 0x98, 0xD6, 0xE4);
            public static readonly Guid CameraRoll = new(0xAB5FB87B, 0x7CE2, 0x4F83, 0x91, 0x5D, 0x55, 0x08, 0x46, 0xC9, 0x53, 0x7B);
            public static readonly Guid CameraRollLibrary = new(0x2B20DF75, 0x1EDA, 0x4039, 0x80, 0x97, 0x38, 0x79, 0x82, 0x27, 0xD5, 0xB7);
            public static readonly Guid CDBurning = new(0x9E52AB10, 0xF80D, 0x49DF, 0xAC, 0xB8, 0x43, 0x30, 0xF5, 0x68, 0x78, 0x55);
            public static readonly Guid ChangeRemovePrograms = new(0xDF7266AC, 0x9274, 0x4867, 0x8D, 0x55, 0x3B, 0xD6, 0x61, 0xDE, 0x87, 0x2D);
            public static readonly Guid CommonAdminTools = new(0xD0384E7D, 0xBAC3, 0x4797, 0x8F, 0x14, 0xCB, 0xA2, 0x29, 0xB3, 0x92, 0xB5);
            public static readonly Guid CommonOEMLinks = new(0xC1BAE2D0, 0x10DF, 0x4334, 0xBE, 0xDD, 0x7A, 0xA2, 0x0B, 0x22, 0x7A, 0x9D);
            public static readonly Guid CommonPrograms = new(0x0139D44E, 0x6AFE, 0x49F2, 0x86, 0x90, 0x3D, 0xAF, 0xCA, 0xE6, 0xFF, 0xB8);
            public static readonly Guid CommonStartMenu = new(0xA4115719, 0xD62E, 0x491D, 0xAA, 0x7C, 0xE7, 0x4B, 0x8B, 0xE3, 0xB0, 0x67);
            public static readonly Guid CommonStartMenuPlaces = new(0xA440879F, 0x87A0, 0x4F7D, 0xB7, 0x00, 0x02, 0x07, 0xB9, 0x66, 0x19, 0x4A);
            public static readonly Guid CommonStartup = new(0x82A5EA35, 0xD9CD, 0x47C5, 0x96, 0x29, 0xE1, 0x5D, 0x2F, 0x71, 0x4E, 0x6E);
            public static readonly Guid CommonTemplates = new(0xB94237E7, 0x57AC, 0x4347, 0x91, 0x51, 0xB0, 0x8C, 0x6C, 0x32, 0xD1, 0xF7);
            public static readonly Guid ComputerFolder = new(0x0AC0837C, 0xBBF8, 0x452A, 0x85, 0x0D, 0x79, 0xD0, 0x8E, 0x66, 0x7C, 0xA7);
            public static readonly Guid ConflictFolder = new(0x4BFEFB45, 0x347D, 0x4006, 0xA5, 0xBE, 0xAC, 0x0C, 0xB0, 0x56, 0x71, 0x92);
            public static readonly Guid ConnectionsFolder = new(0x6F0CD92B, 0x2E97, 0x45D1, 0x88, 0xFF, 0xB0, 0xD1, 0x86, 0xB8, 0xDE, 0xDD);
            public static readonly Guid Contacts = new(0x56784854, 0xC6CB, 0x462B, 0x81, 0x69, 0x88, 0xE3, 0x50, 0xAC, 0xB8, 0x82);
            public static readonly Guid ControlPanelFolder = new(0x82A74AEB, 0xAEB4, 0x465C, 0xA0, 0x14, 0xD0, 0x97, 0xEE, 0x34, 0x6D, 0x63);
            public static readonly Guid Cookies = new(0x2B0F765D, 0xC0E9, 0x4171, 0x90, 0x8E, 0x08, 0xA6, 0x11, 0xB8, 0x4F, 0xF6);
            public static readonly Guid CurrentAppMods = new(0x3DB40B20, 0x2A30, 0x4DBE, 0x91, 0x7E, 0x77, 0x1D, 0xD2, 0x1D, 0xD0, 0x99);
            public static readonly Guid Desktop = new(0xB4BFCC3A, 0xDB2C, 0x424C, 0xB0, 0x29, 0x7F, 0xE9, 0x9A, 0x87, 0xC6, 0x41);
            public static readonly Guid DevelopmentFiles = new(0xDBE8E08E, 0x3053, 0x4BBC, 0xB1, 0x83, 0x2A, 0x7B, 0x2B, 0x19, 0x1E, 0x59);
            public static readonly Guid Device = new(0x1C2AC1DC, 0x4358, 0x4B6C, 0x97, 0x33, 0xAF, 0x21, 0x15, 0x65, 0x76, 0xF0);
            public static readonly Guid DeviceMetadataStore = new(0x5CE4A5E9, 0xE4EB, 0x479D, 0xB8, 0x9F, 0x13, 0x0C, 0x02, 0x88, 0x61, 0x55);
            public static readonly Guid Documents = new(0xFDD39AD0, 0x238F, 0x46AF, 0xAD, 0xB4, 0x6C, 0x85, 0x48, 0x03, 0x69, 0xC7);
            public static readonly Guid DocumentsLibrary = new(0x7B0DB17D, 0x9CD2, 0x4A93, 0x97, 0x33, 0x46, 0xCC, 0x89, 0x02, 0x2E, 0x7C);
            public static readonly Guid Downloads = new(0x374DE290, 0x123F, 0x4565, 0x91, 0x64, 0x39, 0xC4, 0x92, 0x5E, 0x46, 0x7B);
            public static readonly Guid Favorites = new(0x1777F761, 0x68AD, 0x4D8A, 0x87, 0xBD, 0x30, 0xB7, 0x59, 0xFA, 0x33, 0xDD);
            public static readonly Guid Fonts = new(0xFD228CB7, 0xAE11, 0x4AE3, 0x86, 0x4C, 0x16, 0xF3, 0x91, 0x0A, 0xB8, 0xFE);
            public static readonly Guid Games = new(0xCAC52C1A, 0xB53D, 0x4EDC, 0x92, 0xD7, 0x6B, 0x2E, 0x8A, 0xC1, 0x94, 0x34);
            public static readonly Guid GameTasks = new(0x054FAE61, 0x4DD8, 0x4787, 0x80, 0xB6, 0x09, 0x02, 0x20, 0xC4, 0xB7, 0x00);
            public static readonly Guid History = new(0xD9DC8A3B, 0xB784, 0x432E, 0xA7, 0x81, 0x5A, 0x11, 0x30, 0xA7, 0x59, 0x63);
            public static readonly Guid HomeGroup = new(0x52528A6B, 0xB9E3, 0x4ADD, 0xB6, 0x0D, 0x58, 0x8C, 0x2D, 0xBA, 0x84, 0x2D);
            public static readonly Guid HomeGroupCurrentUser = new(0x9B74B6A3, 0x0DFD, 0x4F11, 0x9E, 0x78, 0x5F, 0x78, 0x00, 0xF2, 0xE7, 0x72);
            public static readonly Guid ImplicitAppShortcuts = new(0xBCB5256F, 0x79F6, 0x4CEE, 0xB7, 0x25, 0xDC, 0x34, 0xE4, 0x02, 0xFD, 0x46);
            public static readonly Guid InternetCache = new(0x352481E8, 0x33BE, 0x4251, 0xBA, 0x85, 0x60, 0x07, 0xCA, 0xED, 0xCF, 0x9D);
            public static readonly Guid InternetFolder = new(0x4D9F7874, 0x4E0C, 0x4904, 0x96, 0x7B, 0x40, 0xB0, 0xD2, 0x0C, 0x3E, 0x4B);
            public static readonly Guid Libraries = new(0x1B3EA5DC, 0xB587, 0x4786, 0xB4, 0xEF, 0xBD, 0x1D, 0xC3, 0x32, 0xAE, 0xAE);
            public static readonly Guid Links = new(0xBFB9D5E0, 0xC6A9, 0x404C, 0xB2, 0xB2, 0xAE, 0x6D, 0xB6, 0xAF, 0x49, 0x68);
            public static readonly Guid LocalAppData = new(0xF1B32785, 0x6FBA, 0x4FCF, 0x9D, 0x55, 0x7B, 0x8E, 0x7F, 0x15, 0x70, 0x91);
            public static readonly Guid LocalAppDataLow = new(0xA520A1A4, 0x1780, 0x4FF6, 0xBD, 0x18, 0x16, 0x73, 0x43, 0xC5, 0xAF, 0x16);
            public static readonly Guid LocalDocuments = new(0xF42EE2D3, 0x909F, 0x4907, 0x88, 0x71, 0x4C, 0x22, 0xFC, 0x0B, 0xF7, 0x56);
            public static readonly Guid LocalDownloads = new(0x7D83EE9B, 0x2244, 0x4E70, 0xB1, 0xF5, 0x53, 0x93, 0x04, 0x2A, 0xF1, 0xE4);
            public static readonly Guid LocalizedResourcesDir = new(0x2A00375E, 0x224C, 0x49DE, 0xB8, 0xD1, 0x44, 0x0D, 0xF7, 0xEF, 0x3D, 0xDC);
            public static readonly Guid LocalMusic = new(0xA0C69A99, 0x21C8, 0x4671, 0x87, 0x03, 0x79, 0x34, 0x16, 0x2F, 0xCF, 0x1D);
            public static readonly Guid LocalPictures = new(0x0DDD015D, 0xB06C, 0x45D5, 0x8C, 0x4C, 0xF5, 0x97, 0x13, 0x85, 0x46, 0x39);
            public static readonly Guid LocalVideos = new(0x35286A68, 0x3C57, 0x41A1, 0xBB, 0xB1, 0x0E, 0xAE, 0x73, 0xD7, 0x6C, 0x95);
            public static readonly Guid Music = new(0x4BD8D571, 0x6D19, 0x48D3, 0xBE, 0x97, 0x42, 0x22, 0x20, 0x08, 0x0E, 0x43);
            public static readonly Guid MusicLibrary = new(0x2112AB0A, 0xC86A, 0x4FFE, 0xA3, 0x68, 0x0D, 0xE9, 0x6E, 0x47, 0x01, 0x2E);
            public static readonly Guid NetHood = new(0xC5ABBF53, 0xE17F, 0x4121, 0x89, 0x00, 0x86, 0x62, 0x6F, 0xC2, 0xC9, 0x73);
            public static readonly Guid NetworkFolder = new(0xD20BEEC4, 0x5CA8, 0x4905, 0xAE, 0x3B, 0xBF, 0x25, 0x1E, 0xA0, 0x9B, 0x53);
            public static readonly Guid Objects3D = new(0x31C0DD25, 0x9439, 0x4F12, 0xBF, 0x41, 0x7F, 0xF4, 0xED, 0xA3, 0x87, 0x22);
            public static readonly Guid OneDrive = new(0xA52BBA46, 0xE9E1, 0x435F, 0xB3, 0xD9, 0x28, 0xDA, 0xA6, 0x48, 0xC0, 0xF6);
            public static readonly Guid OriginalImages = new(0x2C36C0AA, 0x5812, 0x4B87, 0xBF, 0xD0, 0x4C, 0xD0, 0xDF, 0xB1, 0x9B, 0x39);
            public static readonly Guid PhotoAlbums = new(0x69D2CF90, 0xFC33, 0x4FB7, 0x9A, 0x0C, 0xEB, 0xB0, 0xF0, 0xFC, 0xB4, 0x3C);
            public static readonly Guid Pictures = new(0x33E28130, 0x4E1E, 0x4676, 0x83, 0x5A, 0x98, 0x39, 0x5C, 0x3B, 0xC3, 0xBB);
            public static readonly Guid PicturesLibrary = new(0xA990AE9F, 0xA03B, 0x4E80, 0x94, 0xBC, 0x99, 0x12, 0xD7, 0x50, 0x41, 0x04);
            public static readonly Guid Playlists = new(0xDE92C1C7, 0x837F, 0x4F69, 0xA3, 0xBB, 0x86, 0xE6, 0x31, 0x20, 0x4A, 0x23);
            public static readonly Guid PrintersFolder = new(0x76FC4E2D, 0xD6AD, 0x4519, 0xA6, 0x63, 0x37, 0xBD, 0x56, 0x06, 0x81, 0x85);
            public static readonly Guid PrintHood = new(0x9274BD8D, 0xCFD1, 0x41C3, 0xB3, 0x5E, 0xB1, 0x3F, 0x55, 0xA7, 0x58, 0xF4);
            public static readonly Guid Profile = new(0x5E6C858F, 0x0E22, 0x4760, 0x9A, 0xFE, 0xEA, 0x33, 0x17, 0xB6, 0x71, 0x73);
            public static readonly Guid ProgramData = new(0x62AB5D82, 0xFDC1, 0x4DC3, 0xA9, 0xDD, 0x07, 0x0D, 0x1D, 0x49, 0x5D, 0x97);
            public static readonly Guid ProgramFiles = new(0x905E63B6, 0xC1BF, 0x494E, 0xB2, 0x9C, 0x65, 0xB7, 0x32, 0xD3, 0xD2, 0x1A);
            public static readonly Guid ProgramFilesCommon = new(0xF7F1ED05, 0x9F6D, 0x47A2, 0xAA, 0xAE, 0x29, 0xD3, 0x17, 0xC6, 0xF0, 0x66);
            public static readonly Guid ProgramFilesCommonX64 = new(0x6365D5A7, 0x0F0D, 0x45E5, 0x87, 0xF6, 0x0D, 0xA5, 0x6B, 0x6A, 0x4F, 0x7D);
            public static readonly Guid ProgramFilesCommonX86 = new(0xDE974D24, 0xD9C6, 0x4D3E, 0xBF, 0x91, 0xF4, 0x45, 0x51, 0x20, 0xB9, 0x17);
            public static readonly Guid ProgramFilesX64 = new(0x6D809377, 0x6AF0, 0x444B, 0x89, 0x57, 0xA3, 0x77, 0x3F, 0x02, 0x20, 0x0E);
            public static readonly Guid ProgramFilesX86 = new(0x7C5A40EF, 0xA0FB, 0x4BFC, 0x87, 0x4A, 0xC0, 0xF2, 0xE0, 0xB9, 0xFA, 0x8E);
            public static readonly Guid Programs = new(0xA77F5D77, 0x2E2B, 0x44C3, 0xA6, 0xA2, 0xAB, 0xA6, 0x01, 0x05, 0x4A, 0x51);
            public static readonly Guid Public = new(0xDFDF76A2, 0xC82A, 0x4D63, 0x90, 0x6A, 0x56, 0x44, 0xAC, 0x45, 0x73, 0x85);
            public static readonly Guid PublicDesktop = new(0xC4AA340D, 0xF20F, 0x4863, 0xAF, 0xEF, 0xF8, 0x7E, 0xF2, 0xE6, 0xBA, 0x25);
            public static readonly Guid PublicDocuments = new(0xED4824AF, 0xDCE4, 0x45A8, 0x81, 0xE2, 0xFC, 0x79, 0x65, 0x08, 0x36, 0x34);
            public static readonly Guid PublicDownloads = new(0x3D644C9B, 0x1FB8, 0x4F30, 0x9B, 0x45, 0xF6, 0x70, 0x23, 0x5F, 0x79, 0xC0);
            public static readonly Guid PublicGameTasks = new(0xDEBF2536, 0xE1A8, 0x4C59, 0xB6, 0xA2, 0x41, 0x45, 0x86, 0x47, 0x6A, 0xEA);
            public static readonly Guid PublicLibraries = new(0x48DAF80B, 0xE6CF, 0x4F4E, 0xB8, 0x00, 0x0E, 0x69, 0xD8, 0x4E, 0xE3, 0x84);
            public static readonly Guid PublicMusic = new(0x3214FAB5, 0x9757, 0x4298, 0xBB, 0x61, 0x92, 0xA9, 0xDE, 0xAA, 0x44, 0xFF);
            public static readonly Guid PublicPictures = new(0xB6EBFB86, 0x6907, 0x413C, 0x9A, 0xF7, 0x4F, 0xC2, 0xAB, 0xF0, 0x7C, 0xC5);
            public static readonly Guid PublicRingtones = new(0xE555AB60, 0x153B, 0x4D17, 0x9F, 0x04, 0xA5, 0xFE, 0x99, 0xFC, 0x15, 0xEC);
            public static readonly Guid PublicUserTiles = new(0x0482AF6C, 0x08F1, 0x4C34, 0x8C, 0x90, 0xE1, 0x7E, 0xC9, 0x8B, 0x1E, 0x17);
            public static readonly Guid PublicVideos = new(0x2400183A, 0x6185, 0x49FB, 0xA2, 0xD8, 0x4A, 0x39, 0x2A, 0x60, 0x2B, 0xA3);
            public static readonly Guid QuickLaunch = new(0x52A4F021, 0x7B75, 0x48A9, 0x9F, 0x6B, 0x4B, 0x87, 0xA2, 0x10, 0xBC, 0x8F);
            public static readonly Guid Recent = new(0xAE50C081, 0xEBD2, 0x438A, 0x86, 0x55, 0x8A, 0x09, 0x2E, 0x34, 0x98, 0x7A);
            public static readonly Guid RecordedCalls = new(0x2F8B40C2, 0x83ED, 0x48EE, 0xB3, 0x83, 0xA1, 0xF1, 0x57, 0xEC, 0x6F, 0x9A);
            public static readonly Guid RecordedTVLibrary = new(0x1A6FDBA2, 0xF42D, 0x4358, 0xA7, 0x98, 0xB7, 0x4D, 0x74, 0x59, 0x26, 0xC5);
            public static readonly Guid RecycleBinFolder = new(0xB7534046, 0x3ECB, 0x4C18, 0xBE, 0x4E, 0x64, 0xCD, 0x4C, 0xB7, 0xD6, 0xAC);
            public static readonly Guid ResourceDir = new(0x8AD10C31, 0x2ADB, 0x4296, 0xA8, 0xF7, 0xE4, 0x70, 0x12, 0x32, 0xC9, 0x72);
            public static readonly Guid RetailDemo = new(0x12D4C69E, 0x24AD, 0x4923, 0xBE, 0x19, 0x31, 0x32, 0x1C, 0x43, 0xA7, 0x67);
            public static readonly Guid Ringtones = new(0xC870044B, 0xF49E, 0x4126, 0xA9, 0xC3, 0xB5, 0x2A, 0x1F, 0xF4, 0x11, 0xE8);
            public static readonly Guid RoamedTileImages = new(0xAAA8D5A5, 0xF1D6, 0x4259, 0xBA, 0xA8, 0x78, 0xE7, 0xEF, 0x60, 0x83, 0x5E);
            public static readonly Guid RoamingAppData = new(0x3EB685DB, 0x65F9, 0x4CF6, 0xA0, 0x3A, 0xE3, 0xEF, 0x65, 0x72, 0x9F, 0x3D);
            public static readonly Guid RoamingTiles = new(0x00BCFC5A, 0xED94, 0x4E48, 0x96, 0xA1, 0x3F, 0x62, 0x17, 0xF2, 0x19, 0x90);
            public static readonly Guid SampleMusic = new(0xB250C668, 0xF57D, 0x4EE1, 0xA6, 0x3C, 0x29, 0x0E, 0xE7, 0xD1, 0xAA, 0x1F);
            public static readonly Guid SamplePictures = new(0xC4900540, 0x2379, 0x4C75, 0x84, 0x4B, 0x64, 0xE6, 0xFA, 0xF8, 0x71, 0x6B);
            public static readonly Guid SamplePlaylists = new(0x15CA69B3, 0x30EE, 0x49C1, 0xAC, 0xE1, 0x6B, 0x5E, 0xC3, 0x72, 0xAF, 0xB5);
            public static readonly Guid SampleVideos = new(0x859EAD94, 0x2E85, 0x48AD, 0xA7, 0x1A, 0x09, 0x69, 0xCB, 0x56, 0xA6, 0xCD);
            public static readonly Guid SavedGames = new(0x4C5C32FF, 0xBB9D, 0x43B0, 0xB5, 0xB4, 0x2D, 0x72, 0xE5, 0x4E, 0xAA, 0xA4);
            public static readonly Guid SavedPictures = new(0x3B193882, 0xD3AD, 0x4EAB, 0x96, 0x5A, 0x69, 0x82, 0x9D, 0x1F, 0xB5, 0x9F);
            public static readonly Guid SavedPicturesLibrary = new(0xE25B5812, 0xBE88, 0x4BD9, 0x94, 0xB0, 0x29, 0x23, 0x34, 0x77, 0xB6, 0xC3);
            public static readonly Guid SavedSearches = new(0x7D1D3A04, 0xDEBB, 0x4115, 0x95, 0xCF, 0x2F, 0x29, 0xDA, 0x29, 0x20, 0xDA);
            public static readonly Guid Screenshots = new(0xB7BEDE81, 0xDF94, 0x4682, 0xA7, 0xD8, 0x57, 0xA5, 0x26, 0x20, 0xB8, 0x6F);
            public static readonly Guid SearchCsc = new(0xEE32E446, 0x31CA, 0x4ABA, 0x81, 0x4F, 0xA5, 0xEB, 0xD2, 0xFD, 0x6D, 0x5E);
            public static readonly Guid SearchMapi = new(0x98EC0E18, 0x2098, 0x4D44, 0x86, 0x44, 0x66, 0x97, 0x93, 0x15, 0xA2, 0x81);
            public static readonly Guid SearchHistory = new(0x0D4C3DB6, 0x03A3, 0x462F, 0xA0, 0xE6, 0x08, 0x92, 0x4C, 0x41, 0xB5, 0xD4);
            public static readonly Guid SearchHome = new(0x190337D1, 0xB8CA, 0x4121, 0xA6, 0x39, 0x6D, 0x47, 0x2D, 0x16, 0x97, 0x2A);
            public static readonly Guid SearchTemplates = new(0x7E636BFE, 0xDFA9, 0x4D5E, 0xB4, 0x56, 0xD7, 0xB3, 0x98, 0x51, 0xD8, 0xA9);
            public static readonly Guid SendTo = new(0x8983036C, 0x27C0, 0x404B, 0x8F, 0x08, 0x10, 0x2D, 0x10, 0xDC, 0xFD, 0x74);
            public static readonly Guid SidebarDefaultParts = new(0x7B396E54, 0x9EC5, 0x4300, 0xBE, 0x0A, 0x24, 0x82, 0xEB, 0xAE, 0x1A, 0x26);
            public static readonly Guid SidebarParts = new(0xA75D362E, 0x50FC, 0x4FB7, 0xAC, 0x2C, 0xA8, 0xBE, 0xAA, 0x31, 0x44, 0x93);
            public static readonly Guid SkyDrive = new(0xA52BBA46, 0xE9E1, 0x435F, 0xB3, 0xD9, 0x28, 0xDA, 0xA6, 0x48, 0xC0, 0xF6);
            public static readonly Guid SkyDriveCameraRoll = new(0x767E6811, 0x49CB, 0x4273, 0x87, 0xC2, 0x20, 0xF3, 0x55, 0xE1, 0x08, 0x5B);
            public static readonly Guid SkyDriveDocuments = new(0x24D89E24, 0x2F19, 0x4534, 0x9D, 0xDE, 0x6A, 0x66, 0x71, 0xFB, 0xB8, 0xFE);
            public static readonly Guid SkyDriveMusic = new(0xC3F2459E, 0x80D6, 0x45DC, 0xBF, 0xEF, 0x1F, 0x76, 0x9F, 0x2B, 0xE7, 0x30);
            public static readonly Guid SkyDrivePictures = new(0x339719B5, 0x8C47, 0x4894, 0x94, 0xC2, 0xD8, 0xF7, 0x7A, 0xDD, 0x44, 0xA6);
            public static readonly Guid StartMenu = new(0x625B53C3, 0xAB48, 0x4EC1, 0xBA, 0x1F, 0xA1, 0xEF, 0x41, 0x46, 0xFC, 0x19);
            public static readonly Guid StartMenuAllPrograms = new(0xF26305EF, 0x6948, 0x40B9, 0xB2, 0x55, 0x81, 0x45, 0x3D, 0x09, 0xC7, 0x85);
            public static readonly Guid Startup = new(0xB97D20BB, 0xF46A, 0x4C97, 0xBA, 0x10, 0x5E, 0x36, 0x08, 0x43, 0x08, 0x54);
            public static readonly Guid SyncManagerFolder = new(0x43668BF8, 0xC14E, 0x49B2, 0x97, 0xC9, 0x74, 0x77, 0x84, 0xD7, 0x84, 0xB7);
            public static readonly Guid SyncResultsFolder = new(0x289A9A43, 0xBE44, 0x4057, 0xA4, 0x1B, 0x58, 0x7A, 0x76, 0xD7, 0xE7, 0xF9);
            public static readonly Guid SyncSetupFolder = new(0x0F214138, 0xB1D3, 0x4A90, 0xBB, 0xA9, 0x27, 0xCB, 0xC0, 0xC5, 0x38, 0x9A);
            public static readonly Guid System = new(0x1AC14E77, 0x02E7, 0x4E5D, 0xB7, 0x44, 0x2E, 0xB1, 0xAE, 0x51, 0x98, 0xB7);
            public static readonly Guid SystemX86 = new(0xD65231B0, 0xB2F1, 0x4857, 0xA4, 0xCE, 0xA8, 0xE7, 0xC6, 0xEA, 0x7D, 0x27);
            public static readonly Guid Templates = new(0xA63293E8, 0x664E, 0x48DB, 0xA0, 0x79, 0xDF, 0x75, 0x9E, 0x05, 0x09, 0xF7);
            public static readonly Guid UserPinned = new(0x9E3995AB, 0x1F9C, 0x4F13, 0xB8, 0x27, 0x48, 0xB2, 0x4B, 0x6C, 0x71, 0x74);
            public static readonly Guid UserProfiles = new(0x0762D272, 0xC50A, 0x4BB0, 0xA3, 0x82, 0x69, 0x7D, 0xCD, 0x72, 0x9B, 0x80);
            public static readonly Guid UserProgramFiles = new(0x5CD7AEE2, 0x2219, 0x4A67, 0xB8, 0x5D, 0x6C, 0x9C, 0xE1, 0x56, 0x60, 0xCB);
            public static readonly Guid UserProgramFilesCommon = new(0xBCBD3057, 0xCA5C, 0x4622, 0xB4, 0x2D, 0xBC, 0x56, 0xDB, 0x0A, 0xE5, 0x16);
            public static readonly Guid UsersFiles = new(0xF3CE0F7C, 0x4901, 0x4ACC, 0x86, 0x48, 0xD5, 0xD4, 0x4B, 0x04, 0xEF, 0x8F);
            public static readonly Guid UsersLibraries = new(0xA302545D, 0xDEFF, 0x464B, 0xAB, 0xE8, 0x61, 0xC8, 0x64, 0x8D, 0x93, 0x9B);
            public static readonly Guid Videos = new(0x18989B1D, 0x99B5, 0x455B, 0x84, 0x1C, 0xAB, 0x7C, 0x74, 0xE4, 0xDD, 0xFC);
            public static readonly Guid VideosLibrary = new(0x491E922F, 0x5643, 0x4AF4, 0xA7, 0xEB, 0x4E, 0x7A, 0x13, 0x8D, 0x81, 0x74);
            public static readonly Guid Windows = new(0xF38BF404, 0x1D43, 0x42F2, 0x93, 0x05, 0x67, 0xDE, 0x0B, 0x28, 0xFC, 0x23);
        }

        public enum KNOWN_FOLDER_FLAG
        {
            KF_FLAG_CREATE = 32768,
            KF_FLAG_DONT_VERIFY = 16384,
            KF_FLAG_DONT_UNEXPAND = 8192,
            KF_FLAG_NO_ALIAS = 4096,
            KF_FLAG_INIT = 2048,
            KF_FLAG_DEFAULT_PATH = 1024,
            KF_FLAG_NOT_PARENT_RELATIVE = 512,
            KF_FLAG_SIMPLE_IDLIST = 256,
            KF_FLAG_ALIAS_ONLY = 214783648
        }

        private enum SHSTOCKICONID : uint
        {
            SIID_DOCNOASSOC = 0,
            SIID_DOCASSOC = 1,
            SIID_APPLICATION = 2,
            SIID_FOLDER = 3,
            SIID_FOLDEROPEN = 4,
            SIID_DRIVE525 = 5,
            SIID_DRIVE35 = 6,
            SIID_DRIVEREMOVE = 7,
            SIID_DRIVEFIXED = 8,
            SIID_DRIVENET = 9,
            SIID_DRIVENETDISABLED = 10,
            SIID_DRIVECD = 11,
            SIID_DRIVERAM = 12,
            SIID_WORLD = 13,
            SIID_SERVER = 15,
            SIID_PRINTER = 16,
            SIID_MYNETWORK = 17,
            SIID_FIND = 22,
            SIID_HELP = 23,
            SIID_SHARE = 28,
            SIID_LINK = 29,
            SIID_SLOWFILE = 30,
            SIID_RECYCLER = 31,
            SIID_RECYCLERFULL = 32,
            SIID_MEDIACDAUDIO = 40,
            SIID_LOCK = 47,
            SIID_AUTOLIST = 49,
            SIID_PRINTERNET = 50,
            SIID_SERVERSHARE = 51,
            SIID_PRINTERFAX = 52,
            SIID_PRINTERFAXNET = 53,
            SIID_PRINTERFILE = 54,
            SIID_STACK = 55,
            SIID_MEDIASVCD = 56,
            SIID_STUFFEDFOLDER = 57,
            SIID_DRIVEUNKNOWN = 58,
            SIID_DRIVEDVD = 59,
            SIID_MEDIADVD = 60,
            SIID_MEDIADVDRAM = 61,
            SIID_MEDIADVDRW = 62,
            SIID_MEDIADVDR = 63,
            SIID_MEDIADVDROM = 64,
            SIID_MEDIACDAUDIOPLUS = 65,
            SIID_MEDIACDRW = 66,
            SIID_MEDIACDR = 67,
            SIID_MEDIACDBURN = 68,
            SIID_MEDIABLANKCD = 69,
            SIID_MEDIACDROM = 70,
            SIID_AUDIOFILES = 71,
            SIID_IMAGEFILES = 72,
            SIID_VIDEOFILES = 73,
            SIID_MIXEDFILES = 74,
            SIID_FOLDERBACK = 75,
            SIID_FOLDERFRONT = 76,
            SIID_SHIELD = 77,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80,
            SIID_KEY = 81,
            SIID_SOFTWARE = 82,
            SIID_RENAME = 83,
            SIID_DELETE = 84,
            SIID_MEDIAAUDIODVD = 85,
            SIID_MEDIAMOVIEDVD = 86,
            SIID_MEDIAENHANCEDCD = 87,
            SIID_MEDIAENHANCEDDVD = 88,
            SIID_MEDIAHDDVD = 89,
            SIID_MEDIABLURAY = 90,
            SIID_MEDIAVCD = 91,
            SIID_MEDIADVDPLUSR = 92,
            SIID_MEDIADVDPLUSRW = 93,
            SIID_DESKTOPPC = 94,
            SIID_MOBILEPC = 95,
            SIID_USERS = 96,
            SIID_MEDIASMARTMEDIA = 97,
            SIID_MEDIACOMPACTFLASH = 98,
            SIID_DEVICECELLPHONE = 99,
            SIID_DEVICECAMERA = 100,
            SIID_DEVICEVIDEOCAMERA = 101,
            SIID_DEVICEAUDIOPLAYER = 102,
            SIID_NETWORKCONNECT = 103,
            SIID_INTERNET = 104,
            SIID_ZIPFILE = 105,
            SIID_SETTINGS = 106,
            SIID_DRIVEHDDVD = 132,
            SIID_DRIVEBD = 133,
            SIID_MEDIAHDDVDROM = 134,
            SIID_MEDIAHDDVDR = 135,
            SIID_MEDIAHDDVDRAM = 136,
            SIID_MEDIABDROM = 137,
            SIID_MEDIABDR = 138,
            SIID_MEDIABDRE = 139,
            SIID_CLUSTEREDDRIVE = 140,
            SIID_MAX_ICONS = 181
        }

        [Flags]
        public enum SHGSI : uint
        {
            /// <summary>The szPath and iIcon members of the SHSTOCKICONINFO structure receive the path and icon index of the requested icon, in a format suitable for passing to the ExtractIcon function. The numerical value of this flag is zero, so you always get the icon location regardless of other flags.</summary>
            SHGSI_ICONLOCATION = 0,
            /// <summary>The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.</summary>
            SHGSI_ICON = 0x000000100,
            /// <summary>The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.</summary>
            SHGSI_SYSICONINDEX = 0x000004000,
            /// <summary>Modifies the SHGSI_ICON value by causing the function to add the link overlay to the file's icon.</summary>
            SHGSI_LINKOVERLAY = 0x000008000,
            /// <summary>Modifies the SHGSI_ICON value by causing the function to blend the icon with the system highlight color.</summary>
            SHGSI_SELECTED = 0x000010000,
            /// <summary>Modifies the SHGSI_ICON value by causing the function to retrieve the large version of the icon, as specified by the SM_CXICON and SM_CYICON system metrics.</summary>
            SHGSI_LARGEICON = 0x000000000,
            /// <summary>Modifies the SHGSI_ICON value by causing the function to retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.</summary>
            SHGSI_SMALLICON = 0x000000001,
            /// <summary>Modifies the SHGSI_LARGEICON or SHGSI_SMALLICON values by causing the function to retrieve the Shell-sized icons rather than the sizes specified by the system metrics.</summary>
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [Flags]
        enum SHGFI
        {
            SHGFI_ICON = 0x000000100,
            SHGFI_DISPLAYNAME = 0x000000200,
            SHGFI_TYPENAME = 0x000000400,
            SHGFI_ATTRIBUTES = 0x000000800,
            SHGFI_ICONLOCATION = 0x000001000,
            SHGFI_EXETYPE = 0x000002000,
            SHGFI_SYSICONINDEX = 0x000004000,
            SHGFI_LINKOVERLAY = 0x000008000,
            SHGFI_SELECTED = 0x000010000,
            SHGFI_ATTR_SPECIFIED = 0x000020000,
            SHGFI_LARGEICON = 0x000000000,
            SHGFI_SMALLICON = 0x000000001,
            SHGFI_OPENICON = 0x000000002,
            SHGFI_SHELLICONSIZE = 0x000000004,
            SHGFI_PIDL = 0x000000008,
            SHGFI_USEFILEATTRIBUTES = 0x000000010,
            SHGFI_ADDOVERLAYS = 0x000000020,
            SHGFI_OVERLAYINDEX = 0x000000040
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, 
            uint dwFileAttributes, 
            ref SHFILEINFO psfi, 
            uint cbFileInfo, 
            uint uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(IntPtr pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SHGetKnownFolderIDList([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, 
            KNOWN_FOLDER_FLAG dwFlags, 
            IntPtr hToken, 
            out IntPtr ppidl);

        [DllImport("shell32.dll")]
        public static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll")]
        private static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

        public static (ImageSource smallIcon, ImageSource largeIcon) GetFileIcon(string filename)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(filename, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_USEFILEATTRIBUTES));
            Icon icon = Icon.FromHandle(shinfo.hIcon);
            ImageSource smallIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(16, 16));
            DestroyIcon(icon.Handle);

            shinfo = new SHFILEINFO();
            SHGetFileInfo(filename, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGFI.SHGFI_ICON | SHGFI.SHGFI_LARGEICON | SHGFI.SHGFI_USEFILEATTRIBUTES));
            icon = Icon.FromHandle(shinfo.hIcon);
            ImageSource largeIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(32, 32));
            DestroyIcon(icon.Handle);

            return (smallIcon, largeIcon);
        }

        private static (ImageSource smallIcon, ImageSource largeIcon) GetKnownIcon(IntPtr pidList)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(pidList, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGFI.SHGFI_PIDL | SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_USEFILEATTRIBUTES));
            Icon icon = Icon.FromHandle(shinfo.hIcon);
            ImageSource smallIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(16, 16));
            DestroyIcon(icon.Handle);

            shinfo = new SHFILEINFO();
            SHGetFileInfo(pidList, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGFI.SHGFI_PIDL | SHGFI.SHGFI_ICON | SHGFI.SHGFI_LARGEICON | SHGFI.SHGFI_USEFILEATTRIBUTES));
            icon = Icon.FromHandle(shinfo.hIcon);
            ImageSource largeIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(32, 32));
            DestroyIcon(icon.Handle);

            return (smallIcon, largeIcon);
        }

        public static (ImageSource smallIcon, ImageSource largeIcon) GetFolderIcon()
        {
            var info = new SHSTOCKICONINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            SHGetStockIconInfo((uint)(SHSTOCKICONID.SIID_FOLDER), (uint)(SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON), ref info);
            var icon = Icon.FromHandle(info.hIcon);
            ImageSource smallIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(16, 16));
            DestroyIcon(info.hIcon);

            SHGetStockIconInfo((uint)(SHSTOCKICONID.SIID_FOLDER), (uint)(SHGSI.SHGSI_ICON | SHGSI.SHGSI_LARGEICON), ref info);
            icon = Icon.FromHandle(info.hIcon);
            ImageSource largeIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(32, 32));
            DestroyIcon(info.hIcon);

            return (smallIcon, largeIcon);

        }

        public static (ImageSource smallIcon, ImageSource largeIcon) GetMyComputerIcon()
        {
            IntPtr pidList = IntPtr.Zero;
            try
            {
                SHGetKnownFolderIDList(KnownFolderId.ComputerFolder, 0, IntPtr.Zero, out pidList);
                return GetKnownIcon(pidList);
            }
            finally
            {
                ILFree(pidList);                
            }
        }

        public static (ImageSource smallIcon, ImageSource largeIcon) GetDesktopIcon()
        {            
            IntPtr pidList = IntPtr.Zero;
            try
            {
                SHGetKnownFolderIDList(KnownFolderId.Desktop, 0, IntPtr.Zero, out pidList);
                return GetKnownIcon(pidList);

            }
            finally
            {
                ILFree(pidList);
            }
        }

        public static (ImageSource smallIcon, ImageSource largeIcon) GetDocumentsIcon()
        {            
            IntPtr pidList = IntPtr.Zero;
            try
            {
                SHGetKnownFolderIDList(KnownFolderId.Documents, 0, IntPtr.Zero, out pidList);
                return GetKnownIcon(pidList);
            }
            finally
            {
                ILFree(pidList);
            }
        }
    }
}
