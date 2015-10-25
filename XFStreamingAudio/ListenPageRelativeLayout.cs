using System;

using Xamarin.Forms;
using System.Diagnostics;

namespace XFStreamingAudio
{
    public class ListenPageRelativeLayout : ContentPage
    {
        public ListenPageRelativeLayout()
        {
            this.Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5);
            BackgroundColor = Color.Blue;

            Image whiteTower = new Image
            { Source = "WhiteTower", Aspect = Aspect.AspectFill, 
                BackgroundColor = Color.Red
            };

            Image launchSettingsImage = new Image
            {
                Source = "SettingsIcon",
                BackgroundColor = Color.Lime
            };

            Image listenPageLogo = new Image
            {
                Source = "ListenPageLogo",
                BackgroundColor = Color.Green
            };

            Button playStopBtn = new Button
            {
                Text = "▶︎",
                TextColor = Color.FromHex("#007AFF"),
                FontSize = 48,
                BackgroundColor = Color.Lime
            };
                        
            RelativeLayout relativeLayout = new RelativeLayout();

            relativeLayout.Children.Add(whiteTower, Constraint.RelativeToParent((parent) =>
                    {
                        Debug.WriteLine("parent.Width = {0}", parent.Width);
                        Debug.WriteLine("parent.Height = {0}", parent.Height);
                        Debug.WriteLine("whiteTower.Width = {0}", whiteTower.Width);
                        Debug.WriteLine("whiteTower.Height = {0}", whiteTower.Height);
                        return 0;
                    }
                ));

            relativeLayout.Children.Add(launchSettingsImage,
                Constraint.RelativeToParent((parent) =>
                    {
                        Debug.WriteLine("whiteTower.Width = {0}", whiteTower.Width);
                        Debug.WriteLine("whiteTower.Height = {0}", whiteTower.Height);
                        return parent.Width - 30;
                    }),
                Constraint.RelativeToParent((parent) =>
                    {
                        var y = 15;
                        Debug.WriteLine("listenPageLogo y = {0}", y);
                        return y;
                    }
                ));

            relativeLayout.Children.Add(listenPageLogo,
                Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width / 3;
                    }),
                Constraint.RelativeToParent((parent) =>
                    {
                        var y = 0.4 * parent.Height - listenPageLogo.Height / 2;
                        Debug.WriteLine("listenPageLogo y = {0}", y);
                        return y;
                    }
                ));

            relativeLayout.Children.Add(playStopBtn,
                Constraint.RelativeToView(listenPageLogo, (parent, sibling) =>
                    {
                        var x = sibling.X + sibling.Width / 2;
                        Debug.WriteLine("playStopBtn x = {0}", x);
                        return x;
                    }),
                Constraint.RelativeToView(listenPageLogo, (parent, sibling) =>
                    {
                        var y = sibling.Y + sibling.Height;
                        Debug.WriteLine("playStopBtn y = {0}", y);
                        return y;
                    }
                ));

            this.Content = relativeLayout;
        }
    }
}


