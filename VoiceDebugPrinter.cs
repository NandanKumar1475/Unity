using System.Collections.Concurrent;
using UnityEngine;

namespace Whisper.Samples
{
    public class VoiceDebugPrinter : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;

        private WhisperStream _stream;
        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        private async void Start()
        {
            if (whisper == null || microphoneRecord == null)
            {
                Debug.LogError("Assign WhisperManager and MicrophoneRecord in Inspector!");
                return;
            }

            _stream = await whisper.CreateStream(microphoneRecord);

            if (_stream == null)
            {
                Debug.LogError("Whisper stream creation failed!");
                return;
            }

            // When Whisper hears something → OnTextReceived fires
            _stream.OnResultUpdated += OnTextReceived;

            // Start listening
            _stream.StartStream();
            microphoneRecord.StartRecord();

            Debug.Log("VOICE DEBUG STARTED. Speak now.");
        }

        private void OnTextReceived(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                _queue.Enqueue(text);
        }

        private void Update()
        {
            while (_queue.TryDequeue(out var txt))
            {
                Debug.Log("YOU SAID: " + txt);
            }
        }
    }
}
