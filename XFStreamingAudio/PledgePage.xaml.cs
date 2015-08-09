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
            buildingCampaignBtn.Clicked += BuildingCampaignBtn_Clicked;
            volunteerBtn.Clicked += VolunteerBtn_Clicked;
            membershipBtn.Clicked += MembershipBtn_Clicked;
            vehicleDonationBtn.Clicked += VehicleDonationBtn_Clicked;
            plannedGiftsBtn.Clicked += PlannedGiftsBtn_Clicked;
            underwriterBtn.Clicked += UnderwriterBtn_Clicked;
        }

        void UnderwriterBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/content/become-underwriter"));
        }

        void PlannedGiftsBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/content/planned-gifts"));
        }

        void VehicleDonationBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/content/vehicle-donation"));
        }

        void MembershipBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/content/membership"));
        }

        void VolunteerBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/content/volunteer"));
        }

        void BuildingCampaignBtn_Clicked (object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/content/bridge-street-project"));
        }

        void DonateBtn_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.kvmr.org/donate"));
        }
    }
}
