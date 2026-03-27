CTE.dll
=======
This native-code library provides a .dll 'wrapper' around the CTE-provided native-code static library fcsa_mfc_x64_20160711.lib

CTE.lib
=======
This is the native-code static link 'import' library to link to at compile-time so that an application can access CTE.dll at run-time.

Notes
=====
CTE provides FCSA with their 3rd-party library of functions that perform a repertoir of Over-Horizon Loss (OHL) microwave transmission calculations. This OHL library was provided to FCSA as a static library compiled from 'C' code using Visual Studio (VS) 2010.

Significant technical finesse was needed to successfully make use of CTE's static library. MICS code developers/maintainers should read and understand the detailed remarks provided below before tinkering with the C# code.

The first issue is that 'C' source code is not available for CTE's 'C' code static library and that it was built using VS2010.

VS2015 uses a newer version of the linker than VS2010 and C/C++ applications built with VS2015 cannot link successfully to static libraries built with VS2010; the VS link stage returns no errors but the application will crash at run-time.

In any event, C# code cannot be linked to native code static (C/C++) libraries.

The solution to this dilemma is to use VS2010 to build a dydnamic link library (dll) from the CTE-supplied static library: managed (C#) applications can use dlls created from unmanaged (C/C++) code; furthermore it does not matter what version of Visual Studio was used to build the dll.

A VS2010 C++ Project was created that builds a .dll by statically linking with CTE's fcsa_mfc_x64_20160711.lib library with the following property set to 'YES':

Property Pages -> Configuration Properties -> Linker -> General -> Link Library Dependencies 

This build-property setting causes the object code in fcsa_mfc_x64_20160711.lib to be copied into the resulting .dll 

Microsoft's P/Invoke functionality is used to [In, Out] pass C# method arguments to CTE's 'C/C++' native code functions.

The CTE native code function calc_ohloss() is called with a single argument, the 'C' structure called ohloss_xfer defined in the CTE-supplied ohloss.h include file. By default, 'C' structures are passed by value, i.e. the whole structure is pushed onto and popped off the stack. Fortunately, CTE designed their function calc_ohloss() to pass only the address of the struct ohloss_xfer, i.e. call by reference.

The CTE-defined 'C' structure ohloss_xfer has five fields that are arrays whose length is explicitely declared. An obscure feature of 'C' is that a structure field declared as a constant length array has its storage 'inlined' inside the structure rather than the field being stored as a pointer to an array somewhere else in heap memory.

The implicit 'C' ohloss_xfer structure storage inlining of fixed-length array fields described above means that some finesse is required to develop a C# structure that can be P/Invoke marshalled [In, Out] with the native function calc_ohloss().

C# does provide the 'fixed' keyword that performs the same (implicit) inlining of structure fields that are explicitely declared as arrays of constant length. The use of 'fixed' in this way is valid only in C# 'struct' definitions not 'class' definitions. (Hence the type OhLoss_Xfer used for the P/Invoke is a 'struct' not a class.)

Just to add further complication, when the C# qualifier 'fixed' is applied to fields of the structure thereafter those fields become 'unsafe' and have to be accessed as such. Also, the project _OHloss has to be compiled with the 'allow unsafe code' swtich turned on.

A C# application calls the method Calc_OhLoss() using an instance of the class OhLossXfer that is a 'clean and simple' equivalent of the 'C' struct ohloss_xfer with all the 'inlined' arrays replaced by conventional C# string and double[] fields.
