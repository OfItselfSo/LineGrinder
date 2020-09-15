using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE: MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
/// ¦                                                                                                                              ¦
/// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
/// ¦                                                                                                                              ¦
/// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
/// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
/// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
/// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
/// +------------------------------------------------------------------------------------------------------------------------------+

namespace LineGrinder
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// These are flags in upper eight bits of the isolation array pixels and their 
    /// meanings
    /// </summary>
    /// <history>
    ///    26 Jul 10  Cynic - Started
    /// </history>
    [Flags]
    public enum GCodeBuilderPixelFlagEnum
    {
        GCodeBuilderPixelFlag_BACKGROUND = 0x01000000,
        GCodeBuilderPixelFlag_EDGE = 0x02000000,
        GCodeBuilderPixelFlag_OVERLAY = 0x04000000,
        GCodeBuilderPixelFlag_DISREGARD = 0x08000000,
        GCodeBuilderPixelFlag_EDGEINUSE = 0x10000000,
    }

//TODO make this documentation better
    /* WHAT THESE FLAGS MEAN
     * The isolation plot array is a 2D array of 32 bit integers. These integers
     * will indicate which objects are using those points (pixels) and for what
     * purpose. 
     *   - A value of 0 (PIXEL_NOTUSED) means no object is using that pixel
     *   - the bottom 24 bits is the builderObjectId if the OVERLAY flag is not used
     *     or the ID of the overlay object (essentially a container for multiple builderObjects)
     *     if the OVERLAY is set on the pixel
     *   - OVERLAY means that two or more builderObjects are using the pixel and that the 
     *   
     A - This pixel is a background pixel BACK_PIXEL
B - This pixel is a overlay pixel OVERLAY_PIXEL
C - this pixel is an edge pixel EDGE_PIXEL
D - this pixel is an intersection edge pixel INTER_PIXEL
*/
}
