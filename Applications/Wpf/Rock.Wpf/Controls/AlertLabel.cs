﻿using System.Windows;
using System.Windows.Controls;

namespace Rock.Wpf.Controls
{
    /// <summary>
    /// A Bootstrap style Alert
    /// </summary>
    /// <seealso cref="System.Windows.Controls.TextBlock" />
    public class AlertLabel : Label
    {
        private AlertMessageType alertMessageType;

        public AlertLabel()
        {
            Style style = new Style( typeof( TextBlock ) );
            style.Setters.Add( new Setter() { TargetName = "TextWrapping", Value = TextWrapping.Wrap } );
            Resources.Add( "Style", style );
        }

        /// <summary>
        /// Gets or sets the type of the alert.
        /// </summary>
        /// <value>
        /// The type of the alert.
        /// </value>
        public AlertMessageType AlertType
        {
            get => alertMessageType;
            set
            {
                alertMessageType = value;

                switch ( alertMessageType )
                {
                    case AlertMessageType.Danger:
                        this.Style = Application.Current.Resources["labelStyleAlertDanger"] as Style;
                        break;
                    case AlertMessageType.Info:
                        this.Style = Application.Current.Resources["labelStyleAlertInfo"] as Style;
                        break;
                    case AlertMessageType.Warning:
                        this.Style = Application.Current.Resources["labelStyleAlertWarning"] as Style;
                        break;
                    case AlertMessageType.Success:
                        this.Style = Application.Current.Resources["labelStyleAlertSuccess"] as Style;
                        break;
                    default:
                        this.Style = Application.Current.Resources["labelStyleAlertBase"] as Style;
                        return;
                }
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get => this.Content as string;
            set => Content = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AlertMessageType
    {
        Success,
        Info,
        Warning,
        Danger
    }
}
