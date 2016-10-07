using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class PledgePage : ContentPage
    {
        public PledgePage()
        {
            InitializeComponent();
            donateBtn.Clicked += DonateBtn_Clicked;
            //buildingCampaignBtn.Clicked += BuildingCampaignBtn_Clicked;
            volunteerBtn.Clicked += VolunteerBtn_Clicked;
            //membershipBtn.Clicked += MembershipBtn_Clicked;
            //vehicleDonationBtn.Clicked += VehicleDonationBtn_Clicked;
            plannedGiftsBtn.Clicked += PlannedGiftsBtn_Clicked;
            //underwriterBtn.Clicked += UnderwriterBtn_Clicked;
        }

        void PlannedGiftsBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://kalx.berkeley.edu/kind-donations"));
        }

        void VolunteerBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://kalx.berkeley.edu/volunteering-kalx"));
        }

        void DonateBtn_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://kalx.berkeley.edu/how-make-monetary-donations"));
        }
    }
}
