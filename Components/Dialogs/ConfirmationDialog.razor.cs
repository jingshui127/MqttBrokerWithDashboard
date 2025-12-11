using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MqttBrokerBlazor.Components.Dialogs
{
    public partial class ConfirmationDialog : ComponentBase
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public string ContentText { get; set; } = string.Empty;

        [Parameter] public string ButtonText { get; set; } = string.Empty;

        [Parameter] public Color Color { get; set; }

        void Submit() => MudDialog.Close(DialogResult.Ok(true));

        void Cancel() => MudDialog.Cancel();
    }
}