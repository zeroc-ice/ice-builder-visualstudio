using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    class Project : FlavoredProjectBase
    {
        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            if (propId == (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList)
            {
                // Get a semicolon-delimited list of clsids of the configuration-dependent
                // property pages.
                ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));

                // Add the CustomPropertyPage property page.
                property += ';' + typeof(CustomPropertyPage).GUID.ToString("B");

                return VSConstants.S_OK;
            }
            return base.GetProperty(itemId, propId, out property);
        }
    }
}
