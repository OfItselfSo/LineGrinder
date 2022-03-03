using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using System.Drawing;

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
    /// A class to persist the settings of the LineGrinder app.
    /// </summary>
    /// <remarks>
    /// The ApplicationSettingsBase class does not write to the registry. It 
    /// persists the configuration settings by leaving an XML file on disk in the location 
    /// below.
    /// 
    /// <document and settings folder>\<userfolder>\Local Settings\Application
    /// Data\<companyname>\<applicationname>_StrongName\<applicationversion>\user.config
    /// 
    /// This code uses the techniques discussed here
    /// http://msdn.microsoft.com/en-us/library/system.configuration.applicationsettingsbase%28VS.80%29.aspx
    /// </remarks>
    public sealed class ApplicationImplicitSettings : ApplicationSettingsBase
    {
        // some defines
        public const ApplicationUnitsEnum DEFAULT_APPLICATION_UNITS = ApplicationUnitsEnum.INCHES;
        public const int DEFAULT_VIRTURALCOORD_PER_INCH = 2000;
        public const int DEFAULT_VIRTURALCOORD_PER_MM = 78;
        public const int DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_IN = DEFAULT_VIRTURALCOORD_PER_INCH;
        public const int DEFAULT_ISOPLOTPOINTS_PER_APPUNIT_MM = DEFAULT_VIRTURALCOORD_PER_MM;

        // ####################################################################
        // ##### Config Items Explicitly Set By the User
        // ####################################################################
        #region Config Items Explicitly Set By the User

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("INCHES")]
        public ApplicationUnitsEnum DefaultApplicationUnits
        {
            get { return (ApplicationUnitsEnum)this["DefaultApplicationUnits"]; }
            set { this["DefaultApplicationUnits"] = value; }
        }
        
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("2000")]
        public int IsoPlotPointsPerAppUnitIN
        {
            get { return (int)this["IsoPlotPointsPerAppUnitIN"]; }
            set { this["IsoPlotPointsPerAppUnitIN"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("78")]
        public int IsoPlotPointsPerAppUnitMM
        {
            get { return (int)this["IsoPlotPointsPerAppUnitMM"]; }
            set { this["IsoPlotPointsPerAppUnitMM"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool ShowGerberApertures
        {
            get { return (bool)this["ShowGerberApertures"]; }
            set { this["ShowGerberApertures"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool ShowGerberCenterLines
        {
            get { return (bool)this["ShowGerberCenterLines"]; }
            set { this["ShowGerberCenterLines"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool ShowOrigin
        {
            get { return (bool)this["ShowOrigin"]; }
            set { this["ShowOrigin"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool ShowFlipAxis
        {
            get { return (bool)this["ShowFlipAxis"]; }
            set { this["ShowFlipAxis"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool ShowGCodeOrigin
        {
            get { return (bool)this["ShowGCodeOrigin"]; }
            set { this["ShowGCodeOrigin"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool ShowGerberOnGCodePlots
        {
            get { return (bool)this["ShowGerberOnGCodePlots"]; }
            set { this["ShowGerberOnGCodePlots"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool OKWithDisclaimer
        {
            get { return (bool)this["OKWithDisclaimer"]; }
            set { this["OKWithDisclaimer"] = value; }
        }

        #endregion

        // ####################################################################
        // ##### Config Items Implicitly Set By the User
        // ####################################################################
        #region Config Items Implicitly Set By the User

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("800, 600")]
        public Size FormSize
        {
            get { return (Size)this["FormSize"]; }
            set { this["FormSize"] = value; }
        }

        // the most recently used file list
        [UserScopedSettingAttribute()]
        public List<string> MRUFileList
        {
            get { return (List<string>)this["MRUFileList"]; }
            set { this["MRUFileList"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LastOpenFileDirectory
        {
            get { return (String)this["LastOpenFileDirectory"];} 
            set { this["LastOpenFileDirectory"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LastGCodeDirectory
        {
            get { return (String)this["LastGCodeDirectory"]; }
            set { this["LastGCodeDirectory"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LastOpenFileName
        {
            get { return (String)this["LastOpenFileName"]; }
            set { this["LastOpenFileName"] = value; }
        }

        #endregion

    }
}

