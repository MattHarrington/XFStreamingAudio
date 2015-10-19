using System;
using Com.Spoledge.Aacdecoder;
using Android.Util;
using Xamarin.Forms;

namespace XFStreamingAudio.Droid
{
    public class AACPlayerCallback : Java.Lang.Object, IPlayerCallback
    {
        const string TAG = "KVMR";

        #region IPlayerCallback implementation

        public void PlayerAudioTrackCreated(Android.Media.AudioTrack p0)
        {
        }

        public void PlayerException(Java.Lang.Throwable ex)
        {
            Log.Debug(TAG, "AACPlayerCallback.PlayerException: " + ex);  
        }

        public void PlayerMetadata(string p0, string p1)
        {
        }

        public void PlayerPCMFeedBuffer(bool p0, int p1, int p2)
        {
        }

        public void PlayerStarted()
        {
            Log.Debug(TAG, "AACPlayerCallback.PlayerStarted()");            
            var playerStartedMessage = new PlayerStartedMessage();
            MessagingCenter.Send(playerStartedMessage, "PlayerStarted");
        }

        public void PlayerStopped(int p0)
        {
            Log.Debug(TAG, "AACPlayerCallback.PlayerStopped()");
            var playerStoppedMessage = new PlayerStoppedMessage();
            MessagingCenter.Send(playerStoppedMessage, "PlayerStopped");
        }

        #endregion
    }
}
