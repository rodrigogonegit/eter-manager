using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EterManager.Utilities
{
    public static class DropCommand
    {
        public static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(ICommand),
            typeof(DropCommand),
            new PropertyMetadata(null, OnDropCommandChange));

        public static void SetDropCommand(DependencyObject source, ICommand value)
        {
            source.SetValue(DropCommandProperty, value);
        }

        public static ICommand GetDropCommand(DependencyObject source)
        {
            return (ICommand)source.GetValue(DropCommandProperty);
        }

        private static void OnDropCommandChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ICommand command = e.NewValue as ICommand;
            UIElement uiElement = d as UIElement;
            if (command != null && uiElement != null)
            {
                uiElement.Drop += (sender, args) => command.Execute(args.Data);
            }

            // todo: if e.OldValue is not null, detatch the handler that references it
        }
    }
}
