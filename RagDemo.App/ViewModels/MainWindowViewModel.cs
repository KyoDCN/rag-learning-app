using RagDemo.App.Services;

namespace RagDemo.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public DocumentViewModel Documents { get; }
    public QueryViewModel Query { get; }

    public MainWindowViewModel()
    {
        var client = new RagApiClient("http://localhost:5045/");
        Documents = new DocumentViewModel(client);
        Query = new QueryViewModel(client);
    }
}
