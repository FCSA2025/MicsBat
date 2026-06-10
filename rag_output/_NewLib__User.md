# Documented File: User.cs
**Repository Path:** `_NewLib\User.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class provides a set of static methods that return information about the
    /// current Windows user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// This method returns true if the current Windows user is a member of the Administrator group.
        /// </summary>
        /// <returns></returns>
        public static bool IsInAdministratorGroup()
        {
            bool isInAdministratorGroup = false;

            using (var pc = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
            {
                using (var up = UserPrincipal.FindByIdentity(pc, WindowsIdentity.GetCurrent().Name))
                {
                    isInAdministratorGroup = up.GetAuthorizationGroups().Any(group => group.Name == "Administrators");
                }
            }

            return isInAdministratorGroup;
        }




    }
}

```
