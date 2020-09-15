using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
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
    /// A class to keep track of the information associated with a Gerber Plot Pad
    /// </summary>
    /// <history>
    ///    09 Sep 10  Cynic - Started
    /// </history>
    public class GerberPad : OISObjBase
    {
        private float x0 = 0;
        private float y0 = 0;
        private const float DEFAULT_PAD_DIAMETER = 0;
        private float padDiameter = DEFAULT_PAD_DIAMETER;
        private const bool DEFAULT_IS_REFPIN = false;
        private bool isRefPin = DEFAULT_IS_REFPIN;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public GerberPad()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x0In">the x center point</param>
        /// <param name="y0In">the y center point</param>
        /// <param name="padDiameterIn">pad diameter</param>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public GerberPad(float x0In, float y0In, float padDiameterIn)
        {
            x0 = x0In;
            y0 = y0In;
            padDiameter = padDiameterIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current x0 value
        /// </summary>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public float X0
        {
            get
            {
                return x0;
            }
            set
            {
                x0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current y0 value
        /// </summary>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public float Y0
        {
            get
            {
                return y0;
            }
            set
            {
                y0 = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pad diameter
        /// </summary>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public float PadDiameter
        {
            get
            {
                if (padDiameter < 0) padDiameter = DEFAULT_PAD_DIAMETER;
                return padDiameter;
            }
            set
            {
                padDiameter = value;
                if (padDiameter < 0) padDiameter = DEFAULT_PAD_DIAMETER;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the isRefPin flag
        /// </summary>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public bool IsRefPin
        {
            get
            {
                return isRefPin;
            }
            set
            {
                isRefPin = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// returns a deep clone of the input object
        /// </summary>
        /// <param name="gcodePadObj">the object to clone</param>
        /// <history>
        ///    09 Sep 10  Cynic - Started
        /// </history>
        public static GerberPad DeepClone(GerberPad gcodePadObj)
        {
            if (gcodePadObj == null) return null;
            GerberPad tmpObj = new GerberPad();
            tmpObj.PadDiameter = gcodePadObj.PadDiameter;
            tmpObj.IsRefPin = gcodePadObj.IsRefPin;
            tmpObj.X0 = gcodePadObj.X0;
            tmpObj.Y0 = gcodePadObj.Y0;
            return tmpObj;
        }
    }
}
