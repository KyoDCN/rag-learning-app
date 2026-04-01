using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RagDemo.App.Services;
using RagDemo.Contracts;

namespace RagDemo.App.ViewModels;

public partial class QueryViewModel : ViewModelBase
{
    private readonly RagApiClient m_client;

    [ObservableProperty] private string m_question = string.Empty;
    [ObservableProperty] private string m_answer = string.Empty;
    [ObservableProperty] private string m_statusMessage = string.Empty;
    [ObservableProperty] private bool m_isBusy;

    public ObservableCollection<string> SourceChunks { get; } = [];

    public QueryViewModel(RagApiClient client)
    {
        m_client = client;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task QueryAsync(CancellationToken ct)
    {
        IsBusy = true;
        Answer = string.Empty;
        SourceChunks.Clear();
        StatusMessage = string.Empty;

        try
        {
            var response = await m_client.QueryAsync(new QueryRequest(Question), ct);

            Answer = response.Answer;

            foreach (var chunk in response.SourceChunks)
                SourceChunks.Add(chunk);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task QueryStreamAsync(CancellationToken ct)
    {
        IsBusy = true;
        Answer = string.Empty;
        SourceChunks.Clear();
        StatusMessage = string.Empty;

        try
        {
            await foreach (var update in m_client.QueryStreamAsync(new QueryRequest(Question), ct))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Answer += update.Delta;

                    if (update.SourceChunks is not null)
                        foreach (var chunk in update.SourceChunks)
                            SourceChunks.Add(chunk);
                });
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
