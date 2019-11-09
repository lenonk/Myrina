using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MsgBox {
    class MessageBox : Window {
        public enum MessageBoxButtons {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult {
            Ok,
            Cancel,
            Yes,
            No
        }

        public MessageBox() {
            AvaloniaXamlLoader.Load(this);
        }

        public static void Show(string text, Window parent = null) {
            Show(parent, text, "", MessageBoxButtons.Ok);
        }

        public static Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons) {
            var msgbox = new MessageBox() {
                Title = title
            };

            msgbox.FindControl<TextBlock>("Text").Text = text;

            var buttonPanel = msgbox.FindControl<StackPanel>("Buttons");
            var res = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool def = false) {
                var btn = new Button { Content = caption };
                btn.Click += (_, __) => {
                    res = r;
                    msgbox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (def)
                    res = r;
                btn.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            }

            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
                AddButton("Ok", MessageBoxResult.Ok, true);
            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel) {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, true);
            }

            if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
                AddButton("Cancel", MessageBoxResult.Cancel, true);

            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.Closed += delegate { tcs.TrySetResult(res); };
            if (parent != null)
                msgbox.ShowDialog(parent);
            else msgbox.Show();
            //msgbox.HandleResized(msgbox.ClientSize.WithHeight(msgbox.ClientSize.Height + 1));
            return tcs.Task;
        }
    }
}