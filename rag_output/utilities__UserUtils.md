# Documented File: UserUtils.cs
**Repository Path:** `utilities\UserUtils.cs`
**Primary Layer:** `utilities`
**Namespace:** `UserUtilities`

## Source Code Representation
```csharp
﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;


namespace UserUtilities
{
    /// <summary>
    /// Summary description for UserUtils
    /// </summary>
    public class UserUtils
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_4
        {
            public string name;
            public string password;
            public int password_age;
            public int priv;
            public string home_dir;
            public string comment;
            public int flags;
            public string script_path;
            public int auth_flags;
            public string full_name;
            public string usr_comment;
            public string parms;
            public string workstations;
            public int last_logon;
            public int last_logoff;
            public int acct_expires;
            public int max_storage;
            public int units_per_week;
            public IntPtr logon_hours;    // This is a PBYTE
            public int bad_pw_count;
            public int num_logons;
            public string logon_server;
            public int country_code;
            public int code_page;
            public IntPtr user_sid;     // This is a PSID
            public int primary_group_id;
            public string profile;
            public string home_dir_drive;
            public int password_expired;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct GROUP_INFO_2
        {
            public int Name;
            public int Comment;
            public int GroupID;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOCALGROUP_USERS_INFO_0
        {
            public string groupname;
        }

        [DllImport("Netapi32.dll", SetLastError = true)]
        public extern static int NetUserGetLocalGroups
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string username,
             int level,
             int flags,
             out IntPtr bufptr,
             int prefmaxlen,
             out int entriesread,
             out int totalentries);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("kernel32.dll")]
        public extern static bool CloseHandle(IntPtr ExistingTokenHandle);

        [DllImport("Netapi32.dll")]
        extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, out IntPtr bufptr);

        [DllImport("Netapi32.dll")]
        extern static int NetUserSetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, ref USER_INFO_4 buf, int error);

        [DllImport("Netapi32.dll")]
        extern static int NetApiBufferFree(IntPtr bufptr);

        //[STAThread]
        public static string checkuser2()
        {
            IntPtr hToken;
            USER_INFO_4 ui4;
            int days_since_change = 0;
            int days_to_expiry = 0;

            HttpContext ctx = HttpContext.Current;

            string logfile = ctx.Application["web_drive"].ToString() + "\\perflogs\\pwdtimeout.txt";	// write password timeout info

            StreamWriter sw = new StreamWriter(logfile, true);
            sw.WriteLine("in checkuser2");
            sw.Flush();

            hToken = WindowsIdentity.GetCurrent().Token;

            // get Windows identity for the current user.

            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            string username = wi.Name.ToString();

            char[] delimiter = "\\".ToCharArray();
            string[] keyparts = username.Split(delimiter);

            username = keyparts[1];

            string retstr = "";

            // get user info
            try
            {
                IntPtr pBuf;
                NetUserGetInfo(null, username, 4, out pBuf);
                ui4 = (USER_INFO_4)Marshal.PtrToStructure(
                pBuf, typeof(USER_INFO_4));
                NetApiBufferFree(pBuf);
            }
            catch (Exception)
            {
                retstr = "ERROR:Cannot get USER_INFO_4.";
                return retstr;
            }

            // check for expired password
            if (ui4.password_expired != 0)
            {
                if (ui4.acct_expires >= 0)  // a -1 in this fields indicates account never expires
                {
                    return "ERROR:Your password has expired - please contact FCSA";
                }
            }

            // check for password near timeout
            // This code assumes expiry is default 90 days
            // and issues warning during last week
            int secs_per_day = 60 * 60 * 24;
            //if (ui4.acct_expires >= 0)  // a -1 in this fields indicates account never expires
            //{
            //if (ui4.password_age > secs_per_day * 35)
            //{
            sw.WriteLine("Password age:" + ui4.password_age.ToString());
            sw.WriteLine("Secs per day:" + secs_per_day.ToString());

            days_since_change = ui4.password_age / secs_per_day;
            days_to_expiry = 90 - days_since_change;

            sw.WriteLine("days_since_change:" + days_since_change.ToString());
            sw.WriteLine("days_to_expiry:" + days_to_expiry.ToString());

            ctx.Session["days_to_password_expiry"] = days_to_expiry;

            //}
            //}

            sw.Close();

            // get user's group info
            string sql_group = getSQLgroup();

            return sql_group;
        }
        public static string getSQLschema()
        {
            // Each valid mics user will belong to one (and only one) group of the form
            // SQLC<company> or SQLR<company>. The former is for contractors, the latter
            // for regular company users.
            // For SQL usage, the <company> value equates to the user's SQL schema

            // This routine returns 3 possible strings

            // "ERROR: Username or computer not found for user" + <username>
            // This occurs if the attempt to look up the user's local groups fails

            // ERROR: No SQL group found for user: " + <username>;
            // This occurs if user's local groups were found, but no group name starts with 'SQL'

            // <schema>
            // If lookup is successful, this string contains the value to be used as the schema

            bool valid_group = false;
            IntPtr hToken;

            hToken = WindowsIdentity.GetCurrent().Token;

            // get Windows identity for the current user.

            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            string username = wi.Name.ToString();

            char[] delimiter = "\\".ToCharArray();
            string[] userparts = username.Split(delimiter);

            username = userparts[1];

            string retstr = "";
            int EntriesRead;
            int TotalEntries;
            IntPtr bufPtr;

            int ErrorCode = NetUserGetLocalGroups(null, username, 0, 0, out bufPtr, 1024, out EntriesRead, out TotalEntries);
            if (ErrorCode != 0)
            {
                return "ERROR: Username or computer not found for user" + username;
            }

            if (EntriesRead > 0)
            {
                LOCALGROUP_USERS_INFO_0[] RetGroups = new LOCALGROUP_USERS_INFO_0[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    RetGroups[i] = (LOCALGROUP_USERS_INFO_0)Marshal.PtrToStructure(iter, typeof(LOCALGROUP_USERS_INFO_0));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(LOCALGROUP_USERS_INFO_0)));
                    if (RetGroups[i].groupname.IndexOf("SQL") == 0)
                    {
                        retstr = RetGroups[i].groupname;
                        valid_group = true;
                    }
                }
                NetApiBufferFree(bufPtr);
            }

            if (valid_group)
            {
                return retstr.Substring(4);
            }
            else
            {
                return "ERROR: No SQL group found for user: " + username;
            }
        }
        public static string getSQLgroup()
        {
            // Each valid mics user will belong to one (and only one) group of the form
            // SQLC<company> or SQLR<company>. The former is for contractors, the latter
            // for regular company users.
            // For SQL usage, the <company> value equates to the user's SQL schema

            // This routine returns 3 possible strings

            // "ERROR: Username or computer not found for user" + <username>
            // This occurs if the attempt to look up the user's local groups fails

            // ERROR: No SQL group found for user: " + <username>;
            // This occurs if user's local groups were found, but no group name starts with 'SQL'

            // <SQLgroup>
            // If lookup is successful, this string contains the value of the user's SQL group

            bool valid_group = false;
            IntPtr hToken;

            hToken = WindowsIdentity.GetCurrent().Token;

            // get Windows identity for the current user.

            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            string username = wi.Name.ToString();

            char[] delimiter = "\\".ToCharArray();
            string[] userparts = username.Split(delimiter);

            username = userparts[1];

            string retstr = "";
            int EntriesRead;
            int TotalEntries;
            IntPtr bufPtr;

            int ErrorCode = NetUserGetLocalGroups(null, username, 0, 0, out bufPtr, 1024, out EntriesRead, out TotalEntries);
            if (ErrorCode != 0)
            {
                return "ERROR: Username or computer not found for user" + username;
            }

            if (EntriesRead > 0)
            {
                LOCALGROUP_USERS_INFO_0[] RetGroups = new LOCALGROUP_USERS_INFO_0[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    RetGroups[i] = (LOCALGROUP_USERS_INFO_0)Marshal.PtrToStructure(iter, typeof(LOCALGROUP_USERS_INFO_0));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(LOCALGROUP_USERS_INFO_0)));
                    if (RetGroups[i].groupname.IndexOf("SQL") == 0)
                    {
                        retstr = RetGroups[i].groupname;
                        valid_group = true;
                    }
                }
                NetApiBufferFree(bufPtr);
            }

            if (valid_group)
            {
                return "OK" + retstr;
            }
            else
            {
                return "ERROR: No SQL group found for user: " + username;
            }
        }
    }
}

```
