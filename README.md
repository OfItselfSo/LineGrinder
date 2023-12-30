# Line Grinder
Line Grinder can generate isolation routing GCode from the Gerber format plot files output by most PCB design programs. The GCode Line Grinder produces, if run sequentially, should also provide a method for reliably producing nicely aligned, double sided isolation routed PCBs including the cutting of border outlines and the drilling of pad holes and vias. It is intended to be a one-stop-shop GCode generator and methodology which will produce consistent results when cutting circuit boards. 

### Latest Version: 03.04 as of 30 December 2023

### Home Page and Compiled Binary
The home page for this project can be found at [http://www.OfItselfSo.com/LineGrinder/LineGrinder.php](http://www.OfItselfSo.com/LineGrinder/LineGrinder.php). A compiled binary is available in zipped format on the home page for those who do not wish to compile their own copy.

## Capabilities

    * Converts Gerber format plot files into GCode files which will engrave isolation cuts around the PCB traces.
    * Can flip bottom layers about the X or Y axis.
    * Will interpret board outline layers and can generate GCode to edge mill the PCB out of a larger board.
    * Can generate bed flattening GCode which can mill flat a sacrificial board bolted to the bed in order to ensure the PCB is perfectly square to the toolhead when creating the isolation routing cuts.
    * Can generate GCode to drill reference pin holes in order to ensure exact registration of the pads when cutting double sided PCB's.
    * Will interpret Excellon drill files and produce GCode suitable for drilling pad holes and vias.
    * Has a nice plot view mode which visually displays the Gerber and GCode files.
    * Is extremely configurable with a variety of options to control cutting depths and toolhead speeds for each of the various types of output GCode.
    * Runs on Windows 10 or higher.
    * The software was written in C# and a Visual Studio 2019 solution and project are included with the source code. 

## Tutorial Videos
Tutorial Videos are available on the OfItselfSo YouTube channel at: [https://www.youtube.com/@ofitselfso](https://www.youtube.com/@ofitselfso)

## Help Files and Manual

The Online Manual (http://www.ofitselfso.com/LineGrinder/LineGrinderHelp/LineGrinderHelp_TableOfContents.html)  contains a set of help files which, when viewed in sequence, form a "Get Started Guide". There are also specific help files which discuss how to prepare and output suitable Gerber files from the Eagle and DesignSpark software. The Line Grinder software repository contains a complete set of these help files.

## What Line Grinder Does Not Do

Please be aware that the Line Grinder software is purely a code generator. In other words, if you feed it some Gerber or Excellon files you will get some GCode out. Where, how and when you run those GCode files is entirely up to you. Specifically, the Line Grinder software is not a machine controller (like LinuxCNC, Mach3, TurboCNC or many others) and it cannot make your PC physically interact with a CNC mill.

## License

The LineGrinder software is open source and released under the MIT License. 

