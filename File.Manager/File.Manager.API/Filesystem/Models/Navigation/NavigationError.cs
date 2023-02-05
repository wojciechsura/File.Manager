namespace File.Manager.API.Filesystem.Models.Navigation
{
    public sealed class NavigationError : NavigationOutcome
    {
        public NavigationError(string message)
        {
            this.Message = message;
        }

        public string Message { get; }
    }
}