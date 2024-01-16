using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTC_Incentive;
using System.Threading;
using System.Collections;
using System.Windows.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace IncentiveTest
{
    /// <summary>
    /// Class IncentiveAPI_CSharp.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class IncentiveAPI_CSharp : Form
    {
        Controller.PLCLogic testplc = new Controller.PLCLogic();                    // Connection to PLC Logic controller stored here
        Controller.AxisSupervisor axisSupervisor = new Controller.AxisSupervisor(); // Connection to overall axis supervisor, typically used in EtherCAT_Master standalone mode
        CTC_Incentive.RuntimeManagement RuntimeSystem = new CTC_Incentive.RuntimeManagement(1);  // Runtime management for start/stop and monitoring activity

        ArrayList axisList = new ArrayList();           // Store axis objects here so know what connections are made and can terminate threads
        bool abortThread = false;                       // Used by threads to monitor at the top of their loop for any abort requests.
        int numAxis = 0;                                // Global for number of axis found available on the PLC
        int numOutputs = 0;                             // Global for number of outputs found available on the PLC
        int ecat_online_status = 0;                     // EtherCAT online status variable monitored by a background thread to ensure still online.

        // ******** Optional Position test parameters
        bool changeOccurred = false;                    // When the Position Update button is clicked we set this
        double Maxspeed = 0;
        double Position = 0;
        double Accel = 0;
        double Decel = 0;

        bool connected_plc = false;                     // Set when connected to PLC and registers are available.

        // Connection names
        string plcName = "CTPLC_1";         // Name of PLC Logic node
        string ecatName = "CTECAT_1";       // Name of EtherCAT Master node
        string ourSpecialName = "";         // Full access name

        string hostNetworkPath = "";  // Local execution of this program does not need "Hostname" + domain from INtime Configuration, Network tab.
//      string hostNetworkPath = "protech.ctc-control.com/";  // Remote execution of this program, only works over network, not local where plclogic is the
                                                                 // name registered by the INtime Network, CTPLC_1 process.

        /// <summary>
        /// Initializes a new instance of the <see cref="IncentiveAPI_CSharp"/> class.
        /// </summary>
        public IncentiveAPI_CSharp()
        {
            InitializeComponent();

            // Get our name prepared
            string ourIP = GetLocalIpAddress();
            if (ourIP == null)
            {
                // found nothing, for now just make one up but really should alert the user and not continue.
                ourIP = "172.16.123.456";
                MessageBox.Show("No valid IP address could be found?  Recommend correcting before continuing.");
            }

            // Use the last 6 digits since on most networks this will be unique, we are using this for global mailbox naming so unique name.
            // Other methods could be used as well.
            string ourNum = ourIP.Substring(ourIP.LastIndexOf('.') + 1);
            string startNum = ourIP.Substring(0, ourIP.LastIndexOf('.'));
            ourSpecialName = startNum.Substring(startNum.LastIndexOf('.') + 1) + ourNum;
        }

        /// <summary>
        /// Updates the state of the system. as well as enabled/disabled buttons for starting and stopping INtime.
        /// </summary>
        public void updateSystemState()
        {
            // Get the current state of the INtime runtime.  If running, which processes.
            RuntimeSystem.updateCurrentStateInformation(hostNetworkPath);
            if ((RuntimeSystem.EtherCAT_started) && (RuntimeSystem.Plclogic_started))
            {
                // Both CTPLC_1 ad CTECAT_1 are running...
                chk_AxisOnly.Checked = false;
                btnIndicator.BackColor = Color.Yellow;
                btnIndicator.Text = "Wait ECAT Operational";  // We will not enable test buttons until EtherCAT network is fully up.
                btnIndicator.Refresh();
                btnStartIncentive.Enabled = false;
                btnStopIncentive.Enabled = true;   // Allow stopping of INtime since is running.
                btn_Start.Enabled = false;
                btn_Stop.Enabled = false;
                btn_Start.Refresh();
                btn_Stop.Refresh();
            }
            else if (RuntimeSystem.EtherCAT_started)
            {
                // Only EtherCAT, CTECAT_1 is running
                chk_AxisOnly.Checked = true;
                btnIndicator.BackColor = Color.Yellow;
                btnIndicator.Text = "Wait ECAT Operational";  // We will not enable test buttons until EtherCAT network is fully up.
                btnIndicator.Refresh();
                btnStartIncentive.Enabled = false;
                btnStopIncentive.Enabled = true;    // Allow stopping of INtime since is running.
                btn_Start.Enabled = false;
                btn_Stop.Enabled = false;
                btn_Start.Refresh();
                btn_Stop.Refresh();
            }
            else
            {
                // Nothing is running
                btnIndicator.BackColor = Color.Red;
                btnIndicator.Text = "Stopped";
                btnIndicator.Refresh();
                btnStartIncentive.Enabled = true;  // Enable starting of INtime.
                btn_Start.Enabled = false;
                btn_Stop.Enabled = false;
                btn_Start.Refresh();
                btn_Stop.Refresh();
            }
//            btn_Start.Enabled = true;

        }
        /// <summary>
        /// Get the local IP address on this computer, best guess.  This is used to create a special mailbox name unique to this computer.
        /// </summary>
        public string GetLocalIpAddress()
        {
            UnicastIPAddressInformation mostSuitableIp = null;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }

                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null
                ? mostSuitableIp.Address.ToString()
                : "";
        }

        /// <summary>
        /// Event fired off when Start Test button is clicked, attaches to PLC Logic and EtherCAT process and starts motion control threads.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void btn_Start_Click(object sender, EventArgs e)
        {
            int iValue = 0;
//            double dValue = 0;
            if (connected_plc)
            {
                // Only do it once since we are connected...
                return;
            }

            // Disable further starts until ready
            btn_Start.Enabled = false;
            btn_Start.Refresh();

            // All mailboxes must have a unique name so we will use the IP Address last 6 characters combined with other characters to be unique.
            // Plc - P[ipadd6], which also becomes P[ipadd6]_r for replies.
            // Axis Supervisor for standalone operation, no PLC - S[ipadd6], which also becomes S[ipadd6]_r for replies.
            // Axis - M[ipadd6][drive #], which also becomes M[ipadd6][drive #]_r for replies.
            // Other systems may need a better approach especially if multiple processes running the API on the same machine use something like a shared file
            // with an incremented number in it as part of the name.  You do not want to call using someone else's mailbox name else the system
            // will think it is caused by a disconnection and then give you control of that mailbox, prior owner will timeout from lack
            // of response.  We could also check to see if the mailbox exists prior to an open if we wanted to...
            //
            // To check for an existing mailbox you can use the bool checkMailboxNameExists(string node, string ourMailboxName) function call.


            // First see if we are running in Standalone mode and only using the EtherCAT Master process where we do all the control
            if (chk_AxisOnly.Checked)
            {
                // Yes, we are in Standalone mode so using the Axis Supervisor since the PLC Logic process is not running.
                try
                {

                    updateStatus("Opening connection to Axis Supervisor...");
                    // Make sure 'our_mailbox_name is unique for each connection/thread.
                    if (axisSupervisor.openConnection(hostNetworkPath + ecatName, "S"+ourSpecialName))  // Name of process, unique name of axis_req mailbox
                    {
                        chk_Monitor.Enabled = false;  // Disable the control so user can not change it while running

                        if ((chk_Monitor.Checked == false) || (chk_EnablePosition.Checked == true))
                        {
                            // Not in monitor mode...  Thus restart network and directly control things...

                            /******** Close any open axis connections first since restarting EtherCAT will break the connection on the other side *************/
                            
                            /******************** Restarting the EtherCAT network *************************/
                            // Controller is stopped so restart the EtherCAT network
                            updateStatus("Restarting the EtherCAT network...");
                            btnIndicator.Text = "Restarting ECAT";
                            btnIndicator.BackColor = Color.Yellow;
                            btnIndicator.Refresh();
                            // Using the axis supervisor restart the EtherCAT network so it is in a known state before we begin.
                            // Restarting does not always have to be done and we could just check for online status if we knew for sure it was stable.
                            if (axisSupervisor.restartEtherCAT(-1) == false)
                            {
                                // failed
                                updateStatus("No Axis found, correct problem...");
                                axisSupervisor.closeConnection();
                                MessageBox.Show("Failed to restart EtherCAT network?  Check for offline devices, correct, and try again...");
                                // Allow user to start again
                                btn_Start.Enabled = true;
                                btn_Start.Refresh();
                                return;
                            }
                            // EtherCAT network is operational now.
                            btnIndicator.Text = "ECAT Operational";
                            btnIndicator.BackColor = Color.Green;
                            btnIndicator.Refresh();

                            /*************************************************************************************/
                        }
                        // Let Windows controls have some cpu time.
                        Application.DoEvents();

                        // Update the resources available (number of axis and IO) since could change once network restarted.
                        axisSupervisor.getResources(ref axisSupervisor.resources);

                        // See how many drives and outputs there are and create a connection for each drive for motion control...
                        numAxis = axisSupervisor.resources.axisnum1;  // Assume just one EtherCAT network at the moment
                        numOutputs = axisSupervisor.resources.douts;

                        // Typically prompt user to fix problem here if none found...
                        if (numAxis == 0)
                        {
                            axisSupervisor.closeConnection();
                            // We may need to restart again, possibly cable was plugged back in again and axis did not register properly, one more time...
                            updateStatus("No Axis found, correct problem...");
                            MessageBox.Show("Failed to start EtherCAT network or no axis found?  Correct problem and then try again...");
                            // Allow user to start again
                            btn_Start.Enabled = true;
                            btn_Start.Refresh();
                            return;
                        }
                        connected_plc = true;   // We are online so set our global status accordingly.
                        // Update the text boxes
                        txt_Axis.Text = "" + numAxis;
                        txt_Outputs.Text = "" + numOutputs;

                        /******************** Connect and launch axis threads *************************/
                        updateStatus("Connect and launch axis threads...");
#if true  // Motion tests
                        for (int i = 0; i != numAxis; i++)
                        {
                            // Create a new axis instance for each drive found since we will run a motion thread for each axis.
                            Controller.Axis testMotion = new Controller.Axis(i + 1);          // We will be using the first axis, starting with index base of 1
                            axisList.Add(testMotion);   // Keep a list so can shutdown later
                            // Read the tick counter.

                            try
                            {
                                // Connect to the EtherCAT process using a unique mailbox base name.
                                if (testMotion.openConnection(hostNetworkPath + ecatName, "M" + ourSpecialName + i))  // Name of process, unique name of Test_req mailbox and Test_resp.
                                {
                                    Thread t2;
                                    if (i == 0)
                                    {
                                        if (chk_EnablePosition.Checked)
                                        {
                                            // Run the Enable Position Test
                                            btn_UpdatePosition.Enabled = true;
                                            t2 = new Thread(doNewPostionThread);
                                        }
                                        else
                                        {
                                            // Do the normal move back and forth test.
                                            t2 = new Thread(doMoveThread);
//                                          t2 = new Thread(doMove_withUpdates_Thread);
                                            
                                        }
                                    }
                                    else
                                    {
                                        // Do the normal move back and forth test.
                                        t2 = new Thread(doMoveThread);
//                                      t2 = new Thread(doMove_withUpdates_Thread);
                                    }
                                    testMotion.userObject1 = t2;  // Save for later use
                                    t2.Name = "Axis_" + i + 1 + "_Thread";
                                    t2.IsBackground = true;
                                    t2.Start(testMotion);         // Begin running the motion thread with axis passed as parameter.
                                    Thread.Sleep(250);            // Sleep a bit so thread starts.
                                }
                            }
                            catch (Controller.Axis.IncentiveAxisException e2)
                            {
                                MessageBox.Show("btn_Start_Click()->Error occurred:  " + e2.ErrMessage);
                                break;
                            }
                            // Give Windows controls a chance to process events.
                            Application.DoEvents();
                        }
#else
                    connected_plc = true;
#endif
                        if (numAxis == 0)
                        {
                            updateStatus("Limited operation, no axis, restart PLC process.");
                        }
                        else
                        {
                            updateStatus("Press Stop button to stop test...");
                        }
                        // Launch running the Axis Supervisor thread to monitor EtherCAT online status
                        Thread t1 = new Thread(doAxisSupervisorThread);
                        t1.Name = "AxisThread";
                        t1.IsBackground = true;
                        t1.Start();
                    }
                    else
                    {
                        MessageBox.Show("Error occurred:  5300PC process probably not running (1)...\r\n");
                    }
                }
                catch (Controller.AxisSupervisor.IncentiveAxisSupervisorException e1)
                {
                    if (e1.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQueAPI)
                    {
                        // It is possible that the queue is not created yet...
                        MessageBox.Show("Wait a bit and try again, it is possible that the EtherCAT runtime has not created the ECATQueAPI yet...");
                    }
                    else
                    {
                        MessageBox.Show("btn_Start_Click()->Error occurred:  " + e1.ErrMessage);
                    }
                }
                btn_Stop.Enabled = true;  // We are running the test so now enable the Stop button.
                btn_Stop.Refresh();
                return;
            }


            // This for when both PLC Logic and EtherCAT process are running.
            try
            {
                 // When opening a connection make sure each name is unique, such as plc1, plc2, etc...  If threading with multiple connections just increment a name and append unique number to it.  
                // The name is limited to 8 characters.  This is because each name is used to create a global mailbox through which communications
                // is done with real time processes running independently on another core of the processor.  Each connection runs unique to itself and may be run with other connections
                // in parallel.  It is best to connect once it is known the PLC runtime is fully operational so that the correct resources and symbols can be read.  This could be done by
                // attempting an initial connection, monitoring a register and when set to a proper value, then invoking get_resources() and load_symbols().  Alternatively the connection
                // can be closed and reopened with the appropriate flags set to automatically retrieve that information.

                updateStatus("Opening connection to PLC logic...");
                // Make sure 'our_mailbox_name is unique for each connection/thread.
                if (testplc.openConnection(hostNetworkPath + plcName, "P" + ourSpecialName, true, true))  // Name of process, unique name of plc_req mailbox, automatically gets resources and any symbols available.
                {
                    // Since the symbols were loaded upon connection we can test accessing a variable by its name.  The symbol must match that of an existing name in a running QuickBuilder (QB)
                    // program.  In the example below the symbol has a name of 'timer'.
                    if (testplc.symbols.Count > 0)
                    {
                        // Symbols are available, show how to read a variable from only the PLCLogic connection
//                        if (readSymbolValue(testplc, null, "timer", ref iValue) == false)
//                        if (readSymbolValue(testplc, null, "nvar_double", 10, 5, ref dValue) == false)
                        {
                            // read failed???
                        }
                    }

                    chk_Monitor.Enabled = false;  // disable the control so user can not change it while running

                    if ((chk_Monitor.Checked == false) || (chk_EnablePosition.Checked == true))
                    {
                        // Not in monitor mode...  Thus restart network and directly control things...

                        /******************** Reset and Stop the real-time Controller *************************/
                        updateStatus("Reset and Stop the real-time Controller...");
#if true
                        testplc.shutdown_QuickBuilder(10000, true);
#endif

                        /******************** Restarting the EtherCAT network *************************/
                        // Controller is stopped so restart the EtherCAT network
                        updateStatus("Restarting the EtherCAT network...");
                        btnIndicator.Text="Restarting ECAT";
                        btnIndicator.BackColor = Color.Yellow;
                        btnIndicator.Refresh();
#if true
                        if (testplc.restartEtherCAT(-1) == false)
                        {
                            // failed
                            updateStatus("No Axis found, correct problem...");
                            testplc.closeConnection();
                            MessageBox.Show("Failed to restart EtherCAT network?  Check for offline devices, correct, and try again...");
                            // Allow user to start again
                            btn_Start.Enabled = true;
                            btn_Start.Refresh();
                            return;
                        }
#endif
                        btnIndicator.Text = "ECAT Operational";
                        btnIndicator.BackColor = Color.Green;
                        btnIndicator.Refresh();

                        /*************************************************************************************/
                    }

                    Application.DoEvents();

                    // Update the resources available since could change once network restarted
                    testplc.getResources(ref testplc.resources);
                    // See how many drives and outputs there are and create a connection for each drive for motion control...
                    numAxis = testplc.resources.axisnum1;  // Assume just one EtherCAT network at the moment
                    numOutputs = testplc.resources.douts;

#if false  // Used only if want to abort when no axis are present
                     // Typically prompt user to fix problem here...
                   if (numAxis == 0)
                    {
                        testplc.closeConnection();
                        // We may need to restart again, possibly cable was plugged back in again and axis did not register properly, one more time...
                        updateStatus("No Axis found, correct problem...");
                        MessageBox.Show("Failed to start EtherCAT network or no axis found?  Correct problem and then try again...");
                        // Allow user to start again
                        btn_Start.Enabled = true;
                        btn_Start.Refresh();
                        return;
                    }
#endif
                    connected_plc = true;
                    // Update the text boxes
                    txt_Axis.Text = "" + numAxis;
                    txt_Outputs.Text = "" + numOutputs;

                    // Lets make sure network is fully up
                    if (!testplc.checkEtherCAT(1))
                    {
                        updateStatus("EtherCAT network is not fully operational yet...");
                        MessageBox.Show("Click OK when EtherCAT Network is operational to continue...");
                    }

                    /******************** Connect and launch axis threads and connections *************************/
                    updateStatus("Connect and launch axis threads...");
                    for (int i = 0; i != numAxis; i++)
                    {
                        // Create a new axis instance for each drive found since we will run a motion thread for each axis.
                        Controller.Axis testMotion = new Controller.Axis(i + 1);          // We will be using the first axis, starting with index base of 1
                        axisList.Add(testMotion);   // Keep a list so can shutdown later
                        // Read the tick counter just for reference to update the text box.
                        if (testplc.getRegister((int)Controller.PLCLogic.REGISTERS.MILLISECOND_COUNTER, ref iValue))
                        {
                            // update the register value
                            txt_register.Text = "" + iValue;

                            try
                            {
                                // Connect to the EtherCAT process using a unique mailbox base name.
                                if (testMotion.openConnection(hostNetworkPath + ecatName, "M" + ourSpecialName + i))  // Name of process, unique name of Test_req mailbox and Test_resp.
                                {
                                    Thread t2;
                                    if (i == 0)
                                    {
                                        if (chk_EnablePosition.Checked)
                                        {
                                            // Run the Enable Position Test
                                            btn_UpdatePosition.Enabled = true;
                                            t2 = new Thread(doNewPostionThread);
                                        }
                                        else
                                        {
                                            // Do normal back and forth move test
                                            t2 = new Thread(doMoveThread);
                                        }
                                    }
                                    else
                                    {
                                        // Do normal back and forth move test
                                        t2 = new Thread(doMoveThread);
                                    }
                                    testMotion.userObject1 = t2;  // Save for later use
                                    t2.Name = "Axis_" + i + 1 + "_Thread";
                                    t2.IsBackground = true;
                                    t2.Start(testMotion);         // Begin running the motion thread with axis passed as parameter.
                                    Thread.Sleep(250);
                                }
                            }
                            catch (Controller.Axis.IncentiveAxisException e2)
                            {
                                MessageBox.Show("btn_Start_Click()->Error occurred:  " + e2.ErrMessage);
                                break;
                            }
                            Application.DoEvents();
                        }
                    }

                    if (numAxis == 0)
                    {
                        updateStatus("Limited operation, no axis found, press stop button to stop test...");
                    }
                    else
                    {
                        updateStatus("Press Stop button to stop test...");
                    }
#if false  // Used to test numerous threads all accessing API via lock(object)
                    for (int i = 0; i != 40; i++)
                    {
                        Thread t1a = new Thread(parallelAccessThread);
                        t1a.IsBackground = true;
                        t1a.Start();
                    }

#endif
                    // Launch running the PLCLogic thread to update register information and monitor EtherCAT online status
                    Thread t1 = new Thread(doOutputsThread);
                    t1.Name = "PLCThread";
                    t1.IsBackground = true;
                    t1.Start();  
                }
                else
                {
                    MessageBox.Show("Error occurred:  5300PC process probably not running (2)...\r\n");
                }
            }
            catch (Controller.PLCLogic.IncentivePLCException e1)
            {
                if (e1.ErrCode == Controller.PLCLogic.PLC_RETVAL.ERROR_PLCQueAPI)
                {
                    // It is possible that the queue is not created yet...
                    MessageBox.Show("Wait a bit and try again, it is possible that the PLC runtime has not created the PLCQueAPI yet...");
                }
                else
                {
                    MessageBox.Show("btn_Start_Click()->Error occurred:  " + e1.ErrMessage);
                }
            }
            btn_Stop.Enabled = true; 
            btn_Start.Refresh();
        }

        /// <summary>
        /// Reads an integer value from the controller symbol specified.
        /// </summary>
        /// <param name="plcConnection">The PLC connection.</param>
        /// <param name="axisConnection">The axis connection.</param>
        /// <param name="s">The symbol name to read from.</param>
        /// <param name="value">Where to store the integer value result after it is read.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool readSymbolValue(Controller.PLCLogic plcConnection, Controller.Axis axisConnection, string s, ref int value)
        {
            // First see if symbol exists
            if (plcConnection == null)
            {
                // Was not provided so fail
                return false;
            }
            int index = plcConnection.symbols.BinarySearch(s, new Controller.PLCLogic.SimpleStringComparer());
            if (index >= 0)
            {
                Controller.Symbol sym = plcConnection.symbols[index] as Controller.Symbol;
                if (sym.IsAxisRegister)
                {
                    // This is an MSB user variable, by default this is a double...
                    if (axisConnection == null)
                    {
                        // Was not provided so fail
                        return false;
                    }
                    Controller.Axis.MOVE_PARAM v = new Controller.Axis.MOVE_PARAM();
                    v.arg_cnt = 1;
                    v.axisnum = sym.RegisterOrObjectNumber;
                    v.cmd = Controller.Axis.MOVE_CMD.READ_VAR;
                    v.var_index = sym.PropertyNumber;  // from avartable.h
                    v.var_type = Variant.VARIANT_INTEGER;
                    Variant.VARIANT_STORAGE var = new Variant.VARIANT_STORAGE();
                    if (axisConnection.getVar(v, ref var))
                    {
                        value = var.iValue;
                        return true;
                    }
                    return false;
                }
                else
                {
                    plcConnection.getRegister(sym.RegisterOrObjectNumber, ref value);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads a double value from the controller symbol specified.
        /// </summary>
        /// <param name="plcConnection">The PLC connection.</param>
        /// <param name="axisConnection">The axis connection.</param>
        /// <param name="s">The symbol name to read from.</param>
        /// <param name="value">Where to store the double value result after it is read.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool readSymbolValue(Controller.PLCLogic plcConnection, Controller.Axis axisConnection, string s, int row, int col, ref double value)
        {
            // First see if symbol exists
            if (plcConnection == null)
            {
                // Was not provided so fail
                return false;
            }
            int index = plcConnection.symbols.BinarySearch(s, new Controller.PLCLogic.SimpleStringComparer());
            if (index >= 0)
            {
                Controller.Symbol sym = plcConnection.symbols[index] as Controller.Symbol;
                if (sym.IsAxisRegister)
                {
                    // This is an MSB user variable, by default this is a double...
                    if (axisConnection == null)
                    {
                        // Was not provided so fail
                        return false;
                    }
                    Controller.Axis.MOVE_PARAM v = new Controller.Axis.MOVE_PARAM();
                    v.arg_cnt = 1;
                    v.axisnum = sym.RegisterOrObjectNumber;
                    v.cmd = Controller.Axis.MOVE_CMD.READ_VAR;
                    v.var_index = sym.PropertyNumber;  
                    v.var_type = Variant.VARIANT_DOUBLE;
                    Variant.VARIANT_STORAGE var = new Variant.VARIANT_STORAGE();
                    if (axisConnection.getVar(v, ref var))
                    {
                        value = var.dValue;
                        return true;
                    }
                    return false;
                }
                else
                {
                    // use 6 as the default precision
                    plcConnection.getRegister(sym.RegisterOrObjectNumber, row, col, 6, ref value);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes a double value to the controller symbol specified.
        /// </summary>
        /// <param name="plcConnection">The PLC connection.</param>
        /// <param name="axisConnection">The axis connection.</param>
        /// <param name="s">The symbol name to write to.</param>
        /// <param name="value">The desired double value.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool writeSymbolValue(Controller.PLCLogic plcConnection, Controller.Axis axisConnection, string s, double value)
        {
            // First see if symbol exists
            if (plcConnection == null)
            {
                // Was not provided so fail
                return false;
            }
            int index = plcConnection.symbols.BinarySearch(s, new Controller.PLCLogic.SimpleStringComparer());
            if (index >= 0)
            {
                Controller.Symbol sym = plcConnection.symbols[index] as Controller.Symbol;
                if (sym.IsAxisRegister)
                {
                    // This is an MSB user variable, by default this is a double...
                    if (axisConnection == null)
                    {
                        // Was not provided so fail
                        return false;
                    }
                    Controller.Axis.MOVE_PARAM v = new Controller.Axis.MOVE_PARAM();
                    v.arg_cnt = 1;
                    v.axisnum = sym.RegisterOrObjectNumber;
                    v.cmd = Controller.Axis.MOVE_CMD.WRITE_VAR;
                    v.var_index = sym.PropertyNumber;  
                    v.var_type = Variant.VARIANT_DOUBLE;
                    v.d_args1 = value;
                    if (axisConnection.setVar(v))
                    {                        
                        return true;
                    }
                }
                else
                {
                    if (plcConnection.putRegister(sym.RegisterOrObjectNumber, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Writes an integer value to the controller symbol specified.
        /// </summary>
        /// <param name="plcConnection">The PLC connection.</param>
        /// <param name="axisConnection">The axis connection.</param>
        /// <param name="s">The symbol name to write to.</param>
        /// <param name="value">The desired integer value.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool writeSymbolValue(Controller.PLCLogic plcConnection, Controller.Axis axisConnection, string s, int value)
        {
            // First see if symbol exists
            if (plcConnection == null)
            {
                // Was not provided so fail
                return false;
            }
            int index = plcConnection.symbols.BinarySearch(s, new Controller.PLCLogic.SimpleStringComparer());
            if (index >= 0)
            {
                Controller.Symbol sym = plcConnection.symbols[index] as Controller.Symbol;
                if (sym.IsAxisRegister)
                {
                    // This is an MSB user variable, by default this is a double...
                    if (axisConnection == null)
                    {
                        // Was not provided so fail
                        return false;
                    }
                    Controller.Axis.MOVE_PARAM v = new Controller.Axis.MOVE_PARAM();
                    v.arg_cnt = 1;
                    v.axisnum = sym.RegisterOrObjectNumber;
                    v.cmd = Controller.Axis.MOVE_CMD.WRITE_VAR;
                    v.var_index = sym.PropertyNumber;  
                    v.var_type = Variant.VARIANT_INTEGER;
                    v.i_args1 = value;
                    if (axisConnection.setVar(v))
                    {                        
                        return true;
                    }
                }
                else
                {
                    if (plcConnection.putRegister(sym.RegisterOrObjectNumber, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Set any drive specific properties.
        /// </summary>
        /// <param name="axis">Incentive axis class for drive access.</param>
        /// <returns>Nothing.</returns>
        public bool set_driveSpecifics(ref Controller.Axis axis)
        {
            axis.inposw = .001;   // Set default in position window
            axis.cmode = Controller.Axis.ETHERCAT_MODES.CYCLIC_SYNC_POSITION_MODE;
            // Determine drive type and set any drive specific properties
            switch (axis.driveType)
            {
                case Controller.Axis.DRIVE_TYPES.DRIVE_ABB_MICROFLEX:
                    axis.ppr = 524288;
                    axis.mppr = 524288;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_AMC:
                    axis.ppr = 12000;
                    axis.mppr = 12000;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_COPLEY:
                case Controller.Axis.DRIVE_TYPES.DRIVE_ELMO:
                    axis.ppr = 8000;
                    axis.mppr = 8000;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_EMERSON:
                    axis.ppr = 65536;
                    axis.mppr = 65536;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_IAI_ACON_MODE0:
                    axis.inposw = .01;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_IAI_ACON_MODE3:
                    axis.inposw = .01;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_KOLLMORGEN:
                    axis.ppr = 1048576;
                    axis.mppr = 1048576;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_LINMOT:
                    axis.ppr = 10000;
                    axis.mppr = 10000;
                    axis.inposw = .15;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_MITSUBISHI:
                    axis.ppr = 4194304;
                    axis.mppr = 4194304;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_SANYO_DENKI:
                    axis.ppr = 131072;
                    axis.mppr = 131072;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_YASKAWA:
                    axis.ppr = 1048576;
                    axis.mppr = 1048576;
                    break;
                case Controller.Axis.DRIVE_TYPES.DRIVE_VIRTUAL:
                    axis.ppr = 65536;
                    axis.mppr = 65536;
                    break;
            }
            return true;
        }
        /// <summary>
        /// Thread to do motion on each axis.
        /// </summary>
        /// <param name="data">Incentive axis class for drive access.</param>
        /// <returns>Nothing.</returns>
        public void doMoveThread(object data)
        {
            bool keep_running = true;
            Controller.Axis.SDO_READ_RESULTS results = new Controller.Axis.SDO_READ_RESULTS();

            Controller.Axis axis = (Controller.Axis)data;  // Cast the passed parameter to the proper type for use.

            while (keep_running)
            {
                try
                {

                    if (abortThread)
                    {
                        // Request to abort the thread
                        keep_running = false;
                        continue;
                    }
                    if (chk_Monitor.Checked == false) 
                    {
                        // Just in case we disconnected and re-connected let's disable the drive so don't get a fault with the dc_sync
                        axis.drive_disable();

                        // Set the proper PPR
                        set_driveSpecifics(ref axis);

                        // Set default deceleration, acceleration and maximum velocity
                        axis.dec = 100;
                        axis.acc = 100;
                        axis.vmax = 100;

                        // Attempt dc sync after a bit of a delay
                        Thread.Sleep(1000);
                        axis.dc_sync(-1, 1000000, 0, 0, 100000000);
                        Thread.Sleep(200);

                        // Enable the drive, return once enabled...
                        axis.drive_enable();
                        Thread.Sleep(1000);

                        // See if homing option is selected
                        if (chk_Home.Checked)
                        {
                            // Some drives like Yaskawa and Mitsubishi support Homing Mode
                            // Enter Homing mode
                            axis.cmode = Controller.Axis.ETHERCAT_MODES.HOMING_MODE;
                            axis.homing_method = 34;
                            axis.homing_speed1 = 1;
                            axis.homing_speed2 = 1;
                            if (axis.driveType == Controller.Axis.DRIVE_TYPES.DRIVE_MITSUBISHI)
                            {
                                // Mitsubishi acceleration time constant is passed with acceleration/deceleration for homing
                                axis.move_to(0, 0, 0);  // home
                            }
                            else
                            {
                                // Other drives allow you to specify it
                                axis.move_to(0, 100, 100);  // home
                            }
                            axis.Command_timout_ms = 60000;  // Set long timeout
                            axis.wait_for_in_pos(-1);
                            axis.Command_timout_ms = 10000;
                            // Restore CSP mode
                            axis.cmode = Controller.Axis.ETHERCAT_MODES.CYCLIC_SYNC_POSITION_MODE;
                        }
                        if (axis.driveType == Controller.Axis.DRIVE_TYPES.DRIVE_MITSUBISHI)
                        {
                            try
                            {
                                // If this is Mitsubishi read PA09, Auto Tuning Response, it is probably around 16.
                                if (axis.sdo_read(-1, 0x2009, 0, Controller.Axis.ECAT_OBJECT_SIZE.DINT, ref results))
                                {
                                    if (results.result == Controller.Axis.MOTION_FAULTS.MF_NO_ERROR)
                                    {
                                        // If this is Mitsubishi write PA09, Auto Tuning Response, set it to 17 just as an example
                                        int foo = 17;
                                        axis.sdo_write(-1, 0x2009, 0, Controller.Axis.ECAT_OBJECT_SIZE.DINT, foo);
                                    }
                                }
                            }
                            catch (Controller.Axis.IncentiveAxisException)
                            {
                                MessageBox.Show("doMoveThread()->ERROR: Axis " + (axis.axis + 1) + ",  SDO read or write to Mitsubishi PA09 failed.\r\n");
                            }
                        }

                        while (true)
                        {
                            if (abortThread)
                            {
                                // Request to abort the thread
                                break;
                            }
                            axis.move_at_for(10.0, 30.0);   // Move at 10 rev/sec for 30 revolutions
                            if (axis.axisnum == 1)
                            {
                                while (axis.inpos != 1)
                                {
                                    if (abortThread)
                                    {
                                        // Request to abort the thread
                                        break;
                                    }
                                    UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                                    UpdateVEL_TextBox("" + axis.vel.ToString("N4"));
                                    Thread.Sleep(100);
                                }
                                UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                                UpdateVEL_TextBox("" + axis.vel.ToString("N4"));
                            }
                            else
                            {
                                axis.wait_for_in_pos(-1);       // Wait forever
                            }
                            if (abortThread)
                            {
                                // Request to abort the thread
                                break;
                            }
                            Thread.Sleep(500);              // Pause 500 milliseconds
                            if (abortThread)
                            {
                                // Request to abort the thread
                                break;
                            }


                            axis.move_at_for(5.0, -30.0);   // Move at 5 rev/sec for -30 revolutions

                            if (axis.axisnum == 1)
                            {
                                // Monitor for in position flag, does the same thing as Wait for inpos except is not checking for offline state...
                                while (axis.inpos != 1)
                                {
                                    if (abortThread)
                                    {
                                        // Request to abort the thread
                                        break;
                                    }
                                    // loop updating the display
                                    UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                                    UpdateVEL_TextBox("" + axis.vel.ToString("N4"));
                                    Thread.Sleep(100);
                                }
                                UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                            }
                            else
                            {
                                axis.wait_for_in_pos(-1);       // Wait forever
                            }
                            if (abortThread)
                            {
                                // Request to abort the thread
                                break;
                            }
                            Thread.Sleep(500);               // Pause 500 milliseconds
                        }
                    }
                    else
                    {
                        // Monitor mode only
                        if (axis.axisnum == 1)
                        {
                            UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                            UpdateVEL_TextBox("" + axis.vel.ToString("N4"));
                        }
                        Thread.Sleep(100);               // Pause 500 milliseconds
                    }
                }
                catch (Controller.Axis.IncentiveAxisException e2)
                {
                    if (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECAT_OFFLINE)
                    {
                        // MF_ECAT_OFFLINE will occur if the network goes down, faults, etc...  This may be recoverable since in most cases the existing
                        // connections are still valid.  One thing to try is to first restart the network once the error is cleared.  This should be coordinated
                        // with any QuickBuilder program that may be running as well.  It is also possible the network was restarted by the user...

                        // Lets assume the user restarted the network so let's monitor for the network to come back up...
                        ecat_online_status = 0;  // Update offline status until other thread updates it again...
                        MessageBox.Show("doMoveThread()->ERROR: Axis " + (axis.axis + 1) + ",  EtherCAT network went offline, check network and either correct or abort the application program.\r\n");
                        // Make sure background thread updates the online status first so we know up to date...
                        Thread.Sleep(500);
                        
                        // Now wait for it to go back online
                        while (ecat_online_status != 1)
                        {
                            if (abortThread)
                            {
                                // Request to abort the thread
                                return;
                            }

                            Thread.Sleep(200);
                        }
                        // By default we will restart at the top of the logic while statement...
                        continue;
                    }
                    else if ((e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQue_REQUEST) || (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQue_RESPONSE) || (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_TIMEOUT))
                    {
                        // Our message queue was closed by the EtherCAT process.  Could be QuickBuilder reloading a program or EtherCAT network restart.
                        axis.connected = false;
                        MessageBox.Show("doMoveThread()->Error occurred:  Axis " + (axis.axis + 1) + ", lost communications with EtherCAT process, possible network restarting or QuickBuilder program being downloaded...\r\n\nWhen ready select the STOP TEST button and then START TEST to automatically restart.");
                        keep_running = false;
                        // Could recover by monitoring online status and then openning the axis connection again...
                        continue;
                    }
                    else
                    {
                        // Error processing, 
                        MessageBox.Show("doMoveThread()->Error occurred:  " + e2.ErrMessage + "\r\nPlease restart the application...\r\n");
                        keep_running = false;  // Abort and shut things down...
                        continue;
                    }
                }
                // Exiting the thread
                try
                {
                    if (abortThread)
                    {
                        if (chk_Monitor.Checked == false)
                        {
                            // Request to abort the thread was made so let's make sure drive is stopped and then disabled first
                            axis.stop();
                            axis.drive_disable();
                        }
                        break;
                    }
                }
                catch(Exception)
                {

                }
            }

        }

        /// <summary>
        /// Thread to do constant motion on each axis updating a new position every N milliseconds.
        /// </summary>
        /// <param name="data">Incentive axis class for drive access.</param>
        /// <returns>Nothing.</returns>
        public void doMove_withUpdates_Thread(object data)
        {
            bool keep_running = true;
            Controller.Axis axis = (Controller.Axis)data;
            Controller.Axis.COMMAND_EXT_RESULTS drive_information = new Controller.Axis.COMMAND_EXT_RESULTS();
            while (keep_running)
            {
                try
                {
                    if (abortThread)
                    {
                        // Request to abort the thread
                        keep_running = false;
                        continue;
                    }
                    if (chk_Monitor.Checked == false)
                    {
                        // Just in case we disconnected and re-connected let's disable the drive so don't get a fault with the dc_sync
                        axis.drive_disable();

                        // Set the proper PPR
                        set_driveSpecifics(ref axis);

                        // Set default deceleration, acceleration and maximum velocity
                        axis.dec = 800;
                        axis.acc = 800;
                        axis.vmax = 80;

                        // Attempt dc sync after a bit of a delay
                        Thread.Sleep(1000);
                        axis.dc_sync(-1, 1000000, 0, 0, 100000000);
                        Thread.Sleep(200);

                        // Enable the drive, return once enabled...
                        axis.drive_enable();
                        Thread.Sleep(1000);

                        // See if homing option is selected
                        if (chk_Home.Checked)
                        {
                            // Some drives like Yaskawa and Mitsubishi support Homing Mode
                            // Enter Homing mode
                            axis.cmode = Controller.Axis.ETHERCAT_MODES.HOMING_MODE;
                            axis.homing_method = 34;
                            axis.homing_speed1 = 1;
                            axis.homing_speed2 = 1;
                            if (axis.driveType == Controller.Axis.DRIVE_TYPES.DRIVE_MITSUBISHI)
                            {
                                // Mitsubishi acceleration time constant is passed with acceleration/deceleration for homing
                                axis.move_to(0, 0, 0);  // home
                            }
                            else
                            {
                                // Other drives allow you to specify it
                                axis.move_to(0, 100, 100);  // home
                            }
                            axis.Command_timout_ms = 60000;  // Set long timeout
                            axis.wait_for_in_pos(-1);
                            axis.Command_timout_ms = 10000;
                            // Restore CSP mode
                            axis.cmode = Controller.Axis.ETHERCAT_MODES.CYCLIC_SYNC_POSITION_MODE;
                        }
                        double adjustment = .4;
                        Boolean toggle = true;
                        axis.inposw = .1;
                        while (true)
                        {
                            if (abortThread)
                            {
                                // Request to abort the thread
                                break;
                            }
                            // Show example of using extended command results
                            // We are moving so take decel ramp out of equation for calculating endpoint, instantaneous deceleration
                            while (true)
                            {
                                if (axis.move_at_to(80, adjustment, 800, 800, ref drive_information))  // Separate Accel and Decel
                                {
                                    if (drive_information.result == Controller.Axis.MOTION_FAULTS.MF_NO_ERROR)
                                    {
                                        if (axis.axisnum == 1)
                                        {
                                            // Drive information is valid at point command began executing...
                                            UpdateFPOS_TextBox("" + drive_information.fpos.ToString("N4"));
                                            UpdateVEL_TextBox("" + drive_information.vel.ToString("N4"));
                                        }
                                    }
                                    if (drive_information.cmd_results == 0)
                                    {
                                        break;  // executed correctly
                                    }
                                    Thread.Sleep(1);  // Try again in a bit
                                }
                                else
                                {
                                    break;  // error?
                                }
                            }
                            if (toggle)
                            {
                                toggle = false;
                                adjustment = .3;
                            }
                            else
                            {
                                toggle = true;
                                adjustment = .4;
                            }
                            if (drive_information.fpos <= 0.0)
                            {
                            }
                            else if (drive_information.fpos >= 1000.0)
                            {
                            }
                            Thread.Sleep(5);
                        }
                    }
                    else
                    {
                        // Monitor mode only
                        if (axis.axisnum == 1)
                        {
                            UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                            UpdateVEL_TextBox("" + axis.vel.ToString("N4"));
                        }
                        Thread.Sleep(100);               // Pause 500 milliseconds
                    }
                }
                catch (Controller.Axis.IncentiveAxisException e2)
                {
                    if (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECAT_OFFLINE)
                    {
                        // MF_ECAT_OFFLINE will occur if the network goes down, faults, etc...  This may be recoverable since in most cases the existing
                        // connections are still valid.  One thing to try is to first restart the network once the error is cleared.  This should be coordinated
                        // with any QuickBuilder program that may be running as well.  It is also possible the network was restarted by the user...

                        // Lets assume the user restarted the network so let's monitor for the network to come back up...
                        ecat_online_status = 0;  // Update offline status until other thread updates it again...
                        MessageBox.Show("doMoveThread()->ERROR: Axis " + (axis.axis + 1) + ",  EtherCAT network went offline, check network and either correct or abort the application program.\r\n");
                        // Make sure background thread updates the online status first so we know up to date...
                        Thread.Sleep(500);

                        // Now wait for it to go back online
                        while (ecat_online_status != 1)
                        {
                            if (abortThread)
                            {
                                // Request to abort the thread
                                return;
                            }

                            Thread.Sleep(200);
                        }
                        // By default we will restart at the top of the logic while statement...
                        continue;
                    }
                    else if ((e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQue_REQUEST) || (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQue_RESPONSE))
                    {
                        // Our message queue was closed by the EtherCAT process.  Could be QuickBuilder reloading a program or EtherCAT network restart.
                        axis.connected = false;
                        MessageBox.Show("doMoveThread()->Error occurred:  Axis " + (axis.axis + 1) + ", lost communications with EtherCAT process, possible network restarting or QuickBuilder program being downloaded...\r\nWhen ready restart the program or monitor network status and automatically restart.");
                        keep_running = false;
                        // Could recover by monitoring online status and then openning the axis connection again...
                        continue;
                    }
                    else
                    {
                        // Error processing, 
                        MessageBox.Show("doMoveThread()->Error occurred:  " + e2.ErrMessage + "\r\nPlease restart the application...\r\n");
                        keep_running = false;  // Abort and shut things down...
                        continue;
                    }
                }
                // Exiting the thread
                try
                {
                    if (abortThread)
                    {
                        if (chk_Monitor.Checked == false)
                        {
                            // Request to abort the thread was made so let's make sure drive is stopped and then disabled first
                            axis.stop();
                            axis.drive_disable();
                        }
                        break;
                    }
                }
                catch (Exception)
                {

                }
            }

        }

        /// <summary>
        /// Thread to do New Motion on axis one, only.
        /// </summary>
        /// <param name="data">Incentive axis class for drive access.</param>
        /// <returns>Nothing.</returns>
        public void doNewPostionThread(object data)
        {
            bool keep_running = true;
            Controller.Axis axis = (Controller.Axis)data;
            while (keep_running)
            {
                try
                {
                    if (abortThread)
                    {
                        // Request to abort the thread
                        keep_running = false;
                        continue;
                    }
                    // Just in case we disconnected and re-connected let's disable the drive so don't get a fault with the dc_sync
                    axis.drive_disable();

                    // Set the proper PPR
                    set_driveSpecifics(ref axis);

                    // Set default deceleration, acceleration and maximum velocity
                    axis.dec = 100;
                    axis.acc = 100;
                    axis.vmax = 90;

                    // Attempt dc sync after a bit of a delay
                    Thread.Sleep(1000);
                    axis.dc_sync(-1, 1000000, 0, 0, 100000000);
                    Thread.Sleep(200);

                    // Enable the drive, return once enabled...
                    axis.drive_enable();
                    Thread.Sleep(1000);

                    // See if homing option is selected
                    if (chk_Home.Checked)
                    {
                        // Some drives like Yaskawa and Mitsubishi support Homing Mode
                        // Enter Homing mode
                        axis.cmode = Controller.Axis.ETHERCAT_MODES.HOMING_MODE;
                        axis.homing_method = 34;
                        axis.homing_speed1 = 1;
                        axis.homing_speed2 = 1;
                        if (axis.driveType == Controller.Axis.DRIVE_TYPES.DRIVE_MITSUBISHI)
                        {
                            // Mitsubishi acceleration time constant is passed with acceleration/deceleration for homing
                            axis.move_to(0, 0, 0);  // home
                        }
                        else
                        {
                            // Other drives allow you to specify it
                            axis.move_to(0, 100, 100);  // home
                        }
                        axis.Command_timout_ms = 60000;  // Set long timeout
                        axis.wait_for_in_pos(-1);
                        axis.Command_timout_ms = 10000;
                        // Restore CSP mode
                        axis.cmode = Controller.Axis.ETHERCAT_MODES.CYCLIC_SYNC_POSITION_MODE;
                    }

                    // Button will be pressed when the values are changed so we can update things.
                    // Typically this would be an event.

                    while (true)
                    {
                        if (abortThread)
                        {
                           // Request to abort the thread was made so let's make sure drive is stopped and then disabled first
                           axis.stop();
                           axis.drive_disable();
                           break;
                        }
                        if (axis.axisnum == 1)
                        {
                            UpdateFPOS_TextBox("" + axis.fpos.ToString("N4"));
                            UpdateVEL_TextBox("" + axis.vel.ToString("N4"));

                            if (changeOccurred)
                            {
                                // Lets go process the change request
                                try
                                {

                                    changeOccurred = false;
                                    Maxspeed = Convert.ToDouble(txt_MaxSpeed.Text);
                                    Accel = Convert.ToDouble(txt_Accel.Text);
                                    Decel = Convert.ToDouble(txt_Decel.Text);
                                    Position = Convert.ToDouble(txt_Position.Text);
                                    if ((Maxspeed != 0) && (Accel != 0) && (Decel != 0))
                                    {
                                        // Attempt change
                                    }
                                    else
                                    {
                                        MessageBox.Show("Bad data entry1, please correct and try again.\r\n");
                                        Thread.Sleep(100);
                                        continue;

                                    }
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("Bad data entry2, please correct and try again.\r\n");
                                    Thread.Sleep(100);
                                    continue;
                                }
                            }
                            else
                            {
                                Thread.Sleep(100);
                                continue;
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                            continue;
                        }
#if false
                        // If using new_endposition from speed of 0 we need to set newvel as it will
                        // automatically get converted to a trapazoidal move if set, otherwise have
                        // to check for (axis.pstate == Controller.Axis.PSTATES.COMPLETE)
                        // and use a move_at_for or move_at_to command.  Additional variable axis.settling can
                        // optionally be set to cause a delay when changing directions.  Sometimes this is needed
                        // when moving a large mass to avoid drive faulting from over-torque caused by inertia.
                        // By default when change directions we wait for the drive to be in position (inpos) with
                        // regards to position error and the inposw.  If axis.settling is not set then we immediately
                        // calculate a trapazoidal move to the final destination and continue the new position move.

                        //axis.settling = .25;   // Allow 1/4 second for settling when change directions
                        axis.newvel = (float)Maxspeed;  // If newvel is not used you can set it to 0 but then first move must 
                                                        // be move_at_to or move_at_for to get things going
                        axis.new_endposition(Position, Accel);  // No way to specify Accel is rate for Decel as well
#else
                        // Show example of using extended command results as well as updating new position on the fly
                        Controller.Axis.COMMAND_EXT_RESULTS drive_information = new Controller.Axis.COMMAND_EXT_RESULTS();
                        if (axis.move_at_to((float)Maxspeed, Position, Accel, Decel, ref drive_information))  // Separate Accel and Decel
                        {
                            if (drive_information.result == Controller.Axis.MOTION_FAULTS.MF_NO_ERROR)
                            {
                                // Drive information is valid at point command began executing...
                                UpdateFPOS_TextBox("" + drive_information.fpos.ToString("N4"));
                                UpdateVEL_TextBox("" + drive_information.vel.ToString("N4"));
                            }
                        }
#endif
                    }
                }
                catch (Controller.Axis.IncentiveAxisException e2)
                {
                    if (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECAT_OFFLINE)
                    {
                        // MF_ECAT_OFFLINE will occur if the network goes down, faults, etc...  This may be recoverable since in most cases the existing
                        // connections are still valid.  One thing to try is to first restart the network once the error is cleared.  This should be coordinated
                        // with any QuickBuilder program that may be running as well.  It is also possible the network was restarted by the user...

                        // Lets assume the user restarted the network so let's monitor for the network to come back up...
                        ecat_online_status = 0;  // Update offline status until other thread updates it again...
                        MessageBox.Show("doNewPostionThread()->ERROR: Axis " + (axis.axis + 1) + ",  EtherCAT network went offline, check network and either correct or abort the application program.\r\n");
                        // Make sure background thread updates the online status first so we know up to date...
                        Thread.Sleep(500);

                        // Now wait for it to go back online
                        while (ecat_online_status != 1)
                        {
                            if (abortThread)
                            {
                                // Request to abort the thread
                                return;
                            }

                            Thread.Sleep(200);
                        }
                        // By default we will restart at the top of the logic while statement...
                        continue;
                    }
                    else if ((e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQue_REQUEST) || (e2.ErrCode == Controller.Axis.MOTION_FAULTS.MF_ECATQue_RESPONSE))
                    {
                        // Our message queue was closed by the EtherCAT process.  Could be QuickBuilder reloading a program or EtherCAT network restart.
                        axis.connected = false;
                        MessageBox.Show("doNewPostionThread()->Error occurred:  Axis " + (axis.axis + 1) + ", lost communications with EtherCAT process, possible network restarting or QuickBuilder program being downloaded...\r\nWhen ready restart the program or monitor network status and automatically restart.");
                        keep_running = false;
                        // Could recover by monitoring online status and then openning the axis connection again...
                        continue;
                    }
                    else
                    {
                        // Error processing, 
                        MessageBox.Show("doNewPostionThread()->Error occurred:  " + e2.ErrMessage + "\r\nPlease restart the application...\r\n");
                        keep_running = false;  // Abort and shut things down...
                        continue;
                    }
                }
                // Exiting the thread
                try
                {
                    if (abortThread)
                    {
                        if (chk_Monitor.Checked == false)
                        {
                            // Request to abort the thread was made so let's make sure drive is stopped and then disabled first
                            axis.stop();
                            axis.drive_disable();
                        }
                        break;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        // Updating text boxes from a background, non UI thread requires the use of Invoke
        delegate void UpdateTextBoxDelegate(string text);

        /// <summary>
        /// Updates the txt_register text box.
        /// </summary>
        /// <param name="text">Text string to write.</param>
        private void UpdateRegister_TextBox(string text)
        {
            if (txt_register.InvokeRequired)
            {
                try
                {
                    txt_register.Invoke(new UpdateTextBoxDelegate(this.UpdateRegister_TextBox), new object[] { text });
                }
                catch(Exception)
                {
                    return;
                }
            }
            else
            {
                // Update the text
                txt_register.Text =  text;
                // Refresh the object
                txt_register.Refresh();
            }
        }
        private void UpdateOutputState_TextBox(string text)
        {
            if (txt_register.InvokeRequired)
            {
                try
                {
                    txt_output.Invoke(new UpdateTextBoxDelegate(this.UpdateOutputState_TextBox), new object[] { text });
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                // Update the text
                txt_output.Text = text;
                // Refresh the object
                txt_output.Refresh();
            }
        }
        private void UpdateInputState_TextBox(string text)
        {
            if (txt_register.InvokeRequired)
            {
                try
                {
                    txt_input.Invoke(new UpdateTextBoxDelegate(this.UpdateInputState_TextBox), new object[] { text });
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                // Update the text
                txt_input.Text = text;
                // Refresh the object
                txt_input.Refresh();
            }
        }
        /// <summary>
        /// Updates the txt_fpos text box.
        /// </summary>
        /// <param name="text">Text string to write.</param>
        private void UpdateFPOS_TextBox(string text)
        {
            if (txt_fpos.InvokeRequired)
            {
                try
                {
                    txt_fpos.Invoke(new UpdateTextBoxDelegate(this.UpdateFPOS_TextBox), new object[] { text });
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                txt_fpos.Text = text;
                txt_fpos.Refresh();
                // Now update the progress bar
                double fpos = Convert.ToDouble(text);
                fpos = fpos * 3.0;  // Graph goes from 0 to 90
                if (fpos < 0)
                { 
                    fpos = 0; 
                }
                if (fpos > 90.0)
                {
                    fpos = 90.0;
                }
                progressBar_fpos.Value = Convert.ToInt32(fpos);
                progressBar_fpos.Refresh();
            }
        }

        /// <summary>
        /// Updates the txt_vel text box.
        /// </summary>
        /// <param name="text">Text string to write.</param>
        private void UpdateVEL_TextBox(string text)
        {
            if (txt_vel.InvokeRequired)
            {
                try
                {
                    txt_vel.Invoke(new UpdateTextBoxDelegate(this.UpdateVEL_TextBox), new object[] { text });
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                txt_vel.Text = text;
                txt_vel.Refresh();
            }
        }

        /// <summary>
        /// Thread to poll Axis Supervisor state, not really needed but using to set general EtherCAT scanning status.
        /// </summary>
        public void doAxisSupervisorThread()
        {
            int loopcounter = 0;
            int sleepcounter = 0;

            while (connected_plc)
            {
                if (abortThread)
                {
                    // Request to abort the thread
                    break;
                }
                if (connected_plc)
                {
                     try
                    {
                        if (loopcounter == 100)
                        {
                            // About once a second...
                            Controller.AxisSupervisor.SCAN_STATES status = axisSupervisor.scanning;
                            if (status == Controller.AxisSupervisor.SCAN_STATES.RUNNING)
                            {
                                ecat_online_status = 1;
                            }
                            else
                            {
                                ecat_online_status = 0;
                            }
                            loopcounter = 0;
                        }
                        else
                        {
                            loopcounter++;
                        }

                        // update the register text box value, since from another thread must use Invoke
                        UpdateRegister_TextBox("" + sleepcounter);

                        if (abortThread)
                        {
                            // Request to abort the thread
                            break;
                        }
                    }
                    catch (Controller.AxisSupervisor.IncentiveAxisSupervisorException e2)
                    {
                        // Error processing, 
                        MessageBox.Show("doAxisSupervisorThread()->Error occurred:  " + e2.ErrMessage);
                    }

                 }
                Thread.Sleep(1);              // Pause 100 milliseconds
                sleepcounter++;
            }
        }

        /// <summary>
        /// Sharing access to a Thread to poll PLC registers, PLC process is running.
        /// </summary>
        object accessMutex = new Object();
        int thrd_counter = 0;
        public void parallelAccessThread()
        {
            int thrd_instance;
            lock (accessMutex)
            {
                thrd_instance = thrd_counter;
                thrd_counter++;
            }

            while (connected_plc)
            {
                Random rnd = new Random();
                int timedelay = rnd.Next(1, 21); // creates a number between 1 and 10
                Thread.Sleep(timedelay);  // random delay
                if (abortThread)
                {
                    // Request to abort the thread
                    break;
                }
                if (connected_plc)
                {
                    try
                    {
                        int iValue=0;
                        // Periodically read the tick counter of the PLC
                        iValue = (int)ReadMemory((uint)Controller.PLCLogic.REGISTERS.MILLISECOND_COUNTER, 0, 0, "Int32");
                        if (abortThread)
                        {
                            // Request to abort the thread
                            break;
                        }

                        // Test volatile variant
                        string sValue;
                        sValue = (string)ReadMemory((uint)36103, 0, 0, "String");
                        sValue = "Test Thread" + thrd_instance;
                        WriteMemory((uint)36103, 0, 0, "String",sValue);

                        timedelay = rnd.Next(1, 21);
                        Thread.Sleep(timedelay);  // random delay

                        // Test non volatile
                        double dValue;
                        dValue = (double)ReadMemory((uint)36701, 5, 0, "Double");
                        dValue = dValue + 1;
                        WriteMemory((uint)36701, 5, 0, "Double", dValue);

                        dValue = thrd_instance;
                        WriteMemory((uint)36701, 5, 1, "Double", dValue);

                    }
                    catch (Controller.PLCLogic.IncentivePLCException e2)
                    {
                        // Error processing, 
                        MessageBox.Show("parallelAccessThread()->Error occurred:  " + e2.ErrMessage);
                    }
                    catch (Exception)
                    {
                        if (testplc == null)
                        {
                            // Caught in shutdown...
                            return;
                        }
                    }
                }
//                Thread.Sleep(100);              // Pause 100 milliseconds
            }
        }
        /// <summary>
        /// Returns data from the specified position in a register holding an array of values
        /// </summary>
        /// <param name="memoryArea"></param>
        /// <param name="address"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="dataType">The type of data to return</param>
        /// <returns></returns>
        public object ReadMemory(UInt32 address, int row, int col, string dataType)
        {
            lock (accessMutex)
            {
//                checkMemoryArea(memoryArea, address);
                const byte precision = 6; //returns the maximum precision of the float and double values
                object value = null;
                try
                {
                    switch (dataType)
                    {
                        case "Single":
                            float fVal = 0;
                            testplc.getRegister((int)address, row, col, precision, ref fVal);
                            value = fVal;
                            break;
                        case "Double":
                            double dVal = 0;
                            testplc.getRegister((int)address, row, col, precision, ref dVal);
                            value = dVal;
                            break;
                        case "Int32":
                            int iVal = 0;
                            testplc.getRegister((int)address, row, col, ref iVal);
                            value = iVal;
                            break;
                        case "String":
                            string sVal = "";
                            testplc.getRegister((int)address, row, col, precision, ref sVal);
                            value = sVal;
                            break;
                    }
                    return value;
                }
                catch (Controller.PLCLogic.IncentivePLCException e2)
                {
                    // Error processing, 
                    MessageBox.Show("ReadMemory1()->Error occurred:  " + e2.ErrMessage + ", " + e2.INtimeReturnCode);
                }
                catch (Exception e)
                {
                    if (testplc == null)
                    {
                        // Caught in shutdown...
                        return value;
                    }
                    MessageBox.Show("ReadMemory2()->Error occurred:  " + e.Message);
                }
                return value;
            }
        }

        /// <summary>
        /// Writes data to the given position in a register holding a table
        /// </summary>
        /// <param name="memoryArea"></param>
        /// <param name="address"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        public void WriteMemory(uint address, int row, int col, string dataType, object data)
        {
            lock (accessMutex)
            {
//                checkMemoryArea(memoryArea, address);
                byte precision = 6;
                try
                {
                    switch (dataType)
                    {
                        case "Single":
                            float fVal = Convert.ToSingle(data);
                            testplc.putRegister((int)address, row, col, precision, fVal);
                            break;
                        case "Double":
                            double dVal = Convert.ToDouble(data);
                            testplc.putRegister((int)address, row, col, precision, dVal);
                            break;
                        case "Int32":
                            int iVal = Convert.ToInt32(data);
                            testplc.putRegister((int)address, row, col, precision, iVal);
                            break;
                        case "String":
                            string sVal = data.ToString();
                            testplc.putRegister((int)address, row, col, precision, sVal);
                            break;
                    }
                }
                catch (Controller.PLCLogic.IncentivePLCException e2)
                {
                    // Error processing, 
                    MessageBox.Show("WriteMemory1()->Error occurred:  " + e2.ErrMessage + ", " + e2.INtimeReturnCode);
                }
                catch (Exception e)
                {
                    if (testplc == null)
                    {
                        // Caught in shutdown...
                        return;
                    }
                    MessageBox.Show("WriteMemory2()->Error occurred:  " + e.Message);
                }
            }
        }



        /// <summary>
        /// Thread to poll PLC registers, PLC process is running.
        /// </summary>
        public void doOutputsThread()
        {
            int outputs = 1;
            int loopcounter = 0;
//            int out_value = 0;
            int outcounter = 0;
            while (connected_plc)
            {
                if (abortThread)
                {
                    // Request to abort the thread
                    break;
                }
                if (connected_plc)
                {
                    int value = 0;
                    lock (accessMutex)  // Mutex the area
                    {
                        try
                        {
                            bool measureTime = false;
                            lock (accessMutex)
                            { 
                                if (loopcounter == 100)
                                {
                                    // About once a second...
                                    testplc.getRegister((int)Controller.PLCLogic.REGISTERS.MASTER_BOARD_ONLINE_STATUS, ref ecat_online_status);  // Update the network status periodically...
                                    loopcounter = 0;
                                    measureTime = true;
                                    outcounter++;
                                }
                                else
                                {
                                    loopcounter++;
                                }
#if false  // Used to test forcing and E_CONTEXT error and reconnection in API
                                if (outcounter == 5)
                                {
                                    // Every 2 seconds output a value and reset link
                                    testplc.force_E_CONTEXT();  // Every 100 loops force error
                                    testplc.putRegister(100, out_value);  // Update the network status periodically...
                                    outcounter = 0;
                                }
                                else
                                {
                                    testplc.putRegister(100, out_value);  // Update the network status periodically...
                                }
                                out_value++;
#endif
                                var watch = System.Diagnostics.Stopwatch.StartNew();
                                // Periodically read the tick counter of the PLC
                                if (testplc.getRegister((int)Controller.PLCLogic.REGISTERS.MILLISECOND_COUNTER, ref value))
                                {
                                    // update the register text box value, since from another thread must use Invoke
                                    UpdateRegister_TextBox("" + value);
                                }
                                // the code that you want to measure comes here
                                watch.Stop();
                                var elapsedMs = (long)0;
                                if (measureTime)
                                {
                                    elapsedMs = watch.ElapsedMilliseconds;
                                    measureTime = false;
                                }
                                else
                                {
                                    elapsedMs = watch.ElapsedMilliseconds;
                                    measureTime = false;
                                }
                            }
                            if (abortThread)
                            {
                                // Request to abort the thread
                                break;
                            }
                            if (chk_Monitor.Checked == false)
                            {
                                // Do a 32 bit shift test cycle
#if false  // Testing digital outputs
                                testplc.output32(1, outputs);   // 32 bit write to first integer block of IO
#endif
                            }
                        }
                        catch (Controller.PLCLogic.IncentivePLCException e2)
                        {
                            // Error processing, 
                            MessageBox.Show("doOutputsThread()->Error occurred:  " + e2.ErrMessage);
                        }
                        catch (Exception)
                        {
                            if (testplc == null)
                            {
                                // Caught in shutdown...
                                return;
                            }
                        }
                    }

                    //MUmeh - Sample code to write to outputs and read inputs using 32-bit register access
                    outputs = outputs << 1;
                    if (testplc.putRegister((int)Controller.PLCLogic.REGISTERS.DIGITAL_OUT_INT_START, outputs))
                    {
                        // update the register text box value, since from another thread must use Invoke
                        UpdateOutputState_TextBox("" + outputs);
                    }
                    Thread.Sleep(20);              // Pause 20 milliseconds
                    if (testplc.getRegister((int)Controller.PLCLogic.REGISTERS.DIGITAL_IN_INT_START, ref value))
                    {
                        // update the register text box value, since from another thread must use Invoke
                        UpdateInputState_TextBox("" + value);
                    }
                    Thread.Sleep(25);          // Pause 250 milliseconds

                    if (outputs == 0)
                    {
                        outputs = 1;
                    }

                }
                Thread.Sleep(10);              // Pause 10 milliseconds
            }
        }

        /// <summary>
        /// Update the status text box from a UI thread so Invoke not needed.
        /// </summary>
        /// <param name="s">String to write to the text box.</param>
        private void updateStatus(string s)
        {
            txt_Status.Clear();
            txt_Status.Text = s;
            txt_Status.Refresh();
        }

        /// <summary>
        /// Handles the FormClosing event of the UI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connected_plc)
            {
                updateStatus("Cleaning up and shutting down threads...");
                // Set we are disconnected
                connected_plc = false;
                // Request threads to shutdown
                abortThread = true;
                // We are exiting so lets close all our connections
                if (!chk_AxisOnly.Checked)
                {
                    try
                    {
                        // Close the PLCLogic connection
                        testplc.closeConnection();
                    }
                    catch (Controller.PLCLogic.IncentivePLCException e2)
                    {
                        // Error occurred?
                        MessageBox.Show("Form1_FormClosing->" + e2.ToString());
                    }
                }
                else
                {
                    try
                    {
                        // Close the PLCLogic connection
                        axisSupervisor.closeConnection();
                    }
                    catch (Controller.AxisSupervisor.IncentiveAxisSupervisorException e2)
                    {
                        // Error occurred?
                        MessageBox.Show("btn_Stop_Click->" + e2.ToString());
                    }
                }
                // Now walk through the array list of axis that are running threads and wait for them to finish shutting down.
                foreach (Controller.Axis axis in axisList)
                {
                    try
                    {
                        while (true)
                        {
                            // If axis thread is still alive then must wait...
                            if (((Thread)axis.userObject1).IsAlive)
                            {
                                Thread.Sleep(50);
                                // All UI messages
                                Application.DoEvents();
                            }
                            else
                            {
                                // It is OK to close the axis connection since the thread is not active anymore.
                                axis.closeConnection();
                                break;
                            }
                        }
                    }
                    catch (Controller.Axis.IncentiveAxisException e2)
                    {
                        // Error occurred?
                        MessageBox.Show("Form1_FormClosing->"+e2.ToString());
                    }
                    catch (System.NullReferenceException)
                    {
                        // userObject1 is probably null?
                        axis.closeConnection();
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects from Incentive application.
        /// </summary>
        private void disconnectFromIncentiveApplication()
        {
            // Disable the stop and start button so no double clicks while processing here
            btn_Stop.Enabled = false;
            btn_Start.Enabled = false;
            btn_Stop.Refresh();
            btn_Start.Refresh();
            if (connected_plc)
            {
                updateStatus("Cleaning up and shutting down threads...");
                btn_UpdatePosition.Enabled = false;
                // Request threads to shutdown
                abortThread = true;
                Thread.Sleep(500);  // let output thread exit...
                // We are exiting so lets close all our connections
                if (!chk_AxisOnly.Checked)
                {
                    try
                    {
                        // Close the PLCLogic connection
                        testplc.closeConnection();
                    }
                    catch (Controller.PLCLogic.IncentivePLCException e2)
                    {
                        // Error occurred?
                        MessageBox.Show("btn_Stop_Click->" + e2.ToString());
                    }
                }
                else
                {
                    try
                    {
                        // Close the PLCLogic connection
                        axisSupervisor.closeConnection();
                    }
                    catch (Controller.AxisSupervisor.IncentiveAxisSupervisorException e2)
                    {
                        // Error occurred?
                        MessageBox.Show("btn_Stop_Click->" + e2.ToString());
                    }
                }
                // Now walk through the array list of axis that are running threads and wait for them to finish shutting down.
                foreach (Controller.Axis axis in axisList)
                {
                    try
                    {
                        while (true)
                        {
                            // If axis thread is still alive then must wait...
                            if (((Thread)axis.userObject1).IsAlive)
                            {
                                Thread.Sleep(50);
                                // All UI messages
                                Application.DoEvents();
                            }
                            else
                            {
                                // It is OK to close the axis connection since the thread is not active anymore.
                                axis.closeConnection();
                                break;
                            }
                        }
                    }
                    catch (Controller.Axis.IncentiveAxisException e2)
                    {
                        // Error occurred?
                        MessageBox.Show("btn_Stop_Click->" + e2.ToString());
                    }
                    catch (System.NullReferenceException)
                    {
                        // userObject1 is probably null?
                        axis.closeConnection();
                    }
                }
            }
            // Set we are disconnected
            connected_plc = false;
            Thread.Sleep(1000);  // Ensure threads abort...
            updateStatus("Press Start button to run test...");
            abortThread = false;
            chk_Monitor.Enabled = true;
            chk_Monitor.Refresh();
            axisList.Clear();  // Existing are not needed
        }
        /// <summary>
        /// Handles the Click event of the btn_Stop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            disconnectFromIncentiveApplication();
            btn_Start.Enabled = true;  // Allow to start again...
            btn_Start.Refresh();

        }

        /// <summary>
        /// Handles the Click event of the btn_UpdatePosition control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btn_UpdatePosition_Click(object sender, EventArgs e)
        {
            changeOccurred = true;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chk_EnablePosition control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void chk_EnablePosition_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_EnablePosition.Checked)
            {
                chk_Monitor.Checked = true;
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the Click event of the btn_Adjustment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btn_Adjustment_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chk_AxisOnly control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void chk_AxisOnly_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the Click event of the btnStartIncentive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnStartIncentive_Click(object sender, EventArgs e)
        {
            // get ready to run again or for first time
            testplc = null;
            axisSupervisor = null;
            RuntimeSystem = null;
            testplc = new Controller.PLCLogic();    // Connection to PLC Logic controller stored here
            axisSupervisor = new Controller.AxisSupervisor();   // Connection to overall axis supervisor, typically used in EtherCAT_Master standalone mode
            RuntimeSystem = new CTC_Incentive.RuntimeManagement(1);
            RuntimeSystem.updateCurrentStateInformation(hostNetworkPath);

            axisList.Clear();
            abortThread = false;
            if (!RuntimeSystem.isRunning())
            {
                // Now wait for it to be running...
                btnIndicator.BackColor = Color.Yellow;
                btnIndicator.Text = "Wait for Running";
                btnStartIncentive.Enabled = false;
                btnStopIncentive.Enabled = false;
                btnIndicator.Refresh();
                btnStopIncentive.Refresh();
                btnStartIncentive.Refresh();
                // If chk_AxisOnly box is checked then we are running just the EtherCAT API and not QB PLC Logic node
                if (RuntimeSystem.startIncentive(10000, chk_AxisOnly.Checked, false))  // Dual core mode
                {
                    while (true)
                    {
                        if (RuntimeSystem.isRunning())
                        {
                            // it is running now
                            break;
                        }
                        Application.DoEvents();
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    MessageBox.Show("Start error, check INtime Menu status.");
                    return;
                }

                // Now that we are running lets do the handle unmapping
            }
            // Already running
            waitForOperational(30000);
        }

        /// <summary>
        /// Handles the Click event of the btnStopIncentive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnStopIncentive_Click(object sender, EventArgs e)
        {
            if (RuntimeSystem.isRunning())
            {
                btnIndicator.BackColor = Color.Yellow;
                btnIndicator.Text = "Stopping Application";
                btnStartIncentive.Enabled = false;
                btnStopIncentive.Enabled = false;
                btnIndicator.Refresh();
                btnStopIncentive.Refresh();
                btnStartIncentive.Refresh();
                // See if have any threads attached and shut them down if are.
                disconnectFromIncentiveApplication();
                // Warn about to stop Incentive
                btnIndicator.BackColor = Color.Yellow;
                btnIndicator.Text = "Stopping Incentive";
                btnStartIncentive.Enabled = false;
                btnStopIncentive.Enabled = false;
                btnIndicator.Refresh();
                btnStopIncentive.Refresh();
                btnStartIncentive.Refresh();

                // Reset some parameters for next startup...
                numAxis = 0;                                // Global for number of axis found available on the PLC
                numOutputs = 0;                             // Global for number of outputs found available on the PLC
                ecat_online_status = 0;                     // EtherCAT online status variable monitored by a background thread to ensure still online.

                // ******** Optional Position test parameters
                changeOccurred = false;                    // When the Position Update button is clicked we set this
                Maxspeed = 0;
                Position = 0;
                Accel = 0;
                Decel = 0;

                connected_plc = false;                     // Set when connected to PLC and registers are available.

                //                RuntimeSystem.updateCurrentStateInformation();  // If any question as to what is running you can always call this to update status

                if (!RuntimeSystem.stopIncentive(10000)) // 10 second timeout
                {
                    MessageBox.Show("Stop error, check INtime Menu status.");
                    return;
                }

            }
            btnIndicator.BackColor = Color.Red;
            btnIndicator.Text = "Stopped";
            btnStartIncentive.Enabled = true;
            btnStopIncentive.Enabled = false;
            btn_Start.Enabled = false;
            btn_Stop.Enabled = false;
            btn_Start.Refresh();
            btn_Stop.Refresh();

            btnIndicator.Refresh();
            btnStopIncentive.Refresh();
            btnStartIncentive.Refresh();
            testplc = null;
            axisSupervisor = null;
            RuntimeSystem = null;
        }


        /// <summary>
        /// Waits for EtherCAT operational state.
        /// </summary>
        /// <param name="ms_timeout">Max time in milliseconds to wait.</param>
        /// <returns><c>true</c> if EtherCAT Operational, <c>false</c> otherwise.</returns>
        bool waitForOperational(int ms_timeout)
        {
            // Get the latest state of execution for the processes
            updateSystemState();
            // Now lets wait for EtherCAT to be operational if things are running
            if (RuntimeSystem.Plclogic_started)
            {
                // PLC logic is running so lets wait for operational
                while (ms_timeout > 0)
                {
                    try
                    {
                        // If connection is already present this will simply use the existing connection and return true so make sure open.
                        if (testplc.openConnection(hostNetworkPath + plcName, "P" + ourSpecialName, true, true))  // Name of process, unique name of plc_req mailbox, automatically gets resources and any symbols available.
                        {
                            while (ms_timeout > 0)
                            {
                                // Test for EtherCAT Operational the designated amount of time.
                                if (testplc.checkEtherCAT(ms_timeout, 1))  // Will block for ms_timeout amount of time.
                                {
                                    // We are running, all set to continue
                                    btnIndicator.BackColor = Color.Green;
                                    btnIndicator.Text = "ECAT Operational";
                                    btnIndicator.Refresh();
                                    btnStartIncentive.Enabled = false;
                                    btnStopIncentive.Enabled = true;
                                    btn_Start.Enabled = true;
                                    btn_Stop.Enabled = false;
                                    btn_Start.Refresh();
                                    btn_Stop.Refresh();
                                    ms_timeout = 0;
                                    return true;
                                }
                                else
                                {
                                    // Timed out, not operational
                                    ms_timeout = 0;
                                    return false;
                                }
                            }
                            if (ms_timeout == 0)
                            {
                                break;  // Give up
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Not running fully yet
                    }
                    // Sleep for a second then try again
                    Thread.Sleep(1000);
                    ms_timeout -= 1000;
                }
                return false;
            }
            else if (RuntimeSystem.EtherCAT_started)
            {
                // Only running CTECAT_1, EtherCAT at the moment.
                while (ms_timeout > 0)
                {
                    try
                    {
                        // If connection is already present this will simply use the existing connection and return true so make sure open.
                        if (axisSupervisor.openConnection(hostNetworkPath + ecatName, "S" + ourSpecialName))  // Name of process, unique name of axis_req mailbox
                        {
                            while (ms_timeout > 0)
                            {
                                Controller.AxisSupervisor.SCAN_STATES status = axisSupervisor.scanning;
                                if (status == Controller.AxisSupervisor.SCAN_STATES.RUNNING)
                                {
                                    ecat_online_status = 1;
                                }
                                else
                                {
                                    ecat_online_status = 0;
                                }
                                if (ecat_online_status == 1)
                                {
                                    // We are running, all set to continue
                                    btnIndicator.BackColor = Color.Green;
                                    btnIndicator.Text = "ECAT Operational";
                                    btnIndicator.Refresh();
                                    btnStartIncentive.Enabled = false;
                                    btnStopIncentive.Enabled = true;
                                    btn_Start.Enabled = true;
                                    btn_Stop.Enabled = false;
                                    btn_Start.Refresh();
                                    btn_Stop.Refresh();
                                    ms_timeout = 0;
                                    // Up and running
                                    return true;
                                }
                                else
                                {
                                    // timed out, not operational
                                    Application.DoEvents();
                                    Thread.Sleep(100);
                                    ms_timeout -= 100;
                                    continue;
                                }
                            }
                            return false;       // failed, timed out
                        }
                    }
                    catch (Exception)
                    {
                        // not running fully yet
                    }
                    // Sleep for a second then try again
                    Thread.Sleep(1000);
                    ms_timeout -= 1000;
                }
                return false;
            }
            return false;
        }

        bool neverInitialized = true;  // Only want to invoke this once
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (neverInitialized)
            {
                neverInitialized = false;
                // Find out current state of Incentive in case user started manually.  Note that if in the middle of starting automatically
                // you may have to update this status again as it is only going to detect current state.
                btn_onlineSlave.Enabled = false;
                btn_offlineSlave.Enabled = false;

                waitForOperational(30000);
//                btn_Start.Enabled = true;
//                btn_Start.Refresh();
                btn_onlineSlave.Enabled = false;
                btn_offlineSlave.Enabled = true;


            }
        }


        private void btn_onlineSlave_Click(object sender, EventArgs e)
        {
            try
            {
                if (axisSupervisor.openConnection(hostNetworkPath + ecatName, "S" + ourSpecialName))  // Name of process, unique name of axis_req mailbox
                {
                    // Was able to open connection or already exists
                    axisSupervisor.Command_timout_ms = 30000;  // Needs to be as long as slew stop can take.
                    try
                    {
                        int[] slaves = new int[10];  // Max is 10
                        char[] separatingChars = {','};
                        string[] nodes = txt_Slaves.Text.Split(separatingChars, StringSplitOptions.None);
                        if ((nodes.Length == 0) || (nodes.Length > 10))
                        {
                            MessageBox.Show("Bad number of slaves, 10 maximum.\r\n");
                            return;
                        }
                        for (int i = 0; i != nodes.Length;i++)
                        {
                            slaves[i] = Convert.ToInt32(nodes[i]);
                        }
                        if (axisSupervisor.onlineSlaves(slaves, nodes.Length, true))
                        {
                            MessageBox.Show("Online attempt succeeded.\r\n");
                            btn_onlineSlave.Enabled = false;
                            btn_offlineSlave.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Online attempt failed1.\r\n");
                        }
                    }
                    catch (Controller.AxisSupervisor.IncentiveAxisSupervisorException e2)
                    {
                        int slave = ((int)e2.ErrCode >> 16) & 0x00ff;
                        Controller.Axis.MOTION_FAULTS errorCode = (Controller.Axis.MOTION_FAULTS)((int)e2.ErrCode & 0x0000ffff);
                        MessageBox.Show("Online attempt failed starting with slave #" + slave + ", error code: " + errorCode + ".\r\n");
                     }
                    catch (Exception)
                    {
                        MessageBox.Show("Online attempt failed2.\r\n");
                    }
                }
                else
                {
                    MessageBox.Show("Online open connection attempt failed1.\r\n");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Online open connection attempt failed2.\r\n");
            }

        }

        private void btn_offlineSlave_Click(object sender, EventArgs e)
        {
            try
            {
                if (axisSupervisor.openConnection(hostNetworkPath + ecatName, "S" + ourSpecialName))  // Name of process, unique name of axis_req mailbox
                {
                    // Was able to open connection or already exists
                    axisSupervisor.Command_timout_ms = 30000;  // Needs to be as long as slew stop can take.
                    int lastSlave = 0;
                    try
                    {
                        int[] slaves = new int[10];  // Max is 10
                        char[] separatingChars = {','};
                        string[] nodes = txt_Slaves.Text.Split(separatingChars, StringSplitOptions.None);
                        if ((nodes.Length == 0) || (nodes.Length > 10))
                        {
                            MessageBox.Show("Bad number of slaves, 10 maximum.\r\n");
                            return;
                        }
                        for (int i = 0; i != nodes.Length;i++)
                        {
                            slaves[i] = Convert.ToInt32(nodes[i]);
                        }
                        if (axisSupervisor.offlineSlaves(slaves,nodes.Length,true, ref lastSlave))  // slew if this is an axis
                        {
                            MessageBox.Show("Offline attempt succeeded.\r\n");
                            btn_onlineSlave.Enabled = true;
                            btn_offlineSlave.Enabled = false;
                        }
                        else
                        {
                            MessageBox.Show("Offline attempt failed1, last slave services = " + lastSlave + ".\r\n");
                        }
                    }
                    catch (Controller.AxisSupervisor.IncentiveAxisSupervisorException e2)
                    {
                        MessageBox.Show("Offline attempt failed, last slave = " + lastSlave + ", error code: " + (Controller.Axis.MOTION_FAULTS)(e2.ErrCode) + ".\r\n");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Offline attempt failed2.\r\n");
                    }
                }
                else
                {
                    MessageBox.Show("Offline open connection attempt failed1.\r\n");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Offline open connection attempt failed2.\r\n");
            }
         }

        private void chk_QBDownload_CheckedStateChanged(object sender, EventArgs e)
        {
            try
            {
                // Checked state changed so set present state, use exception in case there is no connection
                testplc.QB_Downloading(chk_QBDownload.Checked);
            }
            catch (Exception)
            {
                // Failed
            }
        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
