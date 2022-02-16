
namespace SSSoftcoded
{
    public class SSSLoadableResource
    {
        private string _name = "";
        //Extension to find the file at. Contains the '.'
        //A blank extension means the asset is internal. This is only used for wallpapers currently.
        private string _extension = "";

        public SSSLoadableResource(string name, string extension)
        {
            _name = name;
            _extension = extension;
            if (_extension != "" && !_extension.StartsWith("."))
            {
                _extension = "." + _extension;
            }
        }

        public SSSLoadableResource(string filePath)
        {
            _extension = filePath.Substring(filePath.LastIndexOf("."));
            _name = SSSLoadingHelper.IsolateFileName(filePath);
        }

        public string GetName()
        {
            return _name;
        }

        public string GetExtension()
        {
            return _extension;
        }

        public string GetAddress()
        {
            return _name + _extension;
        }
    }
}
