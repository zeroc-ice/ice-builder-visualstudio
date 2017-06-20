// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.Packaging;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IceBuilder
{
    public class SignTask : Task
    {
        [Required]
        public String PackageFile
        {
            get;
            set;
        }

        [Required]
        public String Certificate
        {
            get;
            set;
        }

        [Required]
        public String CertificatePassword
        {
            get;
            set;
        }

        public override bool Execute()
        {
            using (Package package = Package.Open(PackageFile, FileMode.Open))
            {
                try
                {
                    PackageDigitalSignatureManager signatureManager = new PackageDigitalSignatureManager(package);
                    signatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;

                    List<Uri> toSign = package.GetParts().Select(part => part.Uri).ToList();

                    toSign.Add(PackUriHelper.GetRelationshipPartUri(signatureManager.SignatureOrigin));
                    toSign.Add(signatureManager.SignatureOrigin);
                    toSign.Add(PackUriHelper.GetRelationshipPartUri(new Uri("/", UriKind.RelativeOrAbsolute)));

                    signatureManager.Sign(toSign, new X509Certificate2(Certificate, CertificatePassword));
                    return true;
                }
                catch (Exception ex)
                {
                    Log.LogError("Error signing package: ", ex);
                    return false;
                }
            }
        }
    }
}
