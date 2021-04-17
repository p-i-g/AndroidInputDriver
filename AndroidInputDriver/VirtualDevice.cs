using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Input.Preview.Injection;

namespace AndroidInputDriver {
    class VirtualDevice {

        private InputInjector inputInjector;
        private uint pointerID;
        bool prevContact = false;

        public VirtualDevice(PointerPoint point) {
            inputInjector = InputInjector.TryCreate();

            pointerID = point.PointerId + 1;
        }
        public void InjectInputForPen(int x, int y, double pressure, bool contact) {
            prevContact = contact;
            InjectedInputPenInfo info = new InjectedInputPenInfo();
            info.PenParameters = InjectedInputPenParameters.Pressure;
            InjectedInputPointerOptions options;
            if (contact) {
                if (prevContact) {
                    options = InjectedInputPointerOptions.Update | InjectedInputPointerOptions.InContact | InjectedInputPointerOptions.InRange;
                } else {
                    options = InjectedInputPointerOptions.PointerDown | InjectedInputPointerOptions.InContact | InjectedInputPointerOptions.InRange;
                    System.Diagnostics.Debug.WriteLine("pointer down");
                }
            } else {
                if (prevContact) {
                    options = InjectedInputPointerOptions.PointerUp | InjectedInputPointerOptions.InRange;
                } else {
                    options = InjectedInputPointerOptions.Update | InjectedInputPointerOptions.InRange;
                }
            }
            info.PointerInfo = new InjectedInputPointerInfo {
                PointerId = pointerID,
                PointerOptions = options,
                TimeOffsetInMilliseconds = 0,
                PixelLocation = new InjectedInputPoint {
                    PositionX = x,
                    PositionY = y
                }
            };
            info.Pressure = pressure;
            inputInjector.InjectPenInput(info);
        }
        public void InjectInputForKeyboard(string[] codesString) {
            InjectedInputKeyboardInfo[] info = new InjectedInputKeyboardInfo[codesString.Length];
            for (int i = 0; i < codesString.Length; i++) {
                info[i] = new InjectedInputKeyboardInfo();
                info[i].VirtualKey = ushort.Parse(codesString[i]);
                info[i].KeyOptions = InjectedInputKeyOptions.None;
            }
            inputInjector.InjectKeyboardInput(info);
            for(int i = 0; i < info.Length; i++) {
                info[i].KeyOptions = InjectedInputKeyOptions.KeyUp;
            }
            inputInjector.InjectKeyboardInput(info);
        }
    }
}
