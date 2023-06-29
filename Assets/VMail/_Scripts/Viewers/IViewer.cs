
namespace VMail.Viewer
{
    public interface IViewer
    {
        void OpenMessage(Message message);

        void SetState(Page page);

        void SetState(Transition t);

    }
}