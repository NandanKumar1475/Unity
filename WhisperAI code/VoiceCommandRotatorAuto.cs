//using System;
//using System.Threading;
//using UnityEngine;
//using Whisper.Utils;

//namespace Whisper.Samples
//{
//    public class VoiceCommandRotatorAuto : MonoBehaviour
//    {
//        [Header("References")]
//        public WhisperManager whisper;
//        public MicrophoneRecord microphoneRecord;
//        public Transform target;

//        [Header("Rotation")]
//        public float stepAngle = 90f;           // how much one command rotates
//        public float rotateDuration = 0.5f;     // time (seconds) it should take for stepAngle
//        public bool autoStart = true;

//        // internals
//        private WhisperStream _stream;
//        private readonly object _cmdLock = new object();
//        private string _pendingCommand;         // latest transcript (thread-safe)
//        private double _lastCmdReceivedTime;    // realtime to debounce transcripts

//        // rotation state (driven in Update for instant switching)
//        private float _targetYaw;               // world yaw we want to rotate to (degrees)
//        private bool _hasTarget = false;
//        private bool _stopped = true;           // true when no active turning
//        private float _degreesPerSecond;        // derived from stepAngle/rotateDuration

//        private void Awake()
//        {
//            // compute speed (degrees/sec). protect divide-by-zero
//            _degreesPerSecond = (rotateDuration > 0f) ? Mathf.Abs(stepAngle) / rotateDuration : 360f;
//        }

//        private async void Start()
//        {
//            if (whisper == null || microphoneRecord == null || target == null)
//            {
//                Debug.LogError("[VCR_AUTO] Assign whisper, microphoneRecord and target in Inspector.");
//                enabled = false;
//                return;
//            }

//            Debug.Log("[VCR_AUTO] Creating stream...");
//            _stream = await whisper.CreateStream(microphoneRecord);
//            if (_stream == null)
//            {
//                Debug.LogError("[VCR_AUTO] CreateStream returned null.");
//                return;
//            }

//            _stream.OnResultUpdated += OnResultReceived;
//            _stream.OnStreamFinished += s => Debug.Log("[VCR_AUTO] Stream finished: " + s);
//            _stream.OnSegmentFinished += seg => Debug.Log("[VCR_AUTO] Segment finished: " + (seg?.Result ?? "<null>"));

//            Debug.Log("[VCR_AUTO] Stream ready.");

//            // initialize yaw from current target rotation
//            _targetYaw = target.eulerAngles.y;
//            _hasTarget = true;
//            _stopped = true;

//            if (autoStart)
//            {
//                Debug.Log("[VCR_AUTO] autoStart -> starting stream + mic");
//                _stream.StartStream();
//                microphoneRecord.StartRecord();
//            }
//        }

//        // Called on background thread by the stream - store latest safely
//        private void OnResultReceived(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text)) return;

//            // tiny debounce: ignore extremely fast repeated callbacks (partial transcripts)
//            var now = Time.realtimeSinceStartupAsDouble;
//            lock (_cmdLock)
//            {
//                // accept it if > 0.08s since last accepted transcript (reduces chatter)
//                if (now - _lastCmdReceivedTime < 0.08) return;
//                _pendingCommand = text;
//                _lastCmdReceivedTime = now;
//            }
//            Debug.Log($"[VCR_AUTO] Received: '{text}'");
//        }

//        private void Update()
//        {
//            // pick up latest transcript on main thread
//            string cmd = null;
//            lock (_cmdLock)
//            {
//                if (!string.IsNullOrEmpty(_pendingCommand))
//                {
//                    cmd = _pendingCommand;
//                    _pendingCommand = null;
//                }
//            }

//            if (!string.IsNullOrEmpty(cmd))
//            {
//                ProcessCommandLive(cmd);
//            }

//            // If we have a target yaw, rotate toward it with a consistent speed
//            if (_hasTarget && !_stopped)
//            {
//                float currentYaw = target.eulerAngles.y;
//                // compute shortest angle difference (-180..180)
//                float delta = Mathf.DeltaAngle(currentYaw, _targetYaw);
//                if (Mathf.Abs(delta) < 0.25f)
//                {
//                    // close enough -> snap and stop
//                    var r = target.rotation;
//                    r = Quaternion.Euler(r.eulerAngles.x, _targetYaw, r.eulerAngles.z);
//                    target.rotation = r;
//                    _stopped = true;
//                    Debug.Log("[VCR_AUTO] Reached target yaw; stopped.");
//                }
//                else
//                {
//                    // step this frame: sign-preserving, clamp by degrees/sec * dt
//                    float maxStep = _degreesPerSecond * Time.deltaTime;
//                    float step = Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), maxStep);
//                    float newYaw = currentYaw + step;
//                    var newRot = Quaternion.Euler(target.eulerAngles.x, newYaw, target.eulerAngles.z);
//                    target.rotation = newRot;
//                }
//            }
//        }

//        // parse and immediately update target yaw; this overrides whatever was happening
//        private void ProcessCommandLive(string txt)
//        {
//            var lower = txt.ToLowerInvariant();
//            Debug.Log("[VCR_AUTO] Processing: " + lower);

//            // basic synonyms
//            bool isLeft = lower.Contains("left") || lower.Contains("turn left") || lower.Contains("go left");
//            bool isRight = lower.Contains("right") || lower.Contains("turn right") || lower.Contains("go right");
//            bool isStop = lower.Contains("stop") || lower.Contains("halt") || lower.Contains("freeze");

//            // If nothing recognized, try simple words like "move" + direction words later
//            if (!isLeft && !isRight && !isStop)
//            {
//                Debug.Log("[VCR_AUTO] No actionable word found in transcript.");
//                return;
//            }

//            // pick current yaw and compute new target yaw immediately (overrides current target)
//            float currentYaw = target.eulerAngles.y;

//            if (isStop)
//            {
//                // cancel movement and set target to current yaw (instant stop)
//                _targetYaw = currentYaw;
//                _stopped = true;
//                _hasTarget = true;
//                Debug.Log("[VCR_AUTO] STOP received: halting immediately.");
//                return;
//            }

//            // On left/right, compute a new target yaw relative to current yaw (so multiple rapid commands stack naturally)
//            float delta = isLeft ? -Mathf.Abs(stepAngle) : Mathf.Abs(stepAngle);

//            // Allow accumulation if commands arrive quickly: target becomes (currentYaw + delta)
//            // This gives user ability to say "right", "right" -> two steps.
//            _targetYaw = NormalizeAngle(currentYaw + delta);
//            _hasTarget = true;
//            _stopped = false;

//            Debug.Log($"[VCR_AUTO] New target yaw: {_targetYaw} (delta {delta})");
//        }

//        private static float NormalizeAngle(float a)
//        {
//            // keep within 0..360
//            a %= 360f;
//            if (a < 0f) a += 360f;
//            return a;
//        }

//        private void OnDestroy()
//        {
//            if (_stream != null)
//            {
//                _stream.OnResultUpdated -= OnResultReceived;
//                try { _stream.StopStream(); } catch { }
//            }
//        }

//        // toggle mic + stream safely
//        public void ToggleStartStop()
//        {
//            if (_stream == null || microphoneRecord == null)
//            {
//                Debug.LogWarning("[VCR_AUTO] ToggleStartStop called before stream or mic initialized.");
//                return;
//            }

//            if (!microphoneRecord.IsRecording)
//            {
//                try
//                {
//                    _stream.StartStream();
//                    microphoneRecord.StartRecord();
//                    Debug.Log("[VCR_AUTO] Started stream+mic");
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogError("[VCR_AUTO] Failed to start: " + ex.Message);
//                }
//            }
//            else
//            {
//                try
//                {
//                    microphoneRecord.StopRecord();
//                    _stream.StopStream();
//                    Debug.Log("[VCR_AUTO] Stopped mic+stream");
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogError("[VCR_AUTO] Failed to stop: " + ex.Message);
//                }
//            }
//        }
//    }
//}





using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Voice-driven rotator with continuous-mode:
    /// - Saying "left" starts continuous left rotation.
    /// - Saying "right" switches immediately to continuous right rotation.
    /// - Saying "stop" halts rotation.
    /// - Numeric rotate commands ("rotate 45") still supported.
    /// </summary>
    public class VoiceCommandRotatorAuto : MonoBehaviour
    {
        [Header("References")]
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public Transform target;
        public Text lastCommandUIText; // optional UI feedback

        [Header("Rotation Settings")]
        public float stepAngle = 90f;         // used for numeric/step commands
        public float rotateDuration = 0.45f;  // time to complete stepAngle (affects speed)
        public float maxDegreesPerSecond = 720f; // clamp for speed

        [Header("Input Tuning")]
        public int debounceMs = 80;               // ignore transcripts within this ms
        public bool acceptNumericRotate = true;   // allow "rotate 45" style commands

        // internals
        private WhisperStream _stream;
        private readonly object _cmdLock = new object();
        private string _pendingTranscript;
        private double _lastAcceptedRealtime;
        private float _degreesPerSecond;    // current speed (deg/sec)
        private static readonly Regex _rotateNumRegex = new Regex(@"rotate\s*(-?\d+(\.\d+)?)|(-?\d+(\.\d+)?)\s*deg", RegexOptions.IgnoreCase);

        // continuous-mode state
        private bool _continuousMode = false; // when true, rotate continuously in _continuousDir
        private int _continuousDir = 0;       // -1 = left, +1 = right

        private void Awake()
        {
            // initial speed derived from stepAngle/rotateDuration, clamped
            _degreesPerSecond = (rotateDuration > 0f) ? Mathf.Min(Mathf.Abs(stepAngle) / rotateDuration, maxDegreesPerSecond) : maxDegreesPerSecond;
        }

        private async void Start()
        {
            if (whisper == null || microphoneRecord == null || target == null)
            {
                Debug.LogError("[VCR_AUTO] Assign whisper, microphoneRecord and target in Inspector.");
                enabled = false;
                return;
            }

            Debug.Log("[VCR_AUTO] Creating stream...");
            _stream = await whisper.CreateStream(microphoneRecord);
            if (_stream == null)
            {
                Debug.LogError("[VCR_AUTO] CreateStream returned null.");
                return;
            }

            _stream.OnResultUpdated += OnResultReceived;
            _stream.OnStreamFinished += s => Debug.Log("[VCR_AUTO] Stream finished: " + s);
            _stream.OnSegmentFinished += seg => Debug.Log("[VCR_AUTO] Segment finished: " + (seg?.Result ?? "<null>"));

            Debug.Log("[VCR_AUTO] Stream ready.");

            // try auto-start but catch failures
            try
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
                Debug.Log("[VCR_AUTO] Auto started stream+mic");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[VCR_AUTO] Auto-start failed: " + ex.Message);
            }
        }

        // background-thread callback: stash transcript for main thread
        private void OnResultReceived(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_cmdLock)
            {
                _pendingTranscript = text.Trim();
            }
        }

        private void Update()
        {
            // fetch transcript to main thread
            string transcript = null;
            lock (_cmdLock)
            {
                if (!string.IsNullOrEmpty(_pendingTranscript))
                {
                    transcript = _pendingTranscript;
                    _pendingTranscript = null;
                }
            }

            if (!string.IsNullOrEmpty(transcript))
            {
                var now = Time.realtimeSinceStartupAsDouble;
                if (now - _lastAcceptedRealtime >= (debounceMs / 1000.0))
                {
                    if (ProcessTranscriptForContinuous(transcript))
                    {
                        _lastAcceptedRealtime = now;
                    }
                }
                else
                {
                    Debug.Log("[VCR_AUTO] Transcript ignored by debounce: " + transcript);
                }
            }

            // If in continuous mode, rotate continuously each frame
            if (_continuousMode && _continuousDir != 0)
            {
                float stepThisFrame = _degreesPerSecond * Time.deltaTime;
                float newYaw = target.eulerAngles.y + (_continuousDir * stepThisFrame);
                var newRot = Quaternion.Euler(target.eulerAngles.x, newYaw, target.eulerAngles.z);
                target.rotation = newRot;
            }
        }

        // Returns true if an actionable command was processed
        private bool ProcessTranscriptForContinuous(string text)
        {
            var lower = text.ToLowerInvariant();
            Debug.Log("[VCR_AUTO] Received transcript: " + lower);

            // STOP
            if (lower.Contains("stop") || lower.Contains("halt") || lower.Contains("freeze") || lower.Contains("hold"))
            {
                _continuousMode = false;
                _continuousDir = 0;
                SetUI("STOP");
                Debug.Log("[VCR_AUTO] STOP -> continuous mode off");
                return true;
            }

            // numeric rotate (explicit degree) still supported
            if (acceptNumericRotate)
            {
                var m = _rotateNumRegex.Match(lower);
                if (m.Success)
                {
                    string numStr = m.Groups[1].Value;
                    if (string.IsNullOrEmpty(numStr)) numStr = m.Groups[3].Value;
                    if (float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float deg))
                    {
                        // apply immediate single-step rotation to current yaw (not continuous)
                        float newYaw = target.eulerAngles.y + deg;
                        target.rotation = Quaternion.Euler(target.eulerAngles.x, newYaw, target.eulerAngles.z);
                        SetUI($"ROTATE {deg}°");
                        Debug.Log($"[VCR_AUTO] Numeric rotate {deg}° applied immediately");
                        return true;
                    }
                }
            }

            // LEFT / RIGHT -> start (or switch) continuous rotation
            bool left = lower.Contains("left") || lower.Contains("turn left") || lower.Contains("go left") || lower.Contains("move left");
            bool right = lower.Contains("right") || lower.Contains("turn right") || lower.Contains("go right") || lower.Contains("move right");

            if (left || right)
            {
                int dir = left ? -1 : 1;
                // If already in continuous mode and same dir -> keep rotating
                // If different dir -> switch instantly
                _continuousMode = true;
                _continuousDir = dir;

                // Optionally adjust speed based on modifiers
                float speedMultiplier = 1f;
                if (lower.Contains("slow") || lower.Contains("slightly")) speedMultiplier = 0.5f;
                if (lower.Contains("fast") || lower.Contains("quick") || lower.Contains("hard")) speedMultiplier = 2f;
                _degreesPerSecond = Mathf.Clamp((Mathf.Abs(stepAngle) / Mathf.Max(0.001f, rotateDuration)) * speedMultiplier, 1f, maxDegreesPerSecond);

                SetUI(left ? "LEFT (continuous)" : "RIGHT (continuous)");
                Debug.Log($"[VCR_AUTO] Continuous {(left ? "LEFT" : "RIGHT")} started, speed {_degreesPerSecond}°/s");
                return true;
            }

            Debug.Log("[VCR_AUTO] No actionable continuous command found.");
            return false;
        }

        private void SetUI(string text)
        {
            if (lastCommandUIText == null) return;
            try
            {
                lastCommandUIText.text = text;
            }
            catch { }
        }

        private void OnDestroy()
        {
            if (_stream != null)
            {
                _stream.OnResultUpdated -= OnResultReceived;
                try { _stream.StopStream(); } catch { }
            }
        }

        // toggle mic + stream safely
        public void ToggleStartStop()
        {
            if (_stream == null || microphoneRecord == null)
            {
                Debug.LogWarning("[VCR_AUTO] ToggleStartStop called before initialization.");
                return;
            }

            try
            {
                if (!microphoneRecord.IsRecording)
                {
                    _stream.StartStream();
                    microphoneRecord.StartRecord();
                    Debug.Log("[VCR_AUTO] Started stream+mic");
                }
                else
                {
                    microphoneRecord.StopRecord();
                    _stream.StopStream();
                    Debug.Log("[VCR_AUTO] Stopped mic+stream");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[VCR_AUTO] Toggle failed: " + ex.Message);
            }
        }
    }
}
