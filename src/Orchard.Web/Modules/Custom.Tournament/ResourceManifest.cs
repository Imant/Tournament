using Orchard.UI.Resources;

namespace Custom.Tournament
{
    // ResourceManifest classes implement IResourceManifestProvider and possess the BuildManifests method.
    public class ResourceManifest : IResourceManifestProvider
    {
        // This is the only method to implement. Using it we're going to register some static resources
        // to be able to use them in our templates.
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            // We add a new instance of ResourceManifest to the ResourceManifestBuilder,
            // instantiated by the Add method.
            var manifest = builder.Add();

            manifest
                // We're registering a stylesheet with DefineStyle and defining it's name. It's a global name, so choose
                // wisely. There is no strict naming convention, but we can give you and advice how to choose a unique name:
                // it should contain the module's full namespace followed by a meaningful name.
                .DefineStyle("PagedList")
                // This is the actual stylesheet that will be assigned to the resource name. By default, Orchard
                // will look for this file in the Styles folder, but you can override it using SetBasePath:
                // manifest.BasePath points to the root of the project, so setting
                // BasePath(manifest.BasePath + "Bedsheets") will cause that Orchard will look for the given
                // resource in the Bedsheets folder in the project root.
                // Please note that the naming of the file itself is following similiar rules as the resource name,
                // with some modifications applied as it's a file name. Since stylesheets and script are shapes, it's 
                // possible to override them, so you should try to avoid name collisions: file names (not the full path,
                // just the name!) should be globally unique.
                .SetUrl("paged-list.css");
        }
    }
}