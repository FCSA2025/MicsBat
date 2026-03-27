//       10        20        30        40        50        60        70        80
//345678901234567890123456789012345678901234567890123456789012345678901234567890


/// \mainpage \image html "MICS Title graphic.png" ""
/// \section intro_sec Introduction
/// This web page relates to the support of the Frequency Coordination System 
/// Association's (FCSA) 'MICS' application software that provides the 
/// analytical/mathematical capability of its online WebMICS service. Specifically, 
/// this page is the portal to the documentation of the MICS C# source code.
/// \section terminolgy C# Terminology
/// <list type="bullet">
/// <item>a <b>PACKAGE</b> (aka NameSpace) is an assembly of classes that 
/// creates a .NET executable or dynamic link library.</item>
/// <item>a <b>CLASS</b> is an encapsulation of a set of fields and methods that 
/// together provide well-defined, high-level, functionality.</item>
/// <item>a <b>FIELD</b> is a constant or variable data element within a class.</item>
/// <item>a <b>METHOD</b> is a function or subroutine.</item>
/// <item>a <b>FILE</b> of source code typically defines one class.</item>
/// </list>
/// \section techback Technical Background
/// Development of the first MICS software began in 1976 using FORTRAN and COBOL.
/// In 1997 work began to recode MICS in 'C'. As of May 2015, all of the MICS
/// source code was in 'C'. Even though it was compiled as C++, it did not have
/// an object-oriented design. To better ensure long-term maintenance of the MICS 
/// code in January 2017 work began to refactor the legacy 'C' source code into 
/// C#. The legacy'C' structures that mirrored the columns of a specific database
/// table types became classes in C#. The thousand or so constants that were 
/// strewn around the 'C" code are all collected together in one C# class. 
/// A significant amount of duplicated and 'deadwood''C' was eliminated in the 
/// refactoring to C#.
/// 


