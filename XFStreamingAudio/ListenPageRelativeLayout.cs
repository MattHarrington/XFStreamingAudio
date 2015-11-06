using System;

using Xamarin.Forms;
using System.Diagnostics;
using XamSvg.XamForms;

namespace XFStreamingAudio
{
    public class ListenPageRelativeLayout : ContentPage
    {
        public ListenPageRelativeLayout()
        {
            this.Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 0);
            Icon = "ListenIcon";
            Title = "Listen";
            BackgroundColor = Color.Olive;
            RelativeLayout relativeLayout = new RelativeLayout();

            SvgImage whiteTower = new SvgImage
            { Svg = "res:Images.WhiteTower",
                BackgroundColor = Color.Red,
                HorizontalOptions = LayoutOptions.Start
            };
            
            Image launchSettingsImage = new Image
            {
                Source = "SettingsIcon",
                BackgroundColor = Color.Lime
            };

//            Image listenPageLogo = new Image
//            {
//                Source = "ListenPageLogo",
//                BackgroundColor = Color.Green
//            };

            SvgImage listenPageLogo = new SvgImage
                { Svg = "res:Images.ListenPageLogoSVG-outlined",
                    BackgroundColor = Color.Fuchsia,
                    HorizontalOptions=LayoutOptions.Start,
                    VerticalOptions=LayoutOptions.Start
                };

            Button playStopBtn = new Button
            {
                Text = "▶︎",
                TextColor = Color.FromHex("#007AFF"),
                FontSize = 48,
                BackgroundColor = Color.Lime,
                    HorizontalOptions=LayoutOptions.Start
            };
                        

            relativeLayout.Children.Add(whiteTower, 
                Constraint.Constant(0),
                Constraint.Constant(0)
            );

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
                        return parent.Width * 0.45;
                    }),
                Constraint.RelativeToParent((parent) =>
                    {
                        var y = 0.4 * parent.Height - listenPageLogo.Height / 2;
                        Debug.WriteLine("listenPageLogo y = {0}", y);
                        return y;
                    }),
                Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width * 0.5;
                    }
                ));

            relativeLayout.Children.Add(playStopBtn,
                Constraint.RelativeToView(listenPageLogo, (parent, sibling) =>
                    {
                        var x = sibling.X + sibling.Width / 2;
                        Debug.WriteLine("playStopBtn.Width: {0}", playStopBtn.Width);
                        Debug.WriteLine("playStopBtn x = {0}", x);
                        return x;
                    }),
                Constraint.RelativeToView(listenPageLogo, (parent, sibling) =>
                    {
                        var y = sibling.Y + sibling.Height + 40;
                        Debug.WriteLine("playStopBtn y = {0}", y);
                        return y;
                    }
                ));

            this.Content = relativeLayout;
        }
    }
}


