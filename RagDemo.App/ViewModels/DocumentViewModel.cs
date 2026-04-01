using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RagDemo.App.Services;
using RagDemo.Contracts;

namespace RagDemo.App.ViewModels;

public partial class DocumentViewModel : ViewModelBase
{
    private readonly RagApiClient m_client;

    [ObservableProperty] private string m_documentText = string.Empty;
    [ObservableProperty] private string m_documentName = string.Empty;
    [ObservableProperty] private string m_statusMessage = string.Empty;
    [ObservableProperty] private int m_chunkCount;
    [ObservableProperty] private bool m_isBusy;

    public DocumentViewModel(RagApiClient client)
    {
        m_client = client;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task AddDocumentAsync(CancellationToken ct)
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            await m_client.AddDocumentsAsync([new AddDocumentRequest(DocumentText, DocumentName)], ct);
            StatusMessage = "Document added successfully.";
            await RefreshStatusAsync();
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
    private async Task LoadDocumentAsync(CancellationToken ct)
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            await m_client.LoadDocumentsAsync([new LoadDocumentRequest(DocumentText, DocumentName)], ct);
            StatusMessage = "Vector store replaced successfully.";
            await RefreshStatusAsync();
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

    [RelayCommand]
    private async Task ClearDocumentsAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            await m_client.ClearDocumentsAsync();
            ChunkCount = 0;
            StatusMessage = "Vector store cleared.";
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

    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        try
        {
            var status = await m_client.GetStatusAsync();
            ChunkCount = status.ChunkCount;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
