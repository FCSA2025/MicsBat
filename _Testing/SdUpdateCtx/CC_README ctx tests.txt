This data is rarely touched.  In much of the data should not be changed as it 
came from members and manufacturers and is very specific to the equipment

Delete curve and re-add if deleting point or points as point count
is not updated by Validate and can't be updated online.   This is a bug and
needs to fixed.   testctxmod2 shows the problem.


testctxcmdn - Validate should replace changes with the database version when cmd N

testctxmod1 - Validate reports the incorrect count of points and accepts modifications.
              online editor will correct the point count.  

testctxmod2 - Shows Validate fails to update point count when deleting point(s).
	      online edit could be changed to disregard point when cmd is D.
	


update tests

ctxadd - add code to the database.

ctxmod - use s/e to create a file with the code just added.  Set cmd to U
	 make changes and validate and update. Change rqco, rqcull and rqwrst 
	 on description and fsep of 5000 the rq value.  Note the next time this
	 test is done change to -103.5.  Somehow this didn't happen on the
	 July 8th test.  The record was updated because I put a U but the rq
	 was changed due to my oversight.

ctxdel - use s/e to create a file with the code just modified.  File must be
	 exported to text and manually set cmd to D on all records.
	 run validate and update.


after each update on the sql server run check ctx update records.sql
change the date and filename before running
check web.user_tables, web.daily_usage and audit\ctx folder

