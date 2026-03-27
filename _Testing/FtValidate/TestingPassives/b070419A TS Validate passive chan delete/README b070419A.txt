b070419A - Validate reports errors when channel has cmd of D in passive hop

Work around - Delete the whole passive-active hops and re-add without channel


C++ results and C# results in file.

jan 2020 - created new version of hop and compared in winmerge.  No data differences
with original version except command on antennas in old file is U and new is N.

Either file can be used for testing.


Greg Shannan OEL assessment - he wanted rewrite the passive Validate to extract the passive routines to separate it from regular TS Validate.