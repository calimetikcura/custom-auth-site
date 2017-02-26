﻿
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer.Host.Configuration
{
    // TODO:brock rebrand to kcura's name
    public class IdentityServerOptions
    {
        public bool AllowLocalLogin => HttpLoginCallback == null;
        public Func<HttpContext, Task<string>> HttpLoginCallback { get; set; }

        public string CertificateStoreSubjectDistinguishedName { get; set; }
        public string CertificateFileName { get; set; }
        public string CertificatePassword { get; set; }

        public string RelativityClientId { get; set; }
        public ICollection<string> RelativityRedirectUris { get; set; } = new HashSet<string>();
        public ICollection<string> RelativityPostLogoutRedirectUris { get; set; } = new HashSet<string>();
    }
}

