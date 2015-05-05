using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.VCProjectEngine;

namespace IceBuilder
{
    public class VCUtilI : VCUtil
    {
        public bool SetupSliceFilter(EnvDTE.Project dteProject)
        {
            VCProject project = dteProject.Object as VCProject;
            foreach (VCFilter f in project.Filters)
            {
                if (f.Name.Equals("Slice Files"))
                {
                    if (String.IsNullOrEmpty(f.Filter) || !f.Filter.Equals("ice"))
                    {
                        f.Filter = "ice";
                        return true;
                    }
                    return false;
                }
            }

            VCFilter filter = project.AddFilter("Slice Files");
            filter.Filter = "ice";
            return true;
        }
    }
}
