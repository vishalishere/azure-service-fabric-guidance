namespace Tailspin.SimulatedIssuer.ViewModels
{
    public class TailspinSignInViewModel
    {
        public string Domain { get; set; }

        public string UserName { get; set; }

        public string SignInRequest { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0}\\{1}", this.Domain, this.UserName);
            }
        }
    }
}
