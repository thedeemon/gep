#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2007
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using NineRays.Obfuscator;

namespace gep
{
    #region Declarations

    /// <summary>
    /// From AMDDS_* defines
    /// </summary>
    [Flags]
    public enum DirectDrawSwitches
    {
        None = 0x00,
        DCIPS = 0x01,
        PS = 0x02,
        RGBOVR = 0x04,
        YUVOVR = 0x08,
        RGBOFF = 0x10,
        YUVOFF = 0x20,
        RGBFLP = 0x40,
        YUVFLP = 0x80,
        All = 0xFF,
        YUV = (YUVOFF | YUVOVR | YUVFLP),
        RGB = (RGBOFF | RGBOVR | RGBFLP),
        Primary = (DCIPS | PS)
    }

    /// <summary>
    /// From AM_PROPERTY_FRAMESTEP
    /// </summary>
    public enum PropertyFrameStep
    {
        Step   = 0x01,
        Cancel = 0x02,
        CanStep = 0x03,
        CanStepMultiple = 0x04
    }

    /// <summary>
    /// From AM_FRAMESTEP_STEP
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FrameStepStep
    {
        public int dwFramesToStep;
    }

    /// <summary>
    /// From MPEG1VIDEOINFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MPEG1VideoInfo
    {
        public VideoInfoHeader hdr;
        public int dwStartTimeCode;
        public int           cbSequenceHeader;
        public byte            bSequenceHeader;
    }

/*typedef struct tagMPEG2VIDEOINFO {
    VIDEOINFOHEADER2    hdr;
    DWORD               dwStartTimeCode;   
    DWORD               cbSequenceHeader;     
    DWORD               dwProfile;     
    DWORD               dwLevel;            
    DWORD               dwFlags;            
    DWORD               dwSequenceHeader[1];     
} MPEG2VIDEOINFO;*/

    /// <summary>
    /// From MPEG2VIDEOINFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MPEG2VideoInfo
    {
        public VideoInfoHeader2 hdr;
        public int dwStartTimeCode;
        public int cbSequenceHeader;
        public int dwProfile;
        public int dwLevel;
        public int dwFlags;
        public int dwSequenceHeader;     
    }

    /// <summary>
    /// From ANALOGVIDEOINFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AnalogVideoInfo
    {
        public Rectangle            rcSource;
        public Rectangle            rcTarget;
        public int            dwActiveWidth;
        public int            dwActiveHeight;
        public long  AvgTimePerFrame;
    }


    #endregion
 
}
