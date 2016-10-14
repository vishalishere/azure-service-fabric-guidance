namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    public interface IUpdateableAzureQueue
    {
        void DeleteMessage(AzureQueueMessage message);
        void UpdateMessage(AzureQueueMessage message);
    }
}
