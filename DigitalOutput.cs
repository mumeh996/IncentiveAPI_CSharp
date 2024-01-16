using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automation.BDaq;

namespace IncentiveTest.Classes
{
    public class DigitalOutput
    {
        public DigitalOutput(int USBID) // constructor for USB outputs
        {
            _address[0] = 0;
            _address[1] = 0;
            _instantDoCtrl.SelectedDevice = new DeviceInformation(USBID);
        }
        public bool initEtherCAT(int etherCAT_ID) //look for and initialize EtherCAT connection
        {
            return true;
        }

        public bool initUSB(int USB_ID) //look for and initialize USB connection
        {
            return true;
        }

        // Uses Advantech USB-5855 IO interfaces with DAQNavi drivers

        private byte[] _address = new byte[2];
        private readonly InstantDoCtrl _instantDoCtrl = new InstantDoCtrl();
      
       

        public void SetOutput(int channel, OutputState state) 
        {
            int index = channel / 8; // select address bank 
            int bitIndex = channel % 8; // select bit
            lock (this)
            {
                if (state == OutputState.On)
                {
                    _address[index] = (byte)SetBit(_address[index], bitIndex);
                }
                else
                {
                    _address[index] = (byte)ClearBit(_address[index], bitIndex);
                }
                System.Console.WriteLine("setting output channel {0} {1} {2}", channel, state, _address[index]);
                _instantDoCtrl.Write(index, _address[index]);
            }
        }
        public int ReadOutput(int channel) // channels are byte-wide - 0, 8, 16
        {
            int index = channel / 8; // select address bank 
            byte bitIndex = (byte)(channel % 8); // select bit
            byte data;
            lock (this)
            {
                int rawReadValue = (int)_instantDoCtrl.ReadBit(_address[index], bitIndex, out data);
                // Console.WriteLine(index + "/" + bitIndex + "/" + rawReadValue.ToString("X") + "/" + data.ToString("X") + "/" + _address[index]);
                return _address[index];
            }
        }
        public enum OutputState
        {
            Off = 0,
            On = 1,
        }

        private int SetBit(int value, int index)
        {
            int answer = value | (1 << index);
            return answer;
        }

        private int ClearBit(int value, int index)
        {
            int answer = value & ~(1 << index);
            return answer;
        }

        //TODO - add ability to hunt for USB devices
        private bool ScanForAdvantech() //hunt for Advantech DIO devices on the USB chain 
        {
            return true;
        }

    }

}
