// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AspNetCore21MVC.Models
{
    public class DefaultViewModel
    {
        public DefaultViewModel(IList<CookieDetails> cookieDetails)
        {
            CookieDetails = new ReadOnlyCollection<CookieDetails>(cookieDetails);
        }

        public IReadOnlyCollection<CookieDetails> CookieDetails {get; private set;}
    }

    public class CookieDetails
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
