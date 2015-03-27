// **********************************************************************
//
// Copyright (c) 2003-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace ZeroC.IceVisualStudio
{
    public interface IceConfigurationDialog
    {
        void unsetCancelButton();
        void setCancelButton();        
        void needSave();
        void endEditIncludeDir(bool save);
        bool editingIncludeDir();
    }
}
