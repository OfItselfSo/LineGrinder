using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OISCommon;

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
    /// A abstract base class for the objects the GCodeBuilder creates to 
    /// convert Gerber drawing primitives into GCodes
    /// </summary>
    /// <history>
    ///    26 Jul 10  Cynic - Started
    /// </history>
    public abstract class GCodeBuilderObject : OISObjBase
    {
        public const int DEFAULT_BUILDEROJBECT_ID = 0;
        protected int builderObjectID = DEFAULT_BUILDEROJBECT_ID;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builderObjectIDIn">the builderObjectID for this object</param>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public GCodeBuilderObject(int builderObjectIDIn)
        {
            builderObjectID = builderObjectIDIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// gets/sets the builderObjectID
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public int BuilderObjectID
        {
            get
            {
                return builderObjectID;
            }
            set
            {
                builderObjectID = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Utility proc to strip the flags off of a pixel so we can see only the ID. 
        /// <returns>
        /// int with lower 24 bits of the supplied pixel value
        /// </returns>
        /// </summary>
        /// <history>
        ///    26 Jul 10  Cynic - Started
        /// </history>
        public static int ReturnIDFromPixel(int pixelValue)
        {
            return pixelValue & 0x00ffffff;
        }

    }
}
