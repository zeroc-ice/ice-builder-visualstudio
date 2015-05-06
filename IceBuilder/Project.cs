// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class Project : FlavoredProjectBase
    {
        public static readonly String BuildEventsPropertyPageGUID = "{1e78f8db-6c07-4d61-a18f-7514010abd56}";

        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            object objectForIUnknown = null;
            objectForIUnknown = Marshal.GetObjectForIUnknown(innerIUnknown);
            if (base.serviceProvider == null)
            {
                base.serviceProvider = this.Package;
            }
            base.SetInnerProject(innerIUnknown);
            _cfgProvider = objectForIUnknown as IVsProjectFlavorCfgProvider;
        }

        protected override void Close()
        {
            base.Close();
            if (_cfgProvider != null)
            {
                if (Marshal.IsComObject(_cfgProvider))
                {
                    Marshal.ReleaseComObject(_cfgProvider);
                }
                _cfgProvider = null;
            }
        }

        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            if(propId == (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList)
            {
                String propertyPageGUID = typeof(PropertyPage).GUID.ToString("B");
                ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
                property = String.Format("{0};{1}", propertyPageGUID, property);
                return VSConstants.S_OK;
            }
            else if (propId == (int)__VSHPROPID2.VSHPROPID_PriorityPropertyPagesCLSIDList)
            {
                String propertyPageGUID = typeof(PropertyPage).GUID.ToString("B");
                ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
                List<String> sorted = new List<String>();
                property.ToString()
                        .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList()
                        .ForEach(token =>
                            {
                                if (!token.Equals(propertyPageGUID,
                                   StringComparison.CurrentCultureIgnoreCase))
                                {
                                    sorted.Add(token);
                                }

                                if(token.Equals(BuildEventsPropertyPageGUID, 
                                   StringComparison.CurrentCultureIgnoreCase))
                                {
                                    sorted.Add(propertyPageGUID);
                                }
                            });
                property = String.Join(";", sorted);
                return VSConstants.S_OK;
            }
            return base.GetProperty(itemId, propId, out property);
        }

        protected IVsProjectFlavorCfgProvider _cfgProvider = null;
        internal Microsoft.VisualStudio.Shell.Package Package { get; set; }
    }
}
