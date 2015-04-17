// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class ProjectConfigurationSettigns
    {
        public readonly uint storageType = (uint)_PersistStorageType.PST_PROJECT_FILE;

        public readonly String outputDirPropertyName = "OutputDir";
        public readonly String icePropertyName = "Ice";
        public readonly String checksumPropertyName = "Checksum";
        public readonly String streamingPropertyName = "Stream";
        public readonly String tiePropertyName = "Tie";
        public readonly String underscoresPropertyName = "Underscore";
        public readonly String additionalIncludeDirectoriesPropertyName = "AdditionalIncludeDirectories";
        public readonly String additionalOptionsPropertyName = "AdditionalOptions";

        private bool outputDirNeedSave = false;
        private bool iceNeedSave = false;
        private bool checksumNeedSave = false;
        private bool streamingNeedSave = false;
        private bool tieNeedSave = false;
        private bool underscoresNeedSave = false;
        private bool additionalIncludeDirectoriesNeedSave = false;
        private bool additionalOptionsNeedSave = false;

        public Boolean NeedSave
        {
            get
            {
                return outputDirNeedSave ||
                    iceNeedSave ||
                    checksumNeedSave ||
                    streamingNeedSave ||
                    tieNeedSave ||
                    underscoresNeedSave ||
                    additionalIncludeDirectoriesNeedSave ||
                    additionalOptionsNeedSave;
            }
        }

        public ProjectConfigurationSettigns(IVsBuildPropertyStorage storage, String configuration)
        {
            _storage = storage;
            _configuration = configuration;
        }

        public void Load()
        {
            String value;
            _storage.GetPropertyValue(outputDirPropertyName, _configuration, storageType, out value);
            _outputDir = value == null ? "" : value;
            outputDirNeedSave = false;

            _storage.GetPropertyValue(icePropertyName, _configuration, storageType, out value);
            Ice = value == null ? false : value.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
            iceNeedSave = false;

            _storage.GetPropertyValue(checksumPropertyName, _configuration, storageType, out value);
            Checksum = value == null ? false : value.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
            checksumNeedSave = false;

            _storage.GetPropertyValue(streamingPropertyName, _configuration, storageType, out value);
            Streaming = value == null ? false : value.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
            streamingNeedSave = false;

            _storage.GetPropertyValue(tiePropertyName, _configuration, storageType, out value);
            Tie = value == null ? false : value.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
            tieNeedSave = false;

            _storage.GetPropertyValue(underscoresPropertyName, _configuration, storageType, out value);
            Underscores = value == null ? false : value.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
            underscoresNeedSave = false;

            _storage.GetPropertyValue(additionalIncludeDirectoriesPropertyName, _configuration, storageType, out value);
            AdditionalIncludeDirectories = value == null ? "" : value;
            additionalIncludeDirectoriesNeedSave = false;

            _storage.GetPropertyValue(additionalOptionsPropertyName, _configuration, storageType, out value);
            AdditionalOptions = value == null ? "" : value;
            additionalOptionsNeedSave = false;
        }

        public void Save()
        {
            if (outputDirNeedSave)
            {
                _storage.SetPropertyValue(outputDirPropertyName, _configuration, storageType, OutputDir);
                
                outputDirNeedSave = false;
            }

            if (iceNeedSave)
            {
                _storage.SetPropertyValue(icePropertyName, _configuration, storageType, Ice ? "yes" : "no");
                iceNeedSave = false;
            }

            if (checksumNeedSave)
            {
                _storage.SetPropertyValue(checksumPropertyName, _configuration, storageType, Checksum ? "yes" : "no");
                checksumNeedSave = false;
            }

            if (streamingNeedSave)
            {
                _storage.SetPropertyValue(streamingPropertyName, _configuration, storageType, Streaming ? "yes" : "no");
                streamingNeedSave = false;
            }

            if (tieNeedSave)
            {
                _storage.SetPropertyValue(tiePropertyName, _configuration, storageType, Tie ? "yes" : "no");
                tieNeedSave = false;
            }

            if (underscoresNeedSave)
            {
                _storage.SetPropertyValue(underscoresPropertyName, _configuration, storageType, Underscores ? "yes" : "no");
                underscoresNeedSave = false;
            }

            if (additionalIncludeDirectoriesNeedSave)
            {
                _storage.SetPropertyValue(additionalIncludeDirectoriesPropertyName, _configuration, storageType,
                        AdditionalIncludeDirectories);
                additionalIncludeDirectoriesNeedSave = false;
            }

            if (additionalOptionsNeedSave)
            {
                _storage.SetPropertyValue(additionalOptionsPropertyName, _configuration, storageType, AdditionalOptions);
                additionalOptionsNeedSave = false;
            }
        }

        private String _outputDir = "";
        public String OutputDir
        {
            get
            {
                return _outputDir;
            }
            set
            {
                if (!_outputDir.Equals(value))
                {
                    _outputDir = value;
                    outputDirNeedSave = true;
                }
            }
        }

        private Boolean _ice;
        public Boolean Ice
        {
            get
            {
                return _ice;
            }
            set
            {
                if (_ice != value)
                {
                    _ice = value;
                    iceNeedSave = true;
                }
            }
        }

        private Boolean _checksum;
        public Boolean Checksum
        {
            get
            {
                return _checksum;
            }
            set
            {
                if (_checksum != value)
                {
                    _checksum = value;
                    checksumNeedSave = true;
                }
            }
        }

        private Boolean _streaming;
        public Boolean Streaming
        {
            get
            {
                return _streaming;
            }
            set
            {
                if (_streaming != value)
                {
                    _streaming = value;
                    streamingNeedSave = true;
                }
            }
        }

        private Boolean _tie = false;
        public Boolean Tie
        {
            get
            {
                return _tie;
            }
            set
            {
                if (_tie != value)
                {
                    _tie = value;
                    tieNeedSave = true;
                }
            }
        }

        private Boolean _underscores = false;
        public Boolean Underscores
        {
            get
            {
                return _underscores;
            }
            set
            {
                if (_underscores != value)
                {
                    _underscores = value;
                    underscoresNeedSave = true;
                }
            }
        }

        private String _additionalIncludeDirectories = "";
        public String AdditionalIncludeDirectories
        {
            get
            {
                return _additionalIncludeDirectories;
            }
            set
            {
                if(!_additionalIncludeDirectories.Equals(value))
                {
                    _additionalIncludeDirectories = value;
                    additionalIncludeDirectoriesNeedSave = true;
                }
            }
        }

        private String _additionalOptions = "";
        public String AdditionalOptions
        {
            get
            {
                return _additionalOptions;
            }
            set
            {
                if (!_additionalOptions.Equals(value))
                {
                    _additionalOptions = value;
                    additionalOptionsNeedSave = true;
                }
            }
        }

        IVsBuildPropertyStorage _storage;
        String _configuration;
    }

    public class ProjectConfiguration : IVsProjectFlavorCfg
    {
        private static Dictionary<IVsCfg, ProjectConfiguration> _configurations =
            new Dictionary<IVsCfg, ProjectConfiguration>();

        public static ProjectConfiguration getProjectConfiguration(IVsCfg cfg)
        {
            return _configurations[cfg];
        }

        private ProjectConfigurationSettigns _settings;
        public ProjectConfigurationSettigns Settings
        {
            get
            {
                return _settings;
            }
        }

        public void Initialize(Project project, IVsCfg baseConfiguration, IVsProjectFlavorCfg innerConfiguration)
        {
            _project = project;
            _baseConfiguration = baseConfiguration;
            _innerConfiguration = innerConfiguration;
            _configurations.Add(baseConfiguration, this);

            _baseConfiguration.get_DisplayName(out _configuration);
            _storage = _project.InnerHierarchy as IVsBuildPropertyStorage;
            _settings = new ProjectConfigurationSettigns(_storage, _configuration);
            _settings.Load();
        }

        public String GetProperty(String name)
        {
            String value;
            _storage.GetPropertyValue(name, _configuration, (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
            return value;
        }

        /// <summary>
        /// Provides access to a configuration interfaces such as IVsBuildableProjectCfg2
        /// or IVsDebuggableProjectCfg.
        /// </summary>
        /// <param name="iidCfg">IID of the interface that is being asked</param>
        /// <param name="ppCfg">Object that implement the interface</param>
        /// <returns>HRESULT</returns>
        public int get_CfgType(ref Guid iidCfg, out IntPtr ppCfg)
        {
            ppCfg = IntPtr.Zero;
            if(_innerConfiguration != null)
            {
                return _innerConfiguration.get_CfgType(ref iidCfg, out ppCfg);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Closes the IVsProjectFlavorCfg object.
        /// </summary>
        /// <returns></returns>
        public int Close()
        {
            _configurations.Remove(this._baseConfiguration);
            int hr = _innerConfiguration.Close();
            if(_project != null)
            {
                _project = null;
            }

            if(_baseConfiguration != null)
            {
                if(Marshal.IsComObject(_baseConfiguration))
                {
                    Marshal.ReleaseComObject(_baseConfiguration);
                }
                _baseConfiguration = null;
            }

            if(_innerConfiguration != null)
            {
                if(Marshal.IsComObject(_innerConfiguration))
                {
                    Marshal.ReleaseComObject(_innerConfiguration);
                }
                _innerConfiguration = null;
            }
            return hr;
        }

        private Project _project;
        private IVsCfg _baseConfiguration;
        private IVsProjectFlavorCfg _innerConfiguration;

        private IVsBuildPropertyStorage _storage;
        private String _configuration;
    }
}
