using Microsoft.AspNetCore.Components;
using Radzen;

namespace KNTCommon.Blazor.Services
{
    public class HelperService
    {
        private readonly NotificationService NS;

        public HelperService(NotificationService service)
        {
            NS = service;
        }

        public void NotifySuccess(string? summary = null, string? detail = null)
        {
            NS.Notify(NotificationSeverity.Success, summary, detail, 2000, null, false, null);
        }

        public void NotifyError(string? summary = null, string? detail = null)
        {
            NS.Notify(NotificationSeverity.Error, summary, detail, 2000, null, false, null);
        }

        public void Notify(bool ok, string? summary = null, string? detail = null)
        {
            if (ok)
                NotifySuccess(summary, detail);
            else 
                NotifyError(summary, detail);
        }

        public void NotifyWarning(string? summary = null, string? detail = null)
        {
            NS.Notify(NotificationSeverity.Warning, summary, detail, 2000, null, false, null);
        }

        /// <summary>
        /// /// Both draggable and resizeable options are false. Only paramater is the dialogCss.
        /// </summary>
        /// <param name="dialogCss"></param>
        /// <returns></returns>
        public DialogOptions GetDialogOptions(string? dialogCss = "DefaultDialog")
        {
            var options = new DialogOptions();
            options.Resizable = false;
            options.Draggable = false;
            options.CloseDialogOnOverlayClick = true;
            options.CssClass = dialogCss;
            return options;
        }

        /// <summary>
        /// Works in pairs; first parameter is the name and second is its value. If % 2 != 0 parameters, the method will throw an exception.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Dictionary<string, object?> CreateDialogBlazorComponentParameters(params object[] parameters)
        {
            var result = new Dictionary<string, object?>();

            // Check if the number of parameters is even (key-value pairs)
            if (parameters.Length % 2 != 0)
            {
                throw new ArgumentException("Number of parameters must be even.");
            }

            // Process parameters
            for (int i = 0; i < parameters.Length; i += 2)
            {
                string? key = parameters[i] as string;
                object value = parameters[i + 1];

                if (key != null)
                {
                    result[key] = value;
                }
            }

            return result;
        }
    }
}
