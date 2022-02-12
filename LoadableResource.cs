using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSoftcoded
{
    public class LoadableResource
    {
        private string _name = "";
        private bool _isAssetResource = false;
        private bool _isFileResource = false;

        public LoadableResource(string name, bool isAssetResource, bool isFileResource)
        {
            _name = name;
            _isAssetResource = isAssetResource;
            _isFileResource = isFileResource;
        }

        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public bool GetAssetResource()
        {
            return _isAssetResource;
        }

        public bool GetFileResource()
        {
            return _isFileResource;
        }

        public void SetAssetResource(bool state)
        {
            _isAssetResource = state;
        }

        public void SetFileResource(bool state)
        {
            _isFileResource = state;
        }
    }
}
