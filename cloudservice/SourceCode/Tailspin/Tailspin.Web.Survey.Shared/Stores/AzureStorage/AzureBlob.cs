namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using Microsoft.WindowsAzure.StorageClient;

    internal class AzureBlob : CloudBlob, IListBlobItemWithName
    {
        internal AzureBlob(CloudBlob source) : base(source) { }
    }
}
