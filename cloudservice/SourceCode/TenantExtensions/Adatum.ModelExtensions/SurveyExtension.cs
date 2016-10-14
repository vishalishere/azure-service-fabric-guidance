namespace Adatum.ModelExtensions
{
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Extensibility;

    public class SurveyExtension : TableServiceEntity, IModelExtension
    {
        public string ProductName { get; set; }

        //// To extend an initial model add nullable properties
        //// ex. public int? NewValue { get; set; }

        public bool IsChildOf(object parent)
        {
            return this.RowKey.Equals(parent.ToString());
        }
    }
}
