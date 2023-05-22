namespace GitHubSync
{
    public interface IInputParameterProcessing
    {
        string[] InputArgs { get; set; }

        void Process();
    }
}