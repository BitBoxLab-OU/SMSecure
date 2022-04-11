using AnonymousWhiteLabel.UWP.Services;
using MessageCompose.Services;
using System;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioRecorder))]
namespace AnonymousWhiteLabel.UWP.Services
{
    public class AudioRecorder : IAudioRecorder
    {
        private MediaCapture _mediaCapture ;
        private InMemoryRandomAccessStream _memoryBuffer = new InMemoryRandomAccessStream();

        public bool IsRecording { get; set; }


        public  byte[] GetOutput() {
            if(IsRecording)
                StopRecording();
            IRandomAccessStream s = _memoryBuffer.CloneStream();
            var dr = new DataReader(s.GetInputStreamAt(0));
            var bytes = new byte[s.Size];
            _ = dr.LoadAsync((uint)s.Size);
            dr.ReadBytes(bytes);
            return bytes;
        }
        public void PlayRecording() {
            MediaElement playbackMediaElement = new MediaElement();
            playbackMediaElement.SetSource(_memoryBuffer, "MP3");
            playbackMediaElement.Play();
        }
        public  async void StartRecording()
        {
            if (IsRecording)
            {
                throw new InvalidOperationException("Recording already in progress!");
            }
            _mediaCapture = new MediaCapture();

            await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio });

            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
            //profile.Audio = AudioEncodingProperties.CreatePcm(sampleRate, channels, bitsPerSample);


            await _mediaCapture.StartRecordToStreamAsync(profile, _memoryBuffer);


            //MediaCaptureInitializationSettings settings =
            //  new MediaCaptureInitializationSettings
            //  {
            //      StreamingCaptureMode = StreamingCaptureMode.Audio
            //  };
          
            //await _mediaCapture.InitializeAsync(settings);
            //await _mediaCapture.StartRecordToStreamAsync(
            //  MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), _memoryBuffer);
            IsRecording = true;
        }
        public  void StopRecording() {
            var size = _memoryBuffer.Size;
            _ = _mediaCapture.StopRecordAsync();
            IsRecording = false;
            _mediaCapture.Dispose();
        }
    }
}
