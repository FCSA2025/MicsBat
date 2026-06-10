# Documented File: Error.cs
**Repository Path:** `_Configuration\Error.cs`
**Primary Layer:** `_Configuration`
**Namespace:** `_Configuration`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Configuration
{
    /// <summary>
    /// Encapsulates the very large repertoir of error numbers and error message
    /// strings used by the MICS programs and supporting libraries.
    /// </summary>
    public class Error
    {
        private int errNum;       /* error number */
        private string errMsg;      /* text of error message */

        public int Number { get { return errNum; } }
        public string Message { get { return errMsg; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="errNum"> - error number.</param>
        /// <param name="errMsg"> - error message.</param>
        /// <returns></returns>
        public Error(int errNum, string errMsg)
        {
            this.errNum = errNum;
            this.errMsg = errMsg;
        }

        // Exit codes
        public const int COMMAND_LINE_ERROR = 1;
        public const int FATAL_EXCEPTION = 2;
        public const int ENVIRONMENT_VARIABLE_SYNTAX_ERROR = 3;
        public const int UNABLE_TO_ENTER_QUEUE = 100;

        // Job Queueing problems.
        public const int ALREADY_IN_QUEUE = -2;
        public const int JOB_HAS_NO_ROOM_TO_RUN = 1;
        public const int JOB_IN_QUEUE_HAS_BEEN_DELETED = 2;

        // ODBC error messages.
        public const int ODBC_GET_FAILED = -2;
        public const int ODBC_EXECUTE_FAILED = -3;
        public const int ODBC_EXECDIRECT_FAILED = -4;
        public const int ODBC_BINDING_FAILED = -5;
        public const int ODBC_QUERY_FAILED = -6;
        public const int ODBC_SELECT_FAILED = -7;
        public const int ODBC_FETCH_FAILED = -8;
        public const int ODBC_PREPARE_FAILED = -9;
        public const int ODBC_SQLALLOCHANDLE_FAILED = -10;
        public const int ODBC_INVALID_FETCH_COUNT = -11;
        public const int ODBC_NULL_HANDLE = -12;
        public const int ODBC_COULD_NOT_GET_ENV_HANDLE = -13;
        public const int ODBC_COULD_NOT_GET_CONNECT_HANDLE = -14;
        public const int ODBC_SQLCONNECT_FAILED = -15;


        public const int NO_CURSOR_AVAILABLE = -1;

        public const int COULD_NOT_CREATE_MUTEX = -2;
        public const int MUTEX_ALREADY_OWNED = -3;

        public const int UTCVTNAME_FAILED = -10;

        public const int MYBASE = 1000000;
        public const int INVALID_DATA = MYBASE + 1;
        public const int CACHE_WRITE_FAILED = MYBASE + 2;
        public const int CACHE_READ_FAILED = MYBASE + 3;
        public const int INVALID_ARGUMENTS = MYBASE + 4;
        public const int RX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE = MYBASE + 5;
        public const int TX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE = MYBASE + 5;
        public const int VICTIM_TYPE_NOT_A_OR_D = MYBASE + 6;

        public const int RC_BASE = -50;
        public const int WARN_BASE = -200; // base for warnings
        public const int ERROR_BASE = -2000; // base for error numbers

        public const int INVDEST = (RC_BASE - 0);
        public const int ABNORMTERM = (RC_BASE - 1);
        public const int NOPRINTER = (RC_BASE - 2);
        public const int PROGNOTFOUND = (RC_BASE - 3);
        public const int INVPARM = (RC_BASE - 4);
        public const int NOCONNECT = (RC_BASE - 5);
        public const int NOEXECUTE = (RC_BASE - 6);
        public const int NOCOMPLETE = (RC_BASE - 7);
        public const int NODATA = (RC_BASE - 8);

        public const int RECMESS = (WARN_BASE - 1);
        public const int GENMESS = (WARN_BASE - 2);
        public const int INVALIDDOMAIN = (WARN_BASE - 3);
        public const int STACKOVRFLW = (WARN_BASE - 4);
        public const int NOSELECT = (WARN_BASE - 5);
        public const int MUSTBEON = (WARN_BASE - 6);
        public const int SELECTREC = (WARN_BASE - 7);
        public const int NOFILESELECT = (WARN_BASE - 8);
        public const int NOSOURCE = (WARN_BASE - 9);
        public const int NODEST = (WARN_BASE - 10);
        public const int NOOUTPUT = (WARN_BASE - 11);
        public const int VALIDSELECT = (WARN_BASE - 12);
        public const int NORECORDS = (WARN_BASE - 13);
        public const int NOMOREREC = (WARN_BASE - 14);
        public const int NOEDITFILE = (WARN_BASE - 15);
        public const int NODELETEFILE = (WARN_BASE - 16);
        public const int NOSELECTFILE = (WARN_BASE - 17);
        public const int RECADDED = (WARN_BASE - 18);
        public const int RECUPDATED = (WARN_BASE - 19);
        public const int RECDELETED = (WARN_BASE - 20);
        public const int NOTITLE = (WARN_BASE - 21);
        public const int TITLEEXIST = (WARN_BASE - 22);
        public const int ACCHELP = (WARN_BASE - 23);
        public const int NYI = (WARN_BASE - 24);
        public const int WORKING = (WARN_BASE - 25);
        public const int TEXTWRITE = (WARN_BASE - 26);
        public const int PROCRECNO = (WARN_BASE - 27);
        public const int DEFAULT_CTX = (WARN_BASE - 28);
        public const int TABLECOPIED = (WARN_BASE - 29);
        public const int TABLEPRINTED = (WARN_BASE - 30);
        public const int TABLEDELETED = (WARN_BASE - 31);
        public const int TABLEIMPORTED = (WARN_BASE - 32);
        public const int TABLEEXIST = (WARN_BASE - 33);
        public const int AZIMDEFAULT = (WARN_BASE - 34);
        public const int USEWORSTTS = (WARN_BASE - 35);
        public const int USEWORSTES = (WARN_BASE - 36);
        public const int TABLEEXPORTED = (WARN_BASE - 37);
        public const int TABLEVALIDATED = (WARN_BASE - 38);
        public const int PCNGEN = (WARN_BASE - 39);
        public const int SLAGEN = (WARN_BASE - 40);
        public const int USERABANDON = (WARN_BASE - 41);
        public const int CHGPCODE = (WARN_BASE - 42);
        public const int CHANGEN = (WARN_BASE - 43);
        public const int NOCHANGEN = (WARN_BASE - 44);
        public const int ATTOP = (WARN_BASE - 45);
        public const int ATEND = (WARN_BASE - 46);
        public const int NUMSITES = (WARN_BASE - 47);
        public const int BATCHPROC = (WARN_BASE - 48);
        public const int NOPCODETODEL = (WARN_BASE - 49);
        public const int CHANGESSAVED = (WARN_BASE - 50);
        public const int RECSAVED = (WARN_BASE - 51);
        public const int PROCESSING = (WARN_BASE - 52);
        public const int LOADINGSELS = (WARN_BASE - 53);
        public const int ENDOFREC = (WARN_BASE - 54);
        public const int PARMUPDATE = (WARN_BASE - 55);
        public const int ANTDUPDATE = (WARN_BASE - 56);

        public const int GENERROR = (ERROR_BASE - 1);
        public const int FILEERROR = (ERROR_BASE - 2);
        public const int MENUDRIVERERROR = (ERROR_BASE - 3);
        public const int FASTRAKERROR = (ERROR_BASE - 4);
        public const int RECNOTADDED = (ERROR_BASE - 5);
        public const int RECNOTUPDATED = (ERROR_BASE - 6);
        public const int RECNOTDELETED = (ERROR_BASE - 7);
        public const int BADFIELDFORMAT = (ERROR_BASE - 8);
        public const int BADFORMAT = (ERROR_BASE - 9);
        public const int TOOMANY = (ERROR_BASE - 10);
        public const int FILENOTEXIST = (ERROR_BASE - 11);
        public const int ALREADYEXIST = (ERROR_BASE - 12);
        public const int IMPNOTEXIST = (ERROR_BASE - 13);
        public const int NOOPER = (ERROR_BASE - 14);
        public const int LOGOPEN = (ERROR_BASE - 15);
        public const int ERRORADDING = (ERROR_BASE - 16);
        public const int ERRORDELETING = (ERROR_BASE - 17);
        public const int ERRORONS = (ERROR_BASE - 18);
        public const int FILEWEXIST = (ERROR_BASE - 19);
        public const int FILEWCREATERR = (ERROR_BASE - 20);
        public const int PRINTFILEERR = (ERROR_BASE - 21);
        public const int OUTPUTFILEERR = (ERROR_BASE - 22);
        public const int CURSOROPEN = (ERROR_BASE - 23);
        public const int DELETEERR = (ERROR_BASE - 24);
        public const int FILEWNOTFOUND = (ERROR_BASE - 25);
        public const int FILEWNOTCOPY = (ERROR_BASE - 26);
        public const int BADDEST = (ERROR_BASE - 27);
        public const int SOURCEEQDEST = (ERROR_BASE - 28);
        public const int BADPROV = (ERROR_BASE - 29);
        public const int BADDATE = (ERROR_BASE - 30);
        public const int BADLAT = (ERROR_BASE - 31);
        public const int BADLONG = (ERROR_BASE - 32);
        public const int NOTABLE = (ERROR_BASE - 33);
        public const int INVENVTYPE = (ERROR_BASE - 34);
        public const int BADENVTYPE = (ERROR_BASE - 35);
        public const int BADENVNAME = (ERROR_BASE - 36);
        public const int BADSELSITE = (ERROR_BASE - 37);
        public const int INVSELSITE = (ERROR_BASE - 38);
        public const int BADCOUNTRY = (ERROR_BASE - 39);
        public const int BADTSSPHERECALC = (ERROR_BASE - 40);
        public const int BADTSORB = (ERROR_BASE - 41);
        public const int BADCHANCODE = (ERROR_BASE - 42);
        public const int BADCALLSIGN = (ERROR_BASE - 43);
        public const int BADOPCODE = (ERROR_BASE - 44);
        public const int NOEQPTFOUND = (ERROR_BASE - 45);
        public const int NOTRAFFOUND = (ERROR_BASE - 46);
        public const int NOANTEFOUND = (ERROR_BASE - 47);
        public const int NOANTDFOUND = (ERROR_BASE - 48);
        public const int NOCTXFOUND = (ERROR_BASE - 49);
        public const int NOCTXDPTS = (ERROR_BASE - 50);
        public const int FETCH_FAIL = (ERROR_BASE - 51);
        public const int BADSOURCE = (ERROR_BASE - 52);
        public const int INPUTFILEERR = (ERROR_BASE - 53);
        public const int OPENOUTFILEERR = (ERROR_BASE - 54);
        public const int FILEWDNE = (ERROR_BASE - 55);
        public const int ZONESUM = (ERROR_BASE - 56);
        public const int MUST_ENTER = (ERROR_BASE - 57);
        public const int NOTTSIPVALID = (ERROR_BASE - 58);
        public const int INVALIDBANDCODE = (ERROR_BASE - 59);
        public const int NOPRONAME = (ERROR_BASE - 60);
        public const int BADANALOPT = (ERROR_BASE - 61);
        public const int TEMPDNE = (ERROR_BASE - 62);
        public const int PROGNOTDEFINED = (ERROR_BASE - 63);
        public const int TOOFEWANGLES = (ERROR_BASE - 64);
        public const int NOLASTANGLE = (ERROR_BASE - 65);
        public const int NOFIRSTANGLE = (ERROR_BASE - 66);
        public const int BADCOORDISTTS = (ERROR_BASE - 67);
        public const int BADCOORDISTES = (ERROR_BASE - 68);
        public const int IS_INVALID = (ERROR_BASE - 69);
        public const int BAD_CULL = (ERROR_BASE - 70);
        public const int USAGE = (ERROR_BASE - 71);
        public const int NODATABASE = (ERROR_BASE - 72);
        public const int BADTABLENAME = (ERROR_BASE - 73);
        public const int NOCREATETABLE = (ERROR_BASE - 74);
        public const int USAGEEDIT = (ERROR_BASE - 75);
        public const int NOSELECTOPER = (ERROR_BASE - 76);
        public const int NOGENOPCODELIST = (ERROR_BASE - 77);
        public const int NOPCNGEN = (ERROR_BASE - 78);
        public const int NOSENDMAIL = (ERROR_BASE - 79);
        public const int NOGENMAILMESS = (ERROR_BASE - 80);
        public const int TABLEDNE = (ERROR_BASE - 81);
        public const int NOTSLAVALID = (ERROR_BASE - 82);
        public const int NOSLAGEN = (ERROR_BASE - 83);
        public const int USERLISTFULL = (ERROR_BASE - 84);
        public const int NOUSERID = (ERROR_BASE - 85);
        public const int NOMEMOPCODE = (ERROR_BASE - 86);
        public const int OPCODELISTFULL = (ERROR_BASE - 87);
        public const int NOTPCNVALID = (ERROR_BASE - 88);
        public const int MUSTENTERDEST = (ERROR_BASE - 89);
        public const int MUSTENTERLOC = (ERROR_BASE - 90);
        public const int MUSTENTERTFCI = (ERROR_BASE - 91);
        public const int MUSTENTERTFCR = (ERROR_BASE - 92);
        public const int MUSTENTERRXEQPT = (ERROR_BASE - 93);
        public const int MUSTENTERACODE = (ERROR_BASE - 94);
        public const int MUSTENTERCALL1 = (ERROR_BASE - 95);
        public const int NOCHANGE = (ERROR_BASE - 96);
        public const int NOTOWNER = (ERROR_BASE - 97);
        public const int PR_RESOLVED = (ERROR_BASE - 98);
        public const int MUSTENTERREFIND = (ERROR_BASE - 99);
        public const int NOPCODES = (ERROR_BASE - 100);
        public const int INVPCODE = (ERROR_BASE - 101);
        public const int INVHILO = (ERROR_BASE - 102);
        public const int INVMDBO = (ERROR_BASE - 103);
        public const int INVBAND = (ERROR_BASE - 104);
        public const int CHIDRXTX = (ERROR_BASE - 105);
        public const int NOCONSM = (ERROR_BASE - 106);
        public const int NOSMENTRY = (ERROR_BASE - 107);
        public const int NOSMAVAIL = (ERROR_BASE - 108);
        public const int CONTINUE = (ERROR_BASE - 109);
        public const int CCKNOSITE = (ERROR_BASE - 110);
        public const int NOPATT = (ERROR_BASE - 111);
        public const int MAXPARMS = (ERROR_BASE - 112);
        public const int FILEWVAL = (ERROR_BASE - 113);
        public const int NOTPVIEW = (ERROR_BASE - 114);
        public const int INVBNDCDE = (ERROR_BASE - 115);
        public const int INVBNDBIT = (ERROR_BASE - 116);
        public const int BEAMHITS = (ERROR_BASE - 117);
        public const int NOCONVERG = (ERROR_BASE - 118);
        public const int NEGLOG = (ERROR_BASE - 119);
        public const int TOOMANYCODES = (ERROR_BASE - 120);
        public const int NOTINUSERTBL = (ERROR_BASE - 121);
        public const int NOLOGINFILE = (ERROR_BASE - 122);
        public const int NOPRINTACCTFILE = (ERROR_BASE - 123);
        public const int NODELACCTFILE = (ERROR_BASE - 124);
        public const int UNKNOWNUSER = (ERROR_BASE - 125);
        public const int INVMONTH = (ERROR_BASE - 126);
        public const int NOVICCHID = (ERROR_BASE - 127);
        public const int BADTIME = (ERROR_BASE - 128);
        public const int PERMISSIONDENIED = (ERROR_BASE - 129);
        public const int FETCHERROR = (ERROR_BASE - 130);
        public const int BADSCREENWRITE = (ERROR_BASE - 131);
        public const int BADSCREENREAD = (ERROR_BASE - 132);
        public const int MUSTBEYORN = (ERROR_BASE - 133);
        public const int PERMISSIONERROR = (ERROR_BASE - 134);
        public const int NOINTERP = (ERROR_BASE - 135);
        public const int MUSTENTERCHID = (ERROR_BASE - 136);
        public const int INVCULLCODE = (ERROR_BASE - 137);
        public const int PARAMNOTEXIST = (ERROR_BASE - 138);
        public const int NOTMDBUPDVAL = (ERROR_BASE - 139);
        public const int INVREPTYPE = (ERROR_BASE - 140);
        public const int BADULTRIXID = (ERROR_BASE - 141);
        public const int BADOPERCODE = (ERROR_BASE - 142);
        public const int BADCORRESP = (ERROR_BASE - 143);
        public const int TOOFEWCODES = (ERROR_BASE - 144);
        public const int PAIRNOTUNIQUE = (ERROR_BASE - 145);
        public const int RECEXISTS = (ERROR_BASE - 146);
        public const int NOCREATETLIST = (ERROR_BASE - 147);
        public const int FE_TX_RX_GROUP = (ERROR_BASE - 148);
        public const int FE_TX_RX_MISSING = (ERROR_BASE - 149);
        public const int BADTABLE = (ERROR_BASE - 150);
        public const int NOREDIRECT = (ERROR_BASE - 151);
        public const int ALREADYPOSTED = (ERROR_BASE - 152);
        public const int NOTEXECUTABLE = (ERROR_BASE - 153);
        public const int REPORT_ERROR = (ERROR_BASE - 154);
        public const int MAXANTES = (ERROR_BASE - 155);
        public const int MAXTMP2S = (ERROR_BASE - 156);
        public const int SINGLEDIR_ADISC = (ERROR_BASE - 157);
        public const int WRST_ADISC = (ERROR_BASE - 158);
        public const int BADFILECOPY = (ERROR_BASE - 159);
        public const int CALCDISC = (ERROR_BASE - 160);
        public const int MAXPRO2S = (ERROR_BASE - 161);
        public const int MAXCTX = (ERROR_BASE - 162);
        public const int MAXBANDSREAD = (ERROR_BASE - 163);
        public const int FREQUENCYUSED = (ERROR_BASE - 164);
        public const int NOBANDCHANGE = (ERROR_BASE - 165);
        public const int BADPASSIVEDATA = (ERROR_BASE - 166);
        public const int NOTEMPDATA = (ERROR_BASE - 167);
        public const int NOLASTANGLE359 = (ERROR_BASE - 168);
        public const int FIVEPAIRS = (ERROR_BASE - 169);
        public const int CANTDELEPCN = (ERROR_BASE - 170);
        public const int RECNOTSAVED = (ERROR_BASE - 171);
        public const int BADESSPHERECALC = (ERROR_BASE - 172);
        public const int NOPLANPDF = (ERROR_BASE - 173);
        public const int NOPLCHID = (ERROR_BASE - 174);
        public const int BADESANALOPT = (ERROR_BASE - 175);


        /* = (ERROR_BASE -200); to = (ERROR_BASE -250); reserved for Dynamic stuff */

        public const int DYN_CUR_NOT_OPEN = (ERROR_BASE - 200);
        public const int DYN_PAST_LAST_ROW = (ERROR_BASE - 201);
        public const int DYN_MS_SQL_SERVER_ERR = (ERROR_BASE - 202);
        public const int DYN_NO_CURSOR = (ERROR_BASE - 203);

        // New error messages added by AH.
        public const int INVALIDNAMEORVALUE = -2;
        public const int ELEVANGLETOOLOW = (ERROR_BASE - 204);
        public const int FILEISEMPTY = (ERROR_BASE - 205);
        public const int IMPORTFILEHASNOPARSABLECONTENT = (ERROR_BASE - 206);
        public const int UNKNOWNSECORDTYPE = (ERROR_BASE - 207);
        public const int TOOFEWCSVFIELDS = (ERROR_BASE - 208);
        public const int FEIMPORTFILEUNKNOWNLINEQUALIFIER = (ERROR_BASE - 209);
        public const int FEIMPORTFILEQUALIFIERINCORRECTNUMBEROFFIELDS = (ERROR_BASE - 210);
        public const int FEIMPORTINVALIDFIRSTQUALIFIER = (ERROR_BASE - 211);
        public const int FEIMPORTFILEQUALIFIERSINVALIDSEQUENCE = (ERROR_BASE - 212);
        public const int FEIMPORTFILEINCOMPLETESITERECORD = (ERROR_BASE - 213);
        public const int FEIMPORTFILEINCOMPLETEANTENNARECORD = (ERROR_BASE - 214);
        public const int FEIMPORTFILEINCOMPLETECHANNELRECORD = (ERROR_BASE - 215);
        public const int ENVVARMICSUSERNOTSET = (ERROR_BASE - 216);
        public const int UNEXPECTEDENDOFSTRING = (ERROR_BASE - 217);
        public const int IMPORTFILEHASNOCONTENT = (ERROR_BASE - 218);
        public const int MANDATEDFIELDHASNOVALUE = (ERROR_BASE - 219);
        public const int MANDATEDIFADDFIELDHASNOVALUE = (ERROR_BASE - 220);
        public const int STRINGISNULL = (ERROR_BASE - 221);
        public const int NOPASSWORD = (ERROR_BASE - 222);
        public const int MICSUSERNOTSET = (ERROR_BASE - 223);
        public const int MICSBINDIRNOTSET = (ERROR_BASE - 224);
        public const int TSIPSENDEMAILFAILED = (ERROR_BASE - 225);
        public const int OBFUSCATIONSETKEYFAILED = (ERROR_BASE - 226);
        public const int PARMTABLEHASNORECORDS = (ERROR_BASE - 227);
        public const int PARMTABLEDOESNOTEXIST = (ERROR_BASE - 228);
        public const int HILOTABLENAMEISEMPTY = (ERROR_BASE - 229);

        public const int INVALIDEMAILADDRESS = (ERROR_BASE - 230);
        public const int EMAILSUBJECTMISSING = (ERROR_BASE - 231);
        public const int EMAILBODYMISSING = (ERROR_BASE - 232);
        public const int EMAILATTACHMENTFILELISTISNULL = (ERROR_BASE - 233);
        public const int EMAILATTACHMENTINVALIDFILEPATH = (ERROR_BASE - 234);
        public const int EMAILSENDATTEMPTFAILED = (ERROR_BASE - 235);
        public const int ATTEMPTTOGETSCHEMAFAILED = (ERROR_BASE - 236);

        public const int USERTABLENOTFOUND = (ERROR_BASE - 237);
        public const int FTTITLEFETCHFAILED = (ERROR_BASE - 238);
        public const int MDBALREADYUPDATED = (ERROR_BASE - 239);

        public const int CANNOTREADIMPORTFILE = (ERROR_BASE - 240);
        public const int CANNOTWRITEEXPORTFILE = (ERROR_BASE - 241);

        public const int CANNOTSETODBCENVIRONMENTATTRIBUTES = (ERROR_BASE - 242);

        public static Error[] Table = new Error[] {
            new Error(USAGE, "USAGE: {0} {1}"),
            new Error(USAGEEDIT, "USAGE: {0} dbname tablename [tablename ...]"),
            new Error(NODATABASE, "Database {0} does not exist"),
            new Error(INVALIDDOMAIN,   "Invalid Domain Id Used"),
            new Error(STACKOVRFLW, "WARNING!  Domain stack overflow"),
            new Error(NOSELECT, "No selection made, please do SELECTion before using {0}"),
            new Error(NOSELECTOPER,  "No selection made, please select operator before using {0}"),
            new Error(MUSTBEON, "Must be on a {0} to {1}"),
            new Error(SELECTREC, "Selected record {0} ..."),
            new Error(NOSOURCE, "No source filename entered for {0}"),
            new Error(NODEST, "No destination filename entered for {0}"),
            new Error(NOFILESELECT, "No filename entered or SELECTed for {0}"),
            new Error(NOOUTPUT, "No output filename entered for {0}"),
            new Error(VALIDSELECT, "Must be on a valid filename to SELECT/unSELECT"),
            new Error(NORECORDS, "No records were selected"),
            new Error(NOMOREREC, "No more records"),
            new Error(ENDOFREC, "End of records - returning to first record"),
            new Error(PARMUPDATE, "Updating parameter tables to include Maximum Frequency Separation..."),
            new Error(ANTDUPDATE, "Updating antenna detail table to include dtilt field..."),
            new Error(NOEDITFILE, "No source file name entered or SELECTed for EDIT"),
            new Error(NODELETEFILE, "No source file name entered or SELECTed for DELETE"),
            new Error(NOSELECTFILE, "No file to SELECT - can only SELECT listed files"),
            new Error(MUST_ENTER, "Must enter a value for field {0}"),
            new Error(ACCHELP, "Accessing HELP. Please wait..."),
            new Error(NYI, "Not Yet Implemented"),
            new Error(WORKING, "Working . . ."),
            new Error(TEXTWRITE, "Writing data to output file {0}"),
            new Error(PROCRECNO, "Processing record number {0}"),
            new Error(USERABANDON, "User abandoned request"),
            new Error(ATTOP, "At Top Of Data"),
            new Error(ATEND, "At End Of Data"),
            new Error(NUMSITES, "{0} site(s) were found within Coordination Boundaries"),
            new Error(BATCHPROC, "BATCH processing begun . . ."),
            new Error(PROCESSING, "Processing . . ."),
            new Error(RECMESS, " Record {0}. "),
            new Error(GENMESS, "{0}"),
            new Error(GENERROR, "{0}"),
            new Error(FILEERROR, "Error Accessing Menu Data File"),
            new Error(MENUDRIVERERROR, "Error code from menu screen driver"),
            new Error(FASTRAKERROR, "ERROR!  Could not rebuild stack"),
            new Error(RECNOTADDED, "ERROR! Record could not be added"),
            new Error(RECNOTUPDATED,   "ERROR! Record could not be updated"),
            new Error(RECNOTSAVED,   "ERROR! Record could not be saved"),
            new Error(RECNOTDELETED,   "ERROR! Record could not be deleted"),
            new Error(RECADDED, "Record has been added"),
            new Error(RECDELETED, "Record has been deleted"),
            new Error(RECUPDATED, "Record has been updated"),
            new Error(RECSAVED, "Record has been saved"),
            new Error(TABLEEXIST, "Table {0} already exists"),
            new Error(TABLECOPIED, "Table(s) has (have) been copied"),
            new Error(TABLEEXPORTED, "Table(s) has (have) been exported"),
            new Error(TABLEPRINTED, "Table(s) has (have) been printed/exported"),
            new Error(TABLEVALIDATED, "Table(s) has (have) been validated"),
            new Error(TABLEIMPORTED, "Table has been imported"),
            new Error(TABLEDELETED, "Table(s) has (have) been deleted"),
            new Error(BADFIELDFORMAT,  "ERROR! Invalid format for field {0}"),
            new Error(BADFORMAT, "ERROR! Invalid format"),
            new Error(TOOMANY, "Too many {0} - Cannot perform {1}"),
            new Error(FILENOTEXIST, "Filename does not exist for {0}"),
            new Error(NOOPER, "Error executing operation - Check {0}"),
            new Error(IMPNOTEXIST, "Import file does not exist"),
            new Error(LOGOPEN, "Error opening Import log file"),
            new Error(ALREADYEXIST, "This {0} already exists - Cannot perform {1}"),
            new Error(FREQUENCYUSED, "This frequency has already been used in the PDF or MDB"),
            new Error(ERRORADDING, "Error adding {0}"),
            new Error(ERRORDELETING,   "Error deleting {0}"),
            new Error(ERRORONS, "Error on {0} of {1}"),
            new Error(FILEWEXIST, "PDF with this name already exists - Cannot do copy"),
            new Error(FILEWCREATERR,   "PDF create error - PDF {0} not created"),
            new Error(CURSOROPEN, "Error opening {0} cursor"),
            new Error(BADDEST, "Invalid destination filename {0}"),
            new Error(BADSOURCE, "Invalid source filename {0}"),
            new Error(BADTABLENAME, "Invalid table name {0}"),
            new Error(NOCREATETABLE, "Could not create table '{0}'"),
            new Error(NOCREATETLIST, "Could not create table list"),
            new Error(USERLISTFULL, "User list is full"),
            new Error(OPCODELISTFULL, "Operator code list is full"),
            new Error(NOMEMOPCODE, "No members operator code for '{0}'"),
            new Error(NOUSERID, "No userid for given operator code"),
            new Error(PRINTFILEERR, "Error opening printer file"),
            new Error(OUTPUTFILEERR,   "Error opening output file {0}"),
            new Error(INPUTFILEERR, "Error opening input file {0}"),
            new Error(OPENOUTFILEERR,  "No data written to output file - Error on open"),
            new Error(REPORT_ERROR, "Error creating Report Writer source file"),
            new Error(DELETEERR, "Error on DELETE for {0} - DELETE failed"),
            new Error(FILEWNOTFOUND,   "PDF {0} not found - DELETE failed"),
            new Error(FILEWNOTCOPY, "PDF {0} not found - COPY failed"),
            new Error(SOURCEEQDEST, " A file cannot be copied onto itself "),
            new Error(NOTABLE, "No table name specified"),
            new Error(NOTITLE, "No TITLE record exists to be updated"),
            new Error(TITLEEXIST,  "TITLE already exists. Only one TITLE per file is allowed"),
            new Error(INVENVTYPE, "Invalid Env. type with an ES proposed file."),
            new Error(BADENVTYPE, "Reenter :INTRA, PDF_ or MDB_ with TS or ES."),
            new Error(BADENVNAME, "Environment file name must be entered."),
            new Error(BADCOORDISTTS, "Coordination distance must be > 0 and < 500.00"),
            new Error(BADCOORDISTES, "Coordination distance must be > 0 and < 9999.99"),
            new Error(BADSELSITE, "Reenter :ALL, ALL EXCEPT SELF, CALL SIGN or OPERATOR CODE."),
            new Error(INVSELSITE, "Invalide selection option for given country."),
            new Error(BADCOUNTRY, "Reenter: CAN, USA or ALL."),
            new Error(BADTSSPHERECALC, "Reenter: Propagation loss models permitted are 1, 2, 3, 4 or 5.\r\n Prop. Loss Models: 1=CCIR-SJM,2=Sph. Earth,3=Free Space,4=PCS HATA,5=OH-LOSS."),
            new Error(BADESSPHERECALC, "Reenter: Propagation loss models permitted are 1 or 2.\r\n Propagation Loss Models: 1=CCIR-SJM, 2=Spherical Earth."),
            new Error(BADTSORB, "Reenter: Y for yes or N for no."),
            new Error(BADCHANCODE, "Reenter: Channel status codes from 0 to 9 or ALL."),
            new Error(BADCALLSIGN, "Call signs must be entered."),
            new Error(BADOPCODE, "Operator codes must be entered."),
            new Error(DEFAULT_CTX, "\tUsing default CTX Table.\r\n"),
            new Error(AZIMDEFAULT, "\tNo azim. data found for antenna {0} at location {1}.\r\n\tDefault used.\r\n"),
            new Error(USEWORSTTS, "\tDefault Antenna Table 'WORST TS' Used\r\n"),
            new Error(USEWORSTES, "\tDefault Antenna Table 'WORST ES' Used\r\n"),
            new Error(NOEQPTFOUND, "\tNo Equiptment data found in {0} with {1}\r\n"),
            new Error(NOTRAFFOUND, "\tNo Traffic data found in sd_traf with {0}\r\n"),
            new Error(NOCTXFOUND, "\tNo Ctx data found in {0} with {1}\r\n"),
            new Error(NOCTXDPTS, "\tNo Ctx Points found in {0} with {1}\r\n"),
            new Error(NOANTEFOUND, "\tNo Antenna data found in {0} with {1}\r\n"),
            new Error(NOANTDFOUND, "\tNo Antenna Points found in {0} with {1}\r\n"),
            new Error(MAXPARMS, "\tMaximum number ({0}) of parameter records read\r\n"),
            new Error(MAXANTES, "\tMaximum number ({0}) of antenna records read\r\n"),
            new Error(MAXBANDSREAD, "\tMaximum number ({0}) of band table records read\r\n"),
            new Error(MAXCTX, "\tMaximum number ({0}) of ctx records read\r\n"),
            new Error(MAXTMP2S, "\tMaximum number ({0}) of victim site pairs read\r\n"),
            new Error(MAXPRO2S, "\tMaximum number ({0}) of interferer site pairs read\r\n"),
            new Error(INVBNDCDE, "\tInvalid Band Code used\r\n"),
            new Error(INVBNDBIT, "\tInvalid Band Bit Position used\r\n"),
            new Error(SINGLEDIR_ADISC, "\tError determining total ante disc between 2 single directional antenna\r\n"),
            new Error(WRST_ADISC, "\tError calculating Worst Antenna Discrimination\r\n"),
            new Error(INVALIDBANDCODE, "\tRecord with bndcde = {0} not found in sd_band table\r\n"),
            new Error(NOVICCHID, "\tBAND Calculations - No victim channel record\r\n"),
            new Error(FETCH_FAIL, "\tNo {0} data found where {1}\r\n"),
            new Error(CALCDISC, "\tError calulating discrimination for angle {0}\r\n"),
            new Error(BEAMHITS, "\tNo Solution: Beam hits the ground\r\n"),
            new Error(NOCONVERG, "\tIn SATAZE: No convergence to TAU\r\n"),
            new Error(NEGLOG, "\tNegative value to log in {0}\r\n"),
            new Error(FILEWDNE, "{0} PDF {1} not found"),
            new Error(TABLEDNE, "Table {0} not found - not processed"),
            new Error(ZONESUM, "The sum of the 3 zones must be 100 percent"),
            new Error(CONTINUE, "To continue "),
            new Error(CCKNOSITE, "No site info found for {0}S PDF {1}"),
            new Error(NOPATT, "Area or Frequency too small to calculate Pattern"),
            new Error(FILEWVAL, "Error determining validation of PDF"),
            new Error(NOTPVIEW, "*** Could not create view to run reports"),
            new Error(TOOMANYCODES, "Maximum number of codes accepted is {0}"),
            new Error(INVCULLCODE, "Invalid Cull Code"),
            new Error(INVREPTYPE, "Invalid Report Type"),
            new Error(NOTPCNVALID, "PDF '{0}' has not been validated for PCN"),
            new Error(NOTTSIPVALID, "{0} File '{1}' has not been validated for Tsip"),
            new Error(NOTSLAVALID, "{0} PDF '{1}' has not been validated for S.L.A"),
            new Error(NOTMDBUPDVAL, "{0} PDF '{1}' has not been validated for MDB Update"),
            new Error(ALREADYPOSTED, "{0} PDF '{1}' has already been posted to the MDB;\r\n {2} will not be performed"),
            new Error(NOPRONAME, "Proposed file name must be entered."),
            new Error(BADANALOPT, "Reenter: BAND, CHAN, or PLAN"),
            new Error(BADESANALOPT,  "Reenter: BAND or CHAN"),
            new Error(TEMPDNE, "Tsip Temporary {0} table '{1}' not found"),
            new Error(PARAMNOTEXIST, "Parameter file {0} does not exist."),
            new Error(BADPROV, "Invalid Province"),
            new Error(BADDATE, "Invalid Date"),
            new Error(BADTIME, "Invalid Time"),
            new Error(INVMONTH, "Invalid Month"),
            new Error(BADLAT, "Invalid Latitude"),
            new Error(BADLONG, "Invalid Longitude"),
            new Error(IS_INVALID, "{0} is invalid"),
            new Error(BAD_CULL, "The cull :{0}: is invalid"),
            new Error(DYN_CUR_NOT_OPEN,   "Cursor is not open"),
            new Error(DYN_PAST_LAST_ROW,  "No more records. Can't DELETE or UPDATE"),
            new Error(DYN_MS_SQL_SERVER_ERR, "Dynamic SQL Ingres error"),
            new Error(DYN_NO_CURSOR, "No more dynamic cursors available"),
            new Error(TOOFEWANGLES, "Too few antenna angles are specified"),
            new Error(NOLASTANGLE, "No row specified for 180.0 degrees"),
            new Error(NOFIRSTANGLE, "No row specified for 0.0 degrees"),
            new Error(NOGENOPCODELIST,  "Could not create Operator Code list"),
            new Error(PCNGEN, "PCN notification generated."),
            new Error(NOPCNGEN, "Could not generate PCN notification"),
            new Error(SLAGEN, "Shared Link Approval request generated."),
            new Error(NOSLAGEN, "Could not generate Shared Link Approval request"),
            new Error(NOGENMAILMESS,  "Could not create mail message"),
            new Error(NOSENDMAIL,  "Could not send mail message"),
            new Error(MUSTENTERDEST, "Must enter a value for Plot Filename"),
            new Error(MUSTENTERLOC, "Must enter a value for Location"),
            new Error(MUSTENTERTFCI, "Must enter a value for Interference Traffic Code"),
            new Error(MUSTENTERTFCR, "Must enter a value for Receive Traffic Code"),
            new Error(MUSTENTERRXEQPT, "Must enter a value for Receive Equipment Type"),
            new Error(MUSTENTERACODE, "Must enter a value for Antenna Code"),
            new Error(MUSTENTERCALL1, "Must enter a value for Call Sign"),
            new Error(MUSTENTERCHID, "Must enter a value for Channel Id"),
            new Error(MUSTENTERREFIND, "Must enter a value for Refractive Index"),
            new Error(NOCHANGE,  "No changes to screen.  Record NOT added"),
            new Error(NOTOWNER,  "You are not the owner.  Record NOT deleted"),
            new Error(PROGNOTDEFINED, "Program not defined"),
            new Error(PR_RESOLVED,  "Problem/Resolution already resolved"),
            new Error(NOPCODES,     "No project codes found for MICS user"),
            new Error(INVPCODE,  "Invalid project code"),
            new Error(CHGPCODE,  "Project Code has been changed"),
            new Error(INVHILO, "Invalid HiLo code. Cannot generate channels."),
            new Error(INVMDBO, "Invalid MDB operation.  Cannot generate channels."),
            new Error(INVBAND, "Invalid Band or Plan.  Cannot generate channels."),
            new Error(CHIDRXTX, "Chid defined with no Rx or Tx Status"),
            new Error(CHANGEN, "{0} Channels Generated from PLAN"),
            new Error(NOCHANGEN, "No Channels Generated from PLAN"),
            new Error(NOCONSM, "Could not connect to SHARED MEMORY"),
            new Error(NOSMENTRY, "SHARED MEMORY entry not found"),
            new Error(NOSMAVAIL, "SHARED MEMORY full, no memory available"),
            new Error(NOINTERP, "Insufficent azimuth info. Could not interpolate."),
            new Error(FE_TX_RX_GROUP, "TX/RX data must ALL be present or ALL omitted"),
            new Error(FE_TX_RX_MISSING,  "TX and/or RX data must be present"),
            new Error(NOTINUSERTBL, "Not in user table"),
            new Error(NOLOGINFILE, "No login file"),
            new Error(NOPRINTACCTFILE, "No print accounting file /usr/adm/lpacct_sum"),
            new Error(NODELACCTFILE, "Can't delete acct file"),
            new Error(UNKNOWNUSER, "Unknown user"),
            new Error(PROGNOTFOUND, "Program not found"),
            new Error(ABNORMTERM, "Abnormal termination of sub-task"),
            new Error(NOPRINTER, "Can't access printer device"),
            new Error(INVDEST, "Can't open destination output file"),
            new Error(NOCONNECT, "Can't connect to database"),
            new Error(NOEXECUTE, "Can't execute program"),
            new Error(NOCOMPLETE, "Could not complete requested function"),
            new Error(INVPARM, "Invalid parameters used for execution of subtask"),
            new Error(PERMISSIONDENIED, "Permission denied.  Selected option not available to this user."),
            new Error(NOTEXECUTABLE,  "Program cannot be executed from the command line."),
            new Error(FETCHERROR, "Error in fetching next record from the database."),
            new Error(BADSCREENWRITE, "Error writing information to the screen."),
            new Error(BADSCREENREAD, "Error reading information from the screen."),
            new Error(MUSTBEYORN, "Field must be one of 'Y' or 'N' only."),
            new Error(PERMISSIONERROR, "Error determining access rights."),
            new Error(NOPCODETODEL, "Warning!  No project codes were deleted."),
            new Error(BADULTRIXID, "Ultrix ID not valid.  No such Ultrix user."),
            new Error(CHANGESSAVED, "Changes have been saved."),
            new Error(BADOPERCODE, "Operator Code not valid."),
            new Error(BADCORRESP, "Ultrixid or Operator Code already in use."),
            new Error(TOOFEWCODES, "Minimum number of codes accepted is {0}"),
            new Error(PAIRNOTUNIQUE, "Ultrix ID and MICS ID are not a unique pair."),
            new Error(RECEXISTS, "This record already exists."),
            new Error(NODATA, "No data for selected year."),
            new Error(BADTABLE, "Inconsistant data found in Ingres table."),
            new Error(NOREDIRECT, "Cannot redirect output of this program to a file"),
            new Error(LOADINGSELS, "Loading {0} selections.  Please wait ..."),
            new Error(BADFILECOPY, "Error in copying file:  {0}"),
            new Error(NOBANDCHANGE,  "No band related changes in PDF; no coordination required. "),
            new Error(BADPASSIVEDATA,  "There is incomplete passive link information in the PDF.  Validate canceled."),
            new Error(NOTEMPDATA, "The PDF contains temporary data but no temporary table was specified."),
            new Error(NOLASTANGLE359, "No row specified for 359.9 degrees"),
            new Error(FIVEPAIRS, "There may be no more than five pairs in any cull."),
            new Error(CANTDELEPCN, "PDF '{0}' still awaiting EPCN response (cannot delete)."),
            new Error(NOPLANPDF, "Could not generate temporary PDF for PLAN mode analysis."),
            new Error(NOPLCHID,  "Channel ID must not begin with letters 'pl'."),
            new Error(ELEVANGLETOOLOW,  "Elevation angle is too low."),

            // New error messages added by AH.
            new Error(ODBC_COULD_NOT_GET_ENV_HANDLE, "Could not get ODBC Environment Handle."),
            new Error(ODBC_COULD_NOT_GET_CONNECT_HANDLE, "Could not get an ODBC Connection Handle."),
            new Error(ODBC_SQLCONNECT_FAILED, "SQLConnect() failed."),
            new Error(FILEISEMPTY,  "File has zero bytes."),
            new Error(IMPORTFILEHASNOPARSABLECONTENT,  "File has no parsable content."),
            new Error(UNKNOWNSECORDTYPE,  "Unknown ES record type."),
            new Error(TOOFEWCSVFIELDS,  "Too few Comma-Separated-Value fields."),
            new Error(FEIMPORTFILEUNKNOWNLINEQUALIFIER, "Unknown line type/qualifier."),
            new Error(FEIMPORTFILEQUALIFIERINCORRECTNUMBEROFFIELDS, "Invalid number of CSV fields for line type/qualifier."),
            new Error(FEIMPORTINVALIDFIRSTQUALIFIER, "First type/qualifier must be 'TE'"),
            new Error(FEIMPORTFILEQUALIFIERSINVALIDSEQUENCE, "Invalid sequence of lines w.r.t. type/qualifiers"),
            new Error(FEIMPORTFILEINCOMPLETESITERECORD, "Incomplete Site record; missing line."),
            new Error(FEIMPORTFILEINCOMPLETEANTENNARECORD, "Incomplete Antenna record; missing line."),
            new Error(FEIMPORTFILEINCOMPLETECHANNELRECORD, "Incomplete Channel record; missing line."),
            new Error(ENVVARMICSUSERNOTSET, "The Environment Variable MICSUSER is not set."),
            new Error(UNEXPECTEDENDOFSTRING, "Unexpected end of string encountered while parsing."),
            new Error(INVALIDNAMEORVALUE, "Invalid name or value."),
            new Error(IMPORTFILEHASNOCONTENT, "The Import File has no meaningful content."),
            new Error(MANDATEDFIELDHASNOVALUE, "A mandatory field has no value assigned."),
            new Error(MANDATEDIFADDFIELDHASNOVALUE, "A mandatory field for an Add operation has no value assigned."),
            new Error(STRINGISNULL, "A string value is NULL but a non-null value is required."),
            new Error(NOPASSWORD, "PASSWORD not set in Windows Environment."),
            new Error(MICSBINDIRNOTSET, "Could not get the path of the directory containing the MICS binaries."),
            new Error(TSIPSENDEMAILFAILED, "Attempt to send TSIP reports email to user failed."),
            new Error(OBFUSCATIONSETKEYFAILED, "Attempt to set the obfuscation key failed."),
            new Error(PARMTABLEHASNORECORDS, "The run parameters table has no records."),
            new Error(PARMTABLEDOESNOTEXIST, "The run parameters table does not exist."),
            new Error(HILOTABLENAMEISEMPTY, "The table name is null or empty."),

            new Error(INVALIDEMAILADDRESS, "The email address string is NULL or has invalid syntax."),
            new Error(EMAILSUBJECTMISSING, "The email subject string is NULL or empty."),
            new Error(EMAILBODYMISSING, "The email body string is NULL."),
            new Error(EMAILATTACHMENTFILELISTISNULL, "The list of file paths to attach is NULL."),
            new Error(EMAILATTACHMENTINVALIDFILEPATH, "One or more of the attachment file paths is invalid."),
            new Error(EMAILSENDATTEMPTFAILED, "Attempt to send email failed - exception thrown."),
            new Error(ATTEMPTTOGETSCHEMAFAILED, "Attempt to get user's schema from SQL Server failed."),

            new Error(USERTABLENOTFOUND, "One or more user ft_ tables was not found in the database."),
            new Error(FTTITLEFETCHFAILED, "FETCH FROM user table ft_XXX_titl failed."),
            new Error(MDBALREADYUPDATED, "These user tables have already updated the MDB."),

            new Error(CANNOTREADIMPORTFILE, "Cannot read from import file."),
            new Error(CANNOTWRITEEXPORTFILE, "Cannot write to export file."),

            new Error(CANNOTSETODBCENVIRONMENTATTRIBUTES, "Cannot set the ODBC environment attributes.")
    };

        /// <summary>
        /// This method returns the error message associated with 
        /// an error code (number).
        /// </summary>
        /// <param name="errorCode"> - eponym.</param>
        /// <returns></returns>
        public static string MsgForCode(int errorCode)
        {
            string result = "N/A";

            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i].errNum == errorCode)
                {
                    result = Table[i].errMsg;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This methods returns a multi-line CSV formatted text string that provides a listing of
        /// all of the MICS# numerical error codes defined in this class, together with the name
        /// of the associated C# integer constant and brief descriptive message.
        /// </summary>
        /// <returns></returns>
        public static string ToStringAllErrorCodesAsCSV()
        {
            StringBuilder sb = new StringBuilder();

            // Exit codes
            sb.Append(String.Format("\nCOMMAND_LINE_ERROR,{0},\"{1}\"", COMMAND_LINE_ERROR, MsgForCode(COMMAND_LINE_ERROR)));
            sb.Append(String.Format("\nFATAL_EXCEPTION,{0},\"{1}\"", FATAL_EXCEPTION, MsgForCode(FATAL_EXCEPTION)));
            sb.Append(String.Format("\nENVIRONMENT_VARIABLE_SYNTAX_ERROR,{0},\"{1}\"", ENVIRONMENT_VARIABLE_SYNTAX_ERROR, MsgForCode(ENVIRONMENT_VARIABLE_SYNTAX_ERROR)));
            sb.Append(String.Format("\nUNABLE_TO_ENTER_QUEUE,{0},\"{1}\"", UNABLE_TO_ENTER_QUEUE, MsgForCode(UNABLE_TO_ENTER_QUEUE)));
            // Job Queueing problems.
            sb.Append(String.Format("\nALREADY_IN_QUEUE,{0},\"{1}\"", ALREADY_IN_QUEUE, MsgForCode(ALREADY_IN_QUEUE)));
            sb.Append(String.Format("\nJOB_HAS_NO_ROOM_TO_RUN,{0},\"{1}\"", JOB_HAS_NO_ROOM_TO_RUN, MsgForCode(JOB_HAS_NO_ROOM_TO_RUN)));
            sb.Append(String.Format("\nJOB_IN_QUEUE_HAS_BEEN_DELETED,{0},\"{1}\"", JOB_IN_QUEUE_HAS_BEEN_DELETED, MsgForCode(JOB_IN_QUEUE_HAS_BEEN_DELETED)));
            // ODBC error messages.
            sb.Append(String.Format("\nODBC_GET_FAILED,{0},\"{1}\"", ODBC_GET_FAILED, MsgForCode(ODBC_GET_FAILED)));
            sb.Append(String.Format("\nODBC_EXECUTE_FAILED,{0},\"{1}\"", ODBC_EXECUTE_FAILED, MsgForCode(ODBC_EXECUTE_FAILED)));
            sb.Append(String.Format("\nODBC_EXECDIRECT_FAILED,{0},\"{1}\"", ODBC_EXECDIRECT_FAILED, MsgForCode(ODBC_EXECDIRECT_FAILED)));
            sb.Append(String.Format("\nODBC_BINDING_FAILED,{0},\"{1}\"", ODBC_BINDING_FAILED, MsgForCode(ODBC_BINDING_FAILED)));
            sb.Append(String.Format("\nODBC_QUERY_FAILED,{0},\"{1}\"", ODBC_QUERY_FAILED, MsgForCode(ODBC_QUERY_FAILED)));
            sb.Append(String.Format("\nODBC_SELECT_FAILED,{0},\"{1}\"", ODBC_SELECT_FAILED, MsgForCode(ODBC_SELECT_FAILED)));
            sb.Append(String.Format("\nODBC_FETCH_FAILED,{0},\"{1}\"", ODBC_FETCH_FAILED, MsgForCode(ODBC_FETCH_FAILED)));
            sb.Append(String.Format("\nODBC_PREPARE_FAILED,{0},\"{1}\"", ODBC_PREPARE_FAILED, MsgForCode(ODBC_PREPARE_FAILED)));
            sb.Append(String.Format("\nODBC_SQLALLOCHANDLE_FAILED,{0},\"{1}\"", ODBC_SQLALLOCHANDLE_FAILED, MsgForCode(ODBC_SQLALLOCHANDLE_FAILED)));
            sb.Append(String.Format("\nODBC_INVALID_FETCH_COUNT,{0},\"{1}\"", ODBC_INVALID_FETCH_COUNT, MsgForCode(ODBC_INVALID_FETCH_COUNT)));
            sb.Append(String.Format("\nODBC_NULL_HANDLE,{0},\"{1}\"", ODBC_NULL_HANDLE, MsgForCode(ODBC_NULL_HANDLE)));
            sb.Append(String.Format("\nODBC_COULD_NOT_GET_ENV_HANDLE,{0},\"{1}\"", ODBC_COULD_NOT_GET_ENV_HANDLE, MsgForCode(ODBC_COULD_NOT_GET_ENV_HANDLE)));
            sb.Append(String.Format("\nODBC_COULD_NOT_GET_CONNECT_HANDLE,{0},\"{1}\"", ODBC_COULD_NOT_GET_CONNECT_HANDLE, MsgForCode(ODBC_COULD_NOT_GET_CONNECT_HANDLE)));
            sb.Append(String.Format("\nODBC_SQLCONNECT_FAILED,{0},\"{1}\"", ODBC_SQLCONNECT_FAILED, MsgForCode(ODBC_SQLCONNECT_FAILED)));
            sb.Append(String.Format("\nNO_CURSOR_AVAILABLE,{0},\"{1}\"", NO_CURSOR_AVAILABLE, MsgForCode(NO_CURSOR_AVAILABLE)));
            sb.Append(String.Format("\nCOULD_NOT_CREATE_MUTEX,{0},\"{1}\"", COULD_NOT_CREATE_MUTEX, MsgForCode(COULD_NOT_CREATE_MUTEX)));
            sb.Append(String.Format("\nMUTEX_ALREADY_OWNED,{0},\"{1}\"", MUTEX_ALREADY_OWNED, MsgForCode(MUTEX_ALREADY_OWNED)));
            sb.Append(String.Format("\nUTCVTNAME_FAILED,{0},\"{1}\"", UTCVTNAME_FAILED, MsgForCode(UTCVTNAME_FAILED)));
            sb.Append(String.Format("\nMYBASE,{0},\"{1}\"", MYBASE, MsgForCode(MYBASE)));
            sb.Append(String.Format("\nINVALID_DATA,{0},\"{1}\"", INVALID_DATA, MsgForCode(INVALID_DATA)));
            sb.Append(String.Format("\nCACHE_WRITE_FAILED,{0},\"{1}\"", CACHE_WRITE_FAILED, MsgForCode(CACHE_WRITE_FAILED)));
            sb.Append(String.Format("\nCACHE_READ_FAILED,{0},\"{1}\"", CACHE_READ_FAILED, MsgForCode(CACHE_READ_FAILED)));
            sb.Append(String.Format("\nINVALID_ARGUMENTS,{0},\"{1}\"", INVALID_ARGUMENTS, MsgForCode(INVALID_ARGUMENTS)));
            sb.Append(String.Format("\nRX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE,{0},\"{1}\"", RX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE, MsgForCode(RX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE)));
            sb.Append(String.Format("\nTX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE,{0},\"{1}\"", TX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE, MsgForCode(TX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE)));
            sb.Append(String.Format("\nVICTIM_TYPE_NOT_A_OR_D,{0},\"{1}\"", VICTIM_TYPE_NOT_A_OR_D, MsgForCode(VICTIM_TYPE_NOT_A_OR_D)));
            sb.Append(String.Format("\nRC_BASE,{0},\"{1}\"", RC_BASE, MsgForCode(RC_BASE)));
            sb.Append(String.Format("\nWARN_BASE,{0},\"{1}\"", WARN_BASE, MsgForCode(WARN_BASE)));
            sb.Append(String.Format("\nERROR_BASE,{0},\"{1}\"", ERROR_BASE, MsgForCode(ERROR_BASE)));
            sb.Append(String.Format("\nINVDEST,{0},\"{1}\"", INVDEST, MsgForCode(INVDEST)));
            sb.Append(String.Format("\nABNORMTERM,{0},\"{1}\"", ABNORMTERM, MsgForCode(ABNORMTERM)));
            sb.Append(String.Format("\nNOPRINTER,{0},\"{1}\"", NOPRINTER, MsgForCode(NOPRINTER)));
            sb.Append(String.Format("\nPROGNOTFOUND,{0},\"{1}\"", PROGNOTFOUND, MsgForCode(PROGNOTFOUND)));
            sb.Append(String.Format("\nINVPARM,{0},\"{1}\"", INVPARM, MsgForCode(INVPARM)));
            sb.Append(String.Format("\nNOCONNECT,{0},\"{1}\"", NOCONNECT, MsgForCode(NOCONNECT)));
            sb.Append(String.Format("\nNOEXECUTE,{0},\"{1}\"", NOEXECUTE, MsgForCode(NOEXECUTE)));
            sb.Append(String.Format("\nNOCOMPLETE,{0},\"{1}\"", NOCOMPLETE, MsgForCode(NOCOMPLETE)));
            sb.Append(String.Format("\nNODATA,{0},\"{1}\"", NODATA, MsgForCode(NODATA)));
            sb.Append(String.Format("\nRECMESS,{0},\"{1}\"", RECMESS, MsgForCode(RECMESS)));
            sb.Append(String.Format("\nGENMESS,{0},\"{1}\"", GENMESS, MsgForCode(GENMESS)));
            sb.Append(String.Format("\nINVALIDDOMAIN,{0},\"{1}\"", INVALIDDOMAIN, MsgForCode(INVALIDDOMAIN)));
            sb.Append(String.Format("\nSTACKOVRFLW,{0},\"{1}\"", STACKOVRFLW, MsgForCode(STACKOVRFLW)));
            sb.Append(String.Format("\nNOSELECT,{0},\"{1}\"", NOSELECT, MsgForCode(NOSELECT)));
            sb.Append(String.Format("\nMUSTBEON,{0},\"{1}\"", MUSTBEON, MsgForCode(MUSTBEON)));
            sb.Append(String.Format("\nSELECTREC,{0},\"{1}\"", SELECTREC, MsgForCode(SELECTREC)));
            sb.Append(String.Format("\nNOFILESELECT,{0},\"{1}\"", NOFILESELECT, MsgForCode(NOFILESELECT)));
            sb.Append(String.Format("\nNOSOURCE,{0},\"{1}\"", NOSOURCE, MsgForCode(NOSOURCE)));
            sb.Append(String.Format("\nNODEST,{0},\"{1}\"", NODEST, MsgForCode(NODEST)));
            sb.Append(String.Format("\nNOOUTPUT,{0},\"{1}\"", NOOUTPUT, MsgForCode(NOOUTPUT)));
            sb.Append(String.Format("\nVALIDSELECT,{0},\"{1}\"", VALIDSELECT, MsgForCode(VALIDSELECT)));
            sb.Append(String.Format("\nNORECORDS,{0},\"{1}\"", NORECORDS, MsgForCode(NORECORDS)));
            sb.Append(String.Format("\nNOMOREREC,{0},\"{1}\"", NOMOREREC, MsgForCode(NOMOREREC)));
            sb.Append(String.Format("\nNOEDITFILE,{0},\"{1}\"", NOEDITFILE, MsgForCode(NOEDITFILE)));
            sb.Append(String.Format("\nNODELETEFILE,{0},\"{1}\"", NODELETEFILE, MsgForCode(NODELETEFILE)));
            sb.Append(String.Format("\nNOSELECTFILE,{0},\"{1}\"", NOSELECTFILE, MsgForCode(NOSELECTFILE)));
            sb.Append(String.Format("\nRECADDED,{0},\"{1}\"", RECADDED, MsgForCode(RECADDED)));
            sb.Append(String.Format("\nRECUPDATED,{0},\"{1}\"", RECUPDATED, MsgForCode(RECUPDATED)));
            sb.Append(String.Format("\nRECDELETED,{0},\"{1}\"", RECDELETED, MsgForCode(RECDELETED)));
            sb.Append(String.Format("\nNOTITLE,{0},\"{1}\"", NOTITLE, MsgForCode(NOTITLE)));
            sb.Append(String.Format("\nTITLEEXIST,{0},\"{1}\"", TITLEEXIST, MsgForCode(TITLEEXIST)));
            sb.Append(String.Format("\nACCHELP,{0},\"{1}\"", ACCHELP, MsgForCode(ACCHELP)));
            sb.Append(String.Format("\nNYI,{0},\"{1}\"", NYI, MsgForCode(NYI)));
            sb.Append(String.Format("\nWORKING,{0},\"{1}\"", WORKING, MsgForCode(WORKING)));
            sb.Append(String.Format("\nTEXTWRITE,{0},\"{1}\"", TEXTWRITE, MsgForCode(TEXTWRITE)));
            sb.Append(String.Format("\nPROCRECNO,{0},\"{1}\"", PROCRECNO, MsgForCode(PROCRECNO)));
            sb.Append(String.Format("\nDEFAULT_CTX,{0},\"{1}\"", DEFAULT_CTX, MsgForCode(DEFAULT_CTX)));
            sb.Append(String.Format("\nTABLECOPIED,{0},\"{1}\"", TABLECOPIED, MsgForCode(TABLECOPIED)));
            sb.Append(String.Format("\nTABLEPRINTED,{0},\"{1}\"", TABLEPRINTED, MsgForCode(TABLEPRINTED)));
            sb.Append(String.Format("\nTABLEDELETED,{0},\"{1}\"", TABLEDELETED, MsgForCode(TABLEDELETED)));
            sb.Append(String.Format("\nTABLEIMPORTED,{0},\"{1}\"", TABLEIMPORTED, MsgForCode(TABLEIMPORTED)));
            sb.Append(String.Format("\nTABLEEXIST,{0},\"{1}\"", TABLEEXIST, MsgForCode(TABLEEXIST)));
            sb.Append(String.Format("\nAZIMDEFAULT,{0},\"{1}\"", AZIMDEFAULT, MsgForCode(AZIMDEFAULT)));
            sb.Append(String.Format("\nUSEWORSTTS,{0},\"{1}\"", USEWORSTTS, MsgForCode(USEWORSTTS)));
            sb.Append(String.Format("\nUSEWORSTES,{0},\"{1}\"", USEWORSTES, MsgForCode(USEWORSTES)));
            sb.Append(String.Format("\nTABLEEXPORTED,{0},\"{1}\"", TABLEEXPORTED, MsgForCode(TABLEEXPORTED)));
            sb.Append(String.Format("\nTABLEVALIDATED,{0},\"{1}\"", TABLEVALIDATED, MsgForCode(TABLEVALIDATED)));
            sb.Append(String.Format("\nPCNGEN,{0},\"{1}\"", PCNGEN, MsgForCode(PCNGEN)));
            sb.Append(String.Format("\nSLAGEN,{0},\"{1}\"", SLAGEN, MsgForCode(SLAGEN)));
            sb.Append(String.Format("\nUSERABANDON,{0},\"{1}\"", USERABANDON, MsgForCode(USERABANDON)));
            sb.Append(String.Format("\nCHGPCODE,{0},\"{1}\"", CHGPCODE, MsgForCode(CHGPCODE)));
            sb.Append(String.Format("\nCHANGEN,{0},\"{1}\"", CHANGEN, MsgForCode(CHANGEN)));
            sb.Append(String.Format("\nNOCHANGEN,{0},\"{1}\"", NOCHANGEN, MsgForCode(NOCHANGEN)));
            sb.Append(String.Format("\nATTOP,{0},\"{1}\"", ATTOP, MsgForCode(ATTOP)));
            sb.Append(String.Format("\nATEND,{0},\"{1}\"", ATEND, MsgForCode(ATEND)));
            sb.Append(String.Format("\nNUMSITES,{0},\"{1}\"", NUMSITES, MsgForCode(NUMSITES)));
            sb.Append(String.Format("\nBATCHPROC,{0},\"{1}\"", BATCHPROC, MsgForCode(BATCHPROC)));
            sb.Append(String.Format("\nNOPCODETODEL,{0},\"{1}\"", NOPCODETODEL, MsgForCode(NOPCODETODEL)));
            sb.Append(String.Format("\nCHANGESSAVED,{0},\"{1}\"", CHANGESSAVED, MsgForCode(CHANGESSAVED)));
            sb.Append(String.Format("\nRECSAVED,{0},\"{1}\"", RECSAVED, MsgForCode(RECSAVED)));
            sb.Append(String.Format("\nPROCESSING,{0},\"{1}\"", PROCESSING, MsgForCode(PROCESSING)));
            sb.Append(String.Format("\nLOADINGSELS,{0},\"{1}\"", LOADINGSELS, MsgForCode(LOADINGSELS)));
            sb.Append(String.Format("\nENDOFREC,{0},\"{1}\"", ENDOFREC, MsgForCode(ENDOFREC)));
            sb.Append(String.Format("\nPARMUPDATE,{0},\"{1}\"", PARMUPDATE, MsgForCode(PARMUPDATE)));
            sb.Append(String.Format("\nANTDUPDATE,{0},\"{1}\"", ANTDUPDATE, MsgForCode(ANTDUPDATE)));
            sb.Append(String.Format("\nGENERROR,{0},\"{1}\"", GENERROR, MsgForCode(GENERROR)));
            sb.Append(String.Format("\nFILEERROR,{0},\"{1}\"", FILEERROR, MsgForCode(FILEERROR)));
            sb.Append(String.Format("\nMENUDRIVERERROR,{0},\"{1}\"", MENUDRIVERERROR, MsgForCode(MENUDRIVERERROR)));
            sb.Append(String.Format("\nFASTRAKERROR,{0},\"{1}\"", FASTRAKERROR, MsgForCode(FASTRAKERROR)));
            sb.Append(String.Format("\nRECNOTADDED,{0},\"{1}\"", RECNOTADDED, MsgForCode(RECNOTADDED)));
            sb.Append(String.Format("\nRECNOTUPDATED,{0},\"{1}\"", RECNOTUPDATED, MsgForCode(RECNOTUPDATED)));
            sb.Append(String.Format("\nRECNOTDELETED,{0},\"{1}\"", RECNOTDELETED, MsgForCode(RECNOTDELETED)));
            sb.Append(String.Format("\nBADFIELDFORMAT,{0},\"{1}\"", BADFIELDFORMAT, MsgForCode(BADFIELDFORMAT)));
            sb.Append(String.Format("\nBADFORMAT,{0},\"{1}\"", BADFORMAT, MsgForCode(BADFORMAT)));
            sb.Append(String.Format("\nTOOMANY,{0},\"{1}\"", TOOMANY, MsgForCode(TOOMANY)));
            sb.Append(String.Format("\nFILENOTEXIST,{0},\"{1}\"", FILENOTEXIST, MsgForCode(FILENOTEXIST)));
            sb.Append(String.Format("\nALREADYEXIST,{0},\"{1}\"", ALREADYEXIST, MsgForCode(ALREADYEXIST)));
            sb.Append(String.Format("\nIMPNOTEXIST,{0},\"{1}\"", IMPNOTEXIST, MsgForCode(IMPNOTEXIST)));
            sb.Append(String.Format("\nNOOPER,{0},\"{1}\"", NOOPER, MsgForCode(NOOPER)));
            sb.Append(String.Format("\nLOGOPEN,{0},\"{1}\"", LOGOPEN, MsgForCode(LOGOPEN)));
            sb.Append(String.Format("\nERRORADDING,{0},\"{1}\"", ERRORADDING, MsgForCode(ERRORADDING)));
            sb.Append(String.Format("\nERRORDELETING,{0},\"{1}\"", ERRORDELETING, MsgForCode(ERRORDELETING)));
            sb.Append(String.Format("\nERRORONS,{0},\"{1}\"", ERRORONS, MsgForCode(ERRORONS)));
            sb.Append(String.Format("\nFILEWEXIST,{0},\"{1}\"", FILEWEXIST, MsgForCode(FILEWEXIST)));
            sb.Append(String.Format("\nFILEWCREATERR,{0},\"{1}\"", FILEWCREATERR, MsgForCode(FILEWCREATERR)));
            sb.Append(String.Format("\nPRINTFILEERR,{0},\"{1}\"", PRINTFILEERR, MsgForCode(PRINTFILEERR)));
            sb.Append(String.Format("\nOUTPUTFILEERR,{0},\"{1}\"", OUTPUTFILEERR, MsgForCode(OUTPUTFILEERR)));
            sb.Append(String.Format("\nCURSOROPEN,{0},\"{1}\"", CURSOROPEN, MsgForCode(CURSOROPEN)));
            sb.Append(String.Format("\nDELETEERR,{0},\"{1}\"", DELETEERR, MsgForCode(DELETEERR)));
            sb.Append(String.Format("\nFILEWNOTFOUND,{0},\"{1}\"", FILEWNOTFOUND, MsgForCode(FILEWNOTFOUND)));
            sb.Append(String.Format("\nFILEWNOTCOPY,{0},\"{1}\"", FILEWNOTCOPY, MsgForCode(FILEWNOTCOPY)));
            sb.Append(String.Format("\nBADDEST,{0},\"{1}\"", BADDEST, MsgForCode(BADDEST)));
            sb.Append(String.Format("\nSOURCEEQDEST,{0},\"{1}\"", SOURCEEQDEST, MsgForCode(SOURCEEQDEST)));
            sb.Append(String.Format("\nBADPROV,{0},\"{1}\"", BADPROV, MsgForCode(BADPROV)));
            sb.Append(String.Format("\nBADDATE,{0},\"{1}\"", BADDATE, MsgForCode(BADDATE)));
            sb.Append(String.Format("\nBADLAT,{0},\"{1}\"", BADLAT, MsgForCode(BADLAT)));
            sb.Append(String.Format("\nBADLONG,{0},\"{1}\"", BADLONG, MsgForCode(BADLONG)));
            sb.Append(String.Format("\nNOTABLE,{0},\"{1}\"", NOTABLE, MsgForCode(NOTABLE)));
            sb.Append(String.Format("\nINVENVTYPE,{0},\"{1}\"", INVENVTYPE, MsgForCode(INVENVTYPE)));
            sb.Append(String.Format("\nBADENVTYPE,{0},\"{1}\"", BADENVTYPE, MsgForCode(BADENVTYPE)));
            sb.Append(String.Format("\nBADENVNAME,{0},\"{1}\"", BADENVNAME, MsgForCode(BADENVNAME)));
            sb.Append(String.Format("\nBADSELSITE,{0},\"{1}\"", BADSELSITE, MsgForCode(BADSELSITE)));
            sb.Append(String.Format("\nINVSELSITE,{0},\"{1}\"", INVSELSITE, MsgForCode(INVSELSITE)));
            sb.Append(String.Format("\nBADCOUNTRY,{0},\"{1}\"", BADCOUNTRY, MsgForCode(BADCOUNTRY)));
            sb.Append(String.Format("\nBADTSSPHERECALC,{0},\"{1}\"", BADTSSPHERECALC, MsgForCode(BADTSSPHERECALC)));
            sb.Append(String.Format("\nBADTSORB,{0},\"{1}\"", BADTSORB, MsgForCode(BADTSORB)));
            sb.Append(String.Format("\nBADCHANCODE,{0},\"{1}\"", BADCHANCODE, MsgForCode(BADCHANCODE)));
            sb.Append(String.Format("\nBADCALLSIGN,{0},\"{1}\"", BADCALLSIGN, MsgForCode(BADCALLSIGN)));
            sb.Append(String.Format("\nBADOPCODE,{0},\"{1}\"", BADOPCODE, MsgForCode(BADOPCODE)));
            sb.Append(String.Format("\nNOEQPTFOUND,{0},\"{1}\"", NOEQPTFOUND, MsgForCode(NOEQPTFOUND)));
            sb.Append(String.Format("\nNOTRAFFOUND,{0},\"{1}\"", NOTRAFFOUND, MsgForCode(NOTRAFFOUND)));
            sb.Append(String.Format("\nNOANTEFOUND,{0},\"{1}\"", NOANTEFOUND, MsgForCode(NOANTEFOUND)));
            sb.Append(String.Format("\nNOANTDFOUND,{0},\"{1}\"", NOANTDFOUND, MsgForCode(NOANTDFOUND)));
            sb.Append(String.Format("\nNOCTXFOUND,{0},\"{1}\"", NOCTXFOUND, MsgForCode(NOCTXFOUND)));
            sb.Append(String.Format("\nNOCTXDPTS,{0},\"{1}\"", NOCTXDPTS, MsgForCode(NOCTXDPTS)));
            sb.Append(String.Format("\nFETCH_FAIL,{0},\"{1}\"", FETCH_FAIL, MsgForCode(FETCH_FAIL)));
            sb.Append(String.Format("\nBADSOURCE,{0},\"{1}\"", BADSOURCE, MsgForCode(BADSOURCE)));
            sb.Append(String.Format("\nINPUTFILEERR,{0},\"{1}\"", INPUTFILEERR, MsgForCode(INPUTFILEERR)));
            sb.Append(String.Format("\nOPENOUTFILEERR,{0},\"{1}\"", OPENOUTFILEERR, MsgForCode(OPENOUTFILEERR)));
            sb.Append(String.Format("\nFILEWDNE,{0},\"{1}\"", FILEWDNE, MsgForCode(FILEWDNE)));
            sb.Append(String.Format("\nZONESUM,{0},\"{1}\"", ZONESUM, MsgForCode(ZONESUM)));
            sb.Append(String.Format("\nMUST_ENTER,{0},\"{1}\"", MUST_ENTER, MsgForCode(MUST_ENTER)));
            sb.Append(String.Format("\nNOTTSIPVALID,{0},\"{1}\"", NOTTSIPVALID, MsgForCode(NOTTSIPVALID)));
            sb.Append(String.Format("\nINVALIDBANDCODE,{0},\"{1}\"", INVALIDBANDCODE, MsgForCode(INVALIDBANDCODE)));
            sb.Append(String.Format("\nNOPRONAME,{0},\"{1}\"", NOPRONAME, MsgForCode(NOPRONAME)));
            sb.Append(String.Format("\nBADANALOPT,{0},\"{1}\"", BADANALOPT, MsgForCode(BADANALOPT)));
            sb.Append(String.Format("\nTEMPDNE,{0},\"{1}\"", TEMPDNE, MsgForCode(TEMPDNE)));
            sb.Append(String.Format("\nPROGNOTDEFINED,{0},\"{1}\"", PROGNOTDEFINED, MsgForCode(PROGNOTDEFINED)));
            sb.Append(String.Format("\nTOOFEWANGLES,{0},\"{1}\"", TOOFEWANGLES, MsgForCode(TOOFEWANGLES)));
            sb.Append(String.Format("\nNOLASTANGLE,{0},\"{1}\"", NOLASTANGLE, MsgForCode(NOLASTANGLE)));
            sb.Append(String.Format("\nNOFIRSTANGLE,{0},\"{1}\"", NOFIRSTANGLE, MsgForCode(NOFIRSTANGLE)));
            sb.Append(String.Format("\nBADCOORDISTTS,{0},\"{1}\"", BADCOORDISTTS, MsgForCode(BADCOORDISTTS)));
            sb.Append(String.Format("\nBADCOORDISTES,{0},\"{1}\"", BADCOORDISTES, MsgForCode(BADCOORDISTES)));
            sb.Append(String.Format("\nIS_INVALID,{0},\"{1}\"", IS_INVALID, MsgForCode(IS_INVALID)));
            sb.Append(String.Format("\nBAD_CULL,{0},\"{1}\"", BAD_CULL, MsgForCode(BAD_CULL)));
            sb.Append(String.Format("\nUSAGE,{0},\"{1}\"", USAGE, MsgForCode(USAGE)));
            sb.Append(String.Format("\nNODATABASE,{0},\"{1}\"", NODATABASE, MsgForCode(NODATABASE)));
            sb.Append(String.Format("\nBADTABLENAME,{0},\"{1}\"", BADTABLENAME, MsgForCode(BADTABLENAME)));
            sb.Append(String.Format("\nNOCREATETABLE,{0},\"{1}\"", NOCREATETABLE, MsgForCode(NOCREATETABLE)));
            sb.Append(String.Format("\nUSAGEEDIT,{0},\"{1}\"", USAGEEDIT, MsgForCode(USAGEEDIT)));
            sb.Append(String.Format("\nNOSELECTOPER,{0},\"{1}\"", NOSELECTOPER, MsgForCode(NOSELECTOPER)));
            sb.Append(String.Format("\nNOGENOPCODELIST,{0},\"{1}\"", NOGENOPCODELIST, MsgForCode(NOGENOPCODELIST)));
            sb.Append(String.Format("\nNOPCNGEN,{0},\"{1}\"", NOPCNGEN, MsgForCode(NOPCNGEN)));
            sb.Append(String.Format("\nNOSENDMAIL,{0},\"{1}\"", NOSENDMAIL, MsgForCode(NOSENDMAIL)));
            sb.Append(String.Format("\nNOGENMAILMESS,{0},\"{1}\"", NOGENMAILMESS, MsgForCode(NOGENMAILMESS)));
            sb.Append(String.Format("\nTABLEDNE,{0},\"{1}\"", TABLEDNE, MsgForCode(TABLEDNE)));
            sb.Append(String.Format("\nNOTSLAVALID,{0},\"{1}\"", NOTSLAVALID, MsgForCode(NOTSLAVALID)));
            sb.Append(String.Format("\nNOSLAGEN,{0},\"{1}\"", NOSLAGEN, MsgForCode(NOSLAGEN)));
            sb.Append(String.Format("\nUSERLISTFULL,{0},\"{1}\"", USERLISTFULL, MsgForCode(USERLISTFULL)));
            sb.Append(String.Format("\nNOUSERID,{0},\"{1}\"", NOUSERID, MsgForCode(NOUSERID)));
            sb.Append(String.Format("\nNOMEMOPCODE,{0},\"{1}\"", NOMEMOPCODE, MsgForCode(NOMEMOPCODE)));
            sb.Append(String.Format("\nOPCODELISTFULL,{0},\"{1}\"", OPCODELISTFULL, MsgForCode(OPCODELISTFULL)));
            sb.Append(String.Format("\nNOTPCNVALID,{0},\"{1}\"", NOTPCNVALID, MsgForCode(NOTPCNVALID)));
            sb.Append(String.Format("\nMUSTENTERDEST,{0},\"{1}\"", MUSTENTERDEST, MsgForCode(MUSTENTERDEST)));
            sb.Append(String.Format("\nMUSTENTERLOC,{0},\"{1}\"", MUSTENTERLOC, MsgForCode(MUSTENTERLOC)));
            sb.Append(String.Format("\nMUSTENTERTFCI,{0},\"{1}\"", MUSTENTERTFCI, MsgForCode(MUSTENTERTFCI)));
            sb.Append(String.Format("\nMUSTENTERTFCR,{0},\"{1}\"", MUSTENTERTFCR, MsgForCode(MUSTENTERTFCR)));
            sb.Append(String.Format("\nMUSTENTERRXEQPT,{0},\"{1}\"", MUSTENTERRXEQPT, MsgForCode(MUSTENTERRXEQPT)));
            sb.Append(String.Format("\nMUSTENTERACODE,{0},\"{1}\"", MUSTENTERACODE, MsgForCode(MUSTENTERACODE)));
            sb.Append(String.Format("\nMUSTENTERCALL1,{0},\"{1}\"", MUSTENTERCALL1, MsgForCode(MUSTENTERCALL1)));
            sb.Append(String.Format("\nNOCHANGE,{0},\"{1}\"", NOCHANGE, MsgForCode(NOCHANGE)));
            sb.Append(String.Format("\nNOTOWNER,{0},\"{1}\"", NOTOWNER, MsgForCode(NOTOWNER)));
            sb.Append(String.Format("\nPR_RESOLVED,{0},\"{1}\"", PR_RESOLVED, MsgForCode(PR_RESOLVED)));
            sb.Append(String.Format("\nMUSTENTERREFIND,{0},\"{1}\"", MUSTENTERREFIND, MsgForCode(MUSTENTERREFIND)));
            sb.Append(String.Format("\nNOPCODES,{0},\"{1}\"", NOPCODES, MsgForCode(NOPCODES)));
            sb.Append(String.Format("\nINVPCODE,{0},\"{1}\"", INVPCODE, MsgForCode(INVPCODE)));
            sb.Append(String.Format("\nINVHILO,{0},\"{1}\"", INVHILO, MsgForCode(INVHILO)));
            sb.Append(String.Format("\nINVMDBO,{0},\"{1}\"", INVMDBO, MsgForCode(INVMDBO)));
            sb.Append(String.Format("\nINVBAND,{0},\"{1}\"", INVBAND, MsgForCode(INVBAND)));
            sb.Append(String.Format("\nCHIDRXTX,{0},\"{1}\"", CHIDRXTX, MsgForCode(CHIDRXTX)));
            sb.Append(String.Format("\nNOCONSM,{0},\"{1}\"", NOCONSM, MsgForCode(NOCONSM)));
            sb.Append(String.Format("\nNOSMENTRY,{0},\"{1}\"", NOSMENTRY, MsgForCode(NOSMENTRY)));
            sb.Append(String.Format("\nNOSMAVAIL,{0},\"{1}\"", NOSMAVAIL, MsgForCode(NOSMAVAIL)));
            sb.Append(String.Format("\nCONTINUE,{0},\"{1}\"", CONTINUE, MsgForCode(CONTINUE)));
            sb.Append(String.Format("\nCCKNOSITE,{0},\"{1}\"", CCKNOSITE, MsgForCode(CCKNOSITE)));
            sb.Append(String.Format("\nNOPATT,{0},\"{1}\"", NOPATT, MsgForCode(NOPATT)));
            sb.Append(String.Format("\nMAXPARMS,{0},\"{1}\"", MAXPARMS, MsgForCode(MAXPARMS)));
            sb.Append(String.Format("\nFILEWVAL,{0},\"{1}\"", FILEWVAL, MsgForCode(FILEWVAL)));
            sb.Append(String.Format("\nNOTPVIEW,{0},\"{1}\"", NOTPVIEW, MsgForCode(NOTPVIEW)));
            sb.Append(String.Format("\nINVBNDCDE,{0},\"{1}\"", INVBNDCDE, MsgForCode(INVBNDCDE)));
            sb.Append(String.Format("\nINVBNDBIT,{0},\"{1}\"", INVBNDBIT, MsgForCode(INVBNDBIT)));
            sb.Append(String.Format("\nBEAMHITS,{0},\"{1}\"", BEAMHITS, MsgForCode(BEAMHITS)));
            sb.Append(String.Format("\nNOCONVERG,{0},\"{1}\"", NOCONVERG, MsgForCode(NOCONVERG)));
            sb.Append(String.Format("\nNEGLOG,{0},\"{1}\"", NEGLOG, MsgForCode(NEGLOG)));
            sb.Append(String.Format("\nTOOMANYCODES,{0},\"{1}\"", TOOMANYCODES, MsgForCode(TOOMANYCODES)));
            sb.Append(String.Format("\nNOTINUSERTBL,{0},\"{1}\"", NOTINUSERTBL, MsgForCode(NOTINUSERTBL)));
            sb.Append(String.Format("\nNOLOGINFILE,{0},\"{1}\"", NOLOGINFILE, MsgForCode(NOLOGINFILE)));
            sb.Append(String.Format("\nNOPRINTACCTFILE,{0},\"{1}\"", NOPRINTACCTFILE, MsgForCode(NOPRINTACCTFILE)));
            sb.Append(String.Format("\nNODELACCTFILE,{0},\"{1}\"", NODELACCTFILE, MsgForCode(NODELACCTFILE)));
            sb.Append(String.Format("\nUNKNOWNUSER,{0},\"{1}\"", UNKNOWNUSER, MsgForCode(UNKNOWNUSER)));
            sb.Append(String.Format("\nINVMONTH,{0},\"{1}\"", INVMONTH, MsgForCode(INVMONTH)));
            sb.Append(String.Format("\nNOVICCHID,{0},\"{1}\"", NOVICCHID, MsgForCode(NOVICCHID)));
            sb.Append(String.Format("\nBADTIME,{0},\"{1}\"", BADTIME, MsgForCode(BADTIME)));
            sb.Append(String.Format("\nPERMISSIONDENIED,{0},\"{1}\"", PERMISSIONDENIED, MsgForCode(PERMISSIONDENIED)));
            sb.Append(String.Format("\nFETCHERROR,{0},\"{1}\"", FETCHERROR, MsgForCode(FETCHERROR)));
            sb.Append(String.Format("\nBADSCREENWRITE,{0},\"{1}\"", BADSCREENWRITE, MsgForCode(BADSCREENWRITE)));
            sb.Append(String.Format("\nBADSCREENREAD,{0},\"{1}\"", BADSCREENREAD, MsgForCode(BADSCREENREAD)));
            sb.Append(String.Format("\nMUSTBEYORN,{0},\"{1}\"", MUSTBEYORN, MsgForCode(MUSTBEYORN)));
            sb.Append(String.Format("\nPERMISSIONERROR,{0},\"{1}\"", PERMISSIONERROR, MsgForCode(PERMISSIONERROR)));
            sb.Append(String.Format("\nNOINTERP,{0},\"{1}\"", NOINTERP, MsgForCode(NOINTERP)));
            sb.Append(String.Format("\nMUSTENTERCHID,{0},\"{1}\"", MUSTENTERCHID, MsgForCode(MUSTENTERCHID)));
            sb.Append(String.Format("\nINVCULLCODE,{0},\"{1}\"", INVCULLCODE, MsgForCode(INVCULLCODE)));
            sb.Append(String.Format("\nPARAMNOTEXIST,{0},\"{1}\"", PARAMNOTEXIST, MsgForCode(PARAMNOTEXIST)));
            sb.Append(String.Format("\nNOTMDBUPDVAL,{0},\"{1}\"", NOTMDBUPDVAL, MsgForCode(NOTMDBUPDVAL)));
            sb.Append(String.Format("\nINVREPTYPE,{0},\"{1}\"", INVREPTYPE, MsgForCode(INVREPTYPE)));
            sb.Append(String.Format("\nBADULTRIXID,{0},\"{1}\"", BADULTRIXID, MsgForCode(BADULTRIXID)));
            sb.Append(String.Format("\nBADOPERCODE,{0},\"{1}\"", BADOPERCODE, MsgForCode(BADOPERCODE)));
            sb.Append(String.Format("\nBADCORRESP,{0},\"{1}\"", BADCORRESP, MsgForCode(BADCORRESP)));
            sb.Append(String.Format("\nTOOFEWCODES,{0},\"{1}\"", TOOFEWCODES, MsgForCode(TOOFEWCODES)));
            sb.Append(String.Format("\nPAIRNOTUNIQUE,{0},\"{1}\"", PAIRNOTUNIQUE, MsgForCode(PAIRNOTUNIQUE)));
            sb.Append(String.Format("\nRECEXISTS,{0},\"{1}\"", RECEXISTS, MsgForCode(RECEXISTS)));
            sb.Append(String.Format("\nNOCREATETLIST,{0},\"{1}\"", NOCREATETLIST, MsgForCode(NOCREATETLIST)));
            sb.Append(String.Format("\nFE_TX_RX_GROUP,{0},\"{1}\"", FE_TX_RX_GROUP, MsgForCode(FE_TX_RX_GROUP)));
            sb.Append(String.Format("\nFE_TX_RX_MISSING,{0},\"{1}\"", FE_TX_RX_MISSING, MsgForCode(FE_TX_RX_MISSING)));
            sb.Append(String.Format("\nBADTABLE,{0},\"{1}\"", BADTABLE, MsgForCode(BADTABLE)));
            sb.Append(String.Format("\nNOREDIRECT,{0},\"{1}\"", NOREDIRECT, MsgForCode(NOREDIRECT)));
            sb.Append(String.Format("\nALREADYPOSTED,{0},\"{1}\"", ALREADYPOSTED, MsgForCode(ALREADYPOSTED)));
            sb.Append(String.Format("\nNOTEXECUTABLE,{0},\"{1}\"", NOTEXECUTABLE, MsgForCode(NOTEXECUTABLE)));
            sb.Append(String.Format("\nREPORT_ERROR,{0},\"{1}\"", REPORT_ERROR, MsgForCode(REPORT_ERROR)));
            sb.Append(String.Format("\nMAXANTES,{0},\"{1}\"", MAXANTES, MsgForCode(MAXANTES)));
            sb.Append(String.Format("\nMAXTMP2S,{0},\"{1}\"", MAXTMP2S, MsgForCode(MAXTMP2S)));
            sb.Append(String.Format("\nSINGLEDIR_ADISC,{0},\"{1}\"", SINGLEDIR_ADISC, MsgForCode(SINGLEDIR_ADISC)));
            sb.Append(String.Format("\nWRST_ADISC,{0},\"{1}\"", WRST_ADISC, MsgForCode(WRST_ADISC)));
            sb.Append(String.Format("\nBADFILECOPY,{0},\"{1}\"", BADFILECOPY, MsgForCode(BADFILECOPY)));
            sb.Append(String.Format("\nCALCDISC,{0},\"{1}\"", CALCDISC, MsgForCode(CALCDISC)));
            sb.Append(String.Format("\nMAXPRO2S,{0},\"{1}\"", MAXPRO2S, MsgForCode(MAXPRO2S)));
            sb.Append(String.Format("\nMAXCTX,{0},\"{1}\"", MAXCTX, MsgForCode(MAXCTX)));
            sb.Append(String.Format("\nMAXBANDSREAD,{0},\"{1}\"", MAXBANDSREAD, MsgForCode(MAXBANDSREAD)));
            sb.Append(String.Format("\nFREQUENCYUSED,{0},\"{1}\"", FREQUENCYUSED, MsgForCode(FREQUENCYUSED)));
            sb.Append(String.Format("\nNOBANDCHANGE,{0},\"{1}\"", NOBANDCHANGE, MsgForCode(NOBANDCHANGE)));
            sb.Append(String.Format("\nBADPASSIVEDATA,{0},\"{1}\"", BADPASSIVEDATA, MsgForCode(BADPASSIVEDATA)));
            sb.Append(String.Format("\nNOTEMPDATA,{0},\"{1}\"", NOTEMPDATA, MsgForCode(NOTEMPDATA)));
            sb.Append(String.Format("\nNOLASTANGLE359,{0},\"{1}\"", NOLASTANGLE359, MsgForCode(NOLASTANGLE359)));
            sb.Append(String.Format("\nFIVEPAIRS,{0},\"{1}\"", FIVEPAIRS, MsgForCode(FIVEPAIRS)));
            sb.Append(String.Format("\nCANTDELEPCN,{0},\"{1}\"", CANTDELEPCN, MsgForCode(CANTDELEPCN)));
            sb.Append(String.Format("\nRECNOTSAVED,{0},\"{1}\"", RECNOTSAVED, MsgForCode(RECNOTSAVED)));
            sb.Append(String.Format("\nBADESSPHERECALC,{0},\"{1}\"", BADESSPHERECALC, MsgForCode(BADESSPHERECALC)));
            sb.Append(String.Format("\nNOPLANPDF,{0},\"{1}\"", NOPLANPDF, MsgForCode(NOPLANPDF)));
            sb.Append(String.Format("\nNOPLCHID,{0},\"{1}\"", NOPLCHID, MsgForCode(NOPLCHID)));
            sb.Append(String.Format("\nBADESANALOPT,{0},\"{1}\"", BADESANALOPT, MsgForCode(BADESANALOPT)));

            sb.Append(String.Format("\nDYN_CUR_NOT_OPEN,{0},\"{1}\"", DYN_CUR_NOT_OPEN, MsgForCode(DYN_CUR_NOT_OPEN)));
            sb.Append(String.Format("\nDYN_PAST_LAST_ROW,{0},\"{1}\"", DYN_PAST_LAST_ROW, MsgForCode(DYN_PAST_LAST_ROW)));
            sb.Append(String.Format("\nDYN_MS_SQL_SERVER_ERR,{0},\"{1}\"", DYN_MS_SQL_SERVER_ERR, MsgForCode(DYN_MS_SQL_SERVER_ERR)));
            sb.Append(String.Format("\nDYN_NO_CURSOR,{0},\"{1}\"", DYN_NO_CURSOR, MsgForCode(DYN_NO_CURSOR)));
            // New error messages added by AH.
            sb.Append(String.Format("\nINVALIDNAMEORVALUE,{0},\"{1}\"", INVALIDNAMEORVALUE, MsgForCode(INVALIDNAMEORVALUE)));
            sb.Append(String.Format("\nELEVANGLETOOLOW,{0},\"{1}\"", ELEVANGLETOOLOW, MsgForCode(ELEVANGLETOOLOW)));
            sb.Append(String.Format("\nFILEISEMPTY,{0},\"{1}\"", FILEISEMPTY, MsgForCode(FILEISEMPTY)));
            sb.Append(String.Format("\nIMPORTFILEHASNOPARSABLECONTENT,{0},\"{1}\"", IMPORTFILEHASNOPARSABLECONTENT, MsgForCode(IMPORTFILEHASNOPARSABLECONTENT)));
            sb.Append(String.Format("\nUNKNOWNSECORDTYPE,{0},\"{1}\"", UNKNOWNSECORDTYPE, MsgForCode(UNKNOWNSECORDTYPE)));
            sb.Append(String.Format("\nTOOFEWCSVFIELDS,{0},\"{1}\"", TOOFEWCSVFIELDS, MsgForCode(TOOFEWCSVFIELDS)));
            sb.Append(String.Format("\nFEIMPORTFILEUNKNOWNLINEQUALIFIER,{0},\"{1}\"", FEIMPORTFILEUNKNOWNLINEQUALIFIER, MsgForCode(FEIMPORTFILEUNKNOWNLINEQUALIFIER)));
            sb.Append(String.Format("\nFEIMPORTFILEQUALIFIERINCORRECTNUMBEROFFIELDS,{0},\"{1}\"", FEIMPORTFILEQUALIFIERINCORRECTNUMBEROFFIELDS, MsgForCode(FEIMPORTFILEQUALIFIERINCORRECTNUMBEROFFIELDS)));
            sb.Append(String.Format("\nFEIMPORTINVALIDFIRSTQUALIFIER,{0},\"{1}\"", FEIMPORTINVALIDFIRSTQUALIFIER, MsgForCode(FEIMPORTINVALIDFIRSTQUALIFIER)));
            sb.Append(String.Format("\nFEIMPORTFILEQUALIFIERSINVALIDSEQUENCE,{0},\"{1}\"", FEIMPORTFILEQUALIFIERSINVALIDSEQUENCE, MsgForCode(FEIMPORTFILEQUALIFIERSINVALIDSEQUENCE)));
            sb.Append(String.Format("\nFEIMPORTFILEINCOMPLETESITERECORD,{0},\"{1}\"", FEIMPORTFILEINCOMPLETESITERECORD, MsgForCode(FEIMPORTFILEINCOMPLETESITERECORD)));
            sb.Append(String.Format("\nFEIMPORTFILEINCOMPLETEANTENNARECORD,{0},\"{1}\"", FEIMPORTFILEINCOMPLETEANTENNARECORD, MsgForCode(FEIMPORTFILEINCOMPLETEANTENNARECORD)));
            sb.Append(String.Format("\nFEIMPORTFILEINCOMPLETECHANNELRECORD,{0},\"{1}\"", FEIMPORTFILEINCOMPLETECHANNELRECORD, MsgForCode(FEIMPORTFILEINCOMPLETECHANNELRECORD)));
            sb.Append(String.Format("\nENVVARMICSUSERNOTSET,{0},\"{1}\"", ENVVARMICSUSERNOTSET, MsgForCode(ENVVARMICSUSERNOTSET)));
            sb.Append(String.Format("\nUNEXPECTEDENDOFSTRING,{0},\"{1}\"", UNEXPECTEDENDOFSTRING, MsgForCode(UNEXPECTEDENDOFSTRING)));
            sb.Append(String.Format("\nIMPORTFILEHASNOCONTENT,{0},\"{1}\"", IMPORTFILEHASNOCONTENT, MsgForCode(IMPORTFILEHASNOCONTENT)));
            sb.Append(String.Format("\nMANDATEDFIELDHASNOVALUE,{0},\"{1}\"", MANDATEDFIELDHASNOVALUE, MsgForCode(MANDATEDFIELDHASNOVALUE)));
            sb.Append(String.Format("\nMANDATEDIFADDFIELDHASNOVALUE,{0},\"{1}\"", MANDATEDIFADDFIELDHASNOVALUE, MsgForCode(MANDATEDIFADDFIELDHASNOVALUE)));
            sb.Append(String.Format("\nSTRINGISNULL,{0},\"{1}\"", STRINGISNULL, MsgForCode(STRINGISNULL)));
            sb.Append(String.Format("\nNOPASSWORD,{0},\"{1}\"", NOPASSWORD, MsgForCode(NOPASSWORD)));
            sb.Append(String.Format("\nMICSUSERNOTSET,{0},\"{1}\"", MICSUSERNOTSET, MsgForCode(MICSUSERNOTSET)));
            sb.Append(String.Format("\nMICSBINDIRNOTSET,{0},\"{1}\"", MICSBINDIRNOTSET, MsgForCode(MICSBINDIRNOTSET)));
            sb.Append(String.Format("\nTSIPSENDEMAILFAILED,{0},\"{1}\"", TSIPSENDEMAILFAILED, MsgForCode(TSIPSENDEMAILFAILED)));
            sb.Append(String.Format("\nOBFUSCATIONSETKEYFAILED,{0},\"{1}\"", OBFUSCATIONSETKEYFAILED, MsgForCode(OBFUSCATIONSETKEYFAILED)));
            sb.Append(String.Format("\nPARMTABLEHASNORECORDS,{0},\"{1}\"", PARMTABLEHASNORECORDS, MsgForCode(PARMTABLEHASNORECORDS)));
            sb.Append(String.Format("\nPARMTABLEDOESNOTEXIST,{0},\"{1}\"", PARMTABLEDOESNOTEXIST, MsgForCode(PARMTABLEDOESNOTEXIST)));
            sb.Append(String.Format("\nHILOTABLENAMEISEMPTY,{0},\"{1}\"", HILOTABLENAMEISEMPTY, MsgForCode(HILOTABLENAMEISEMPTY)));
            sb.Append(String.Format("\nINVALIDEMAILADDRESS,{0},\"{1}\"", INVALIDEMAILADDRESS, MsgForCode(INVALIDEMAILADDRESS)));
            sb.Append(String.Format("\nEMAILSUBJECTMISSING,{0},\"{1}\"", EMAILSUBJECTMISSING, MsgForCode(EMAILSUBJECTMISSING)));
            sb.Append(String.Format("\nEMAILBODYMISSING,{0},\"{1}\"", EMAILBODYMISSING, MsgForCode(EMAILBODYMISSING)));
            sb.Append(String.Format("\nEMAILATTACHMENTFILELISTISNULL,{0},\"{1}\"", EMAILATTACHMENTFILELISTISNULL, MsgForCode(EMAILATTACHMENTFILELISTISNULL)));
            sb.Append(String.Format("\nEMAILATTACHMENTINVALIDFILEPATH,{0},\"{1}\"", EMAILATTACHMENTINVALIDFILEPATH, MsgForCode(EMAILATTACHMENTINVALIDFILEPATH)));
            sb.Append(String.Format("\nEMAILSENDATTEMPTFAILED,{0},\"{1}\"", EMAILSENDATTEMPTFAILED, MsgForCode(EMAILSENDATTEMPTFAILED)));
            sb.Append(String.Format("\nATTEMPTTOGETSCHEMAFAILED,{0},\"{1}\"", ATTEMPTTOGETSCHEMAFAILED, MsgForCode(ATTEMPTTOGETSCHEMAFAILED)));
            sb.Append(String.Format("\nUSERTABLENOTFOUND,{0},\"{1}\"", USERTABLENOTFOUND, MsgForCode(USERTABLENOTFOUND)));
            sb.Append(String.Format("\nFTTITLEFETCHFAILED,{0},\"{1}\"", FTTITLEFETCHFAILED, MsgForCode(FTTITLEFETCHFAILED)));
            sb.Append(String.Format("\nMDBALREADYUPDATED,{0},\"{1}\"", MDBALREADYUPDATED, MsgForCode(MDBALREADYUPDATED)));
            sb.Append(String.Format("\nCANNOTSETODBCENVIRONMENTATTRIBUTES,{0},\"{1}\"", CANNOTSETODBCENVIRONMENTATTRIBUTES, MsgForCode(CANNOTSETODBCENVIRONMENTATTRIBUTES)));

            return sb.ToString();
        }

    }

}

```
