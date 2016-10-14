namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using Microsoft.WindowsAzure.StorageClient;

    public interface IListBlobItemWithName : IListBlobItem
    {
        string Name { get; }
    }
}
