using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Leap;

namespace ElBruno.SlapUrBossAway
{
    public class ElBrunoListener : Listener
    {
        private readonly Object _thisLock = new Object();
        public event Action<bool> OnSwipeDetected;


        private void SafeWriteLine(String line)
        {
            lock (_thisLock)
            {
                Debug.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            var gestures = frame.Gestures();
            foreach (var swipe in from gesture in gestures where gesture.Type == Gesture.GestureType.TYPESWIPE select new SwipeGesture(gesture))
            {
                var gestureName = GetGestureNameFromSwipe(swipe);
                SafeWriteLine(string.Format("Swipe id: {0}, name: {1}", swipe.Id, gestureName));
                break;
            }
            if (frame.Gestures().Count > 0)
                Task.Factory.StartNew(() => OnSwipeDetected(true));
        }

        private string GetGestureNameFromSwipe(SwipeGesture swipe)
        {
            var gestureName = "undefined";
            var direction = swipe.Direction;
            if (direction.Yaw > 0)
            {
                gestureName = "left to right";
            }
            if (direction.Yaw < 0)
            {
                gestureName = "right to left";
            }
            return gestureName;
        }
    }
}