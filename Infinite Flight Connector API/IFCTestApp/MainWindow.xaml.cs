<<<<<<< HEAD
﻿using Fds.IFAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IFCTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IFConnectorClient client = new IFConnectorClient();

        public MainWindow()
        {
            InitializeComponent();

            airplaneStateGrid.DataContext = null;
            airplaneStateGrid.DataContext = new APIAircraftState();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            client.Connect();

            client.CommandReceived += client_CommandReceived;

            Task.Run(() =>
            {

                while (true)
                {
                    try
                    {
                        client.SendCommand(new APICall { Command = "Airplane.GetState" });

                        Thread.Sleep(2000);

                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        void client_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => 
            {
                var type = Type.GetType(e.Response.Type);

                if (type == typeof(APIAircraftState))
                {
                    var state = Serializer.DeserializeJson<APIAircraftState>(e.CommandString);

                    airplaneStateGrid.DataContext = null;
                    airplaneStateGrid.DataContext = state;
                }
                else if (type == typeof(GetValueResponse))
                {
                    var state = Serializer.DeserializeJson<GetValueResponse>(e.CommandString);

                    Console.WriteLine("{0} -> {1}", state.Parameters[0].Name, state.Parameters[0].Value);
                }
            }));
            
        }

        private void toggleBrakesButton_Click(object sender, RoutedEventArgs e)
        {
            //client.SetValue("Aircraft.Systems.Autopilot.EnableHeading", "True");
//            
        }
        
        private void toggleBrakesButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            client.ExecuteCommand("Commands.Brakes", new CallParameter[] { new CallParameter { Name = "KeyAction", Value = "Down" } } );
        }

        private void toggleBrakesButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            client.ExecuteCommand("Commands.Brakes", new CallParameter[] { new CallParameter { Name = "KeyAction", Value = "Up" } });
        }

        private void parkingBrakeButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.ParkingBrakes");
            client.GetValue("Aircraft.State.IsBraking");
        }

        private void prevCameraButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.PrevCamera");
        }

        private void nextCameraButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.NextCamera");
        }

        private void autopilotHeadingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            client.ExecuteCommand("Commands.Autopilot.SetHeading", new CallParameter[] { new CallParameter { Name = "Heading", Value = autopilotHeadingSlider.Value.ToString() } });            
        }

        private void setGearStateButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.LandingGear");
        }

        private void flapsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            client.ExecuteCommand("Commands.Aircraft.SetFlapState", new CallParameter[] { new CallParameter { Name = "Heading", Value = flapsSlider.Value.ToString() } });
        }

        private void addFplItems_Click(object sender, RoutedEventArgs e)
        {
            var items = flightPlanItemsTextBlock.Text.Split(',');
            client.ExecuteCommand("Commands.FlightPlan.AddWaypoints", items.Select(x => new CallParameter { Name = "WPT", Value = x }).ToArray());
        }

        private void clearFplItems_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.FlightPlan.Clear");
        }

        private void activateLegButton_Click(object sender, RoutedEventArgs e)        
        {
            client.ExecuteCommand("Commands.FlightPlan.ActivateLeg", new CallParameter[] { new CallParameter { Name = "Index", Value = "3" } });
        }

        private void aileronsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            client.ExecuteCommand("NetworkJoystick.SetAxisValue", new CallParameter[] 
            { 
                new CallParameter 
                {
                    Name = "0", // axis index
                    Value = aileronsSlider.Value.ToString()
                } 
            });
        }
        
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;

            client.ExecuteCommand("NetworkJoystick.SetButtonState", new CallParameter[] 
            {
                new CallParameter 
                { 
                    Name = button.Content.ToString(),  // button index
                    Value = "Down"
                }
            });
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;

            client.ExecuteCommand("NetworkJoystick.SetButtonState", new CallParameter[] 
            {
                new CallParameter 
                { 
                    Name = button.Content.ToString(),  // button index
                    Value = "Up"
                }
            });
        }

        private void POVButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            var type = button.Content.ToString();

            var xValue = 0;
            var yValue = 0;

            if (type == "Up")
            {
                xValue = 0;
                yValue = 1;
            }
            else if (type == "Down")
            {
                xValue = 0;
                yValue = -1;
            }
            else if (type == "Left")
            {
                xValue = -1;
                yValue = 0;
            }
            else if (type == "Right")
            {
                xValue = 1;
                yValue = 0;
            }

            client.ExecuteCommand("NetworkJoystick.SetPOVState", new CallParameter[] 
                {
                    new CallParameter 
                    { 
                        Name = "X",
                        Value = xValue.ToString()
                    },
                    new CallParameter 
                    { 
                        Name = "Y",
                        Value = yValue.ToString()
                    }
                });


        }

        private void POVButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            var type = button.Content.ToString();

            var xValue = 0;
            var yValue = 0;

            if (type == "Up")
            {
                xValue = 0;
                yValue = 0;
            }
            else if (type == "Down")
            {
                xValue = 0;
                yValue = 0;
            }
            else if (type == "Left")
            {
                xValue = 0;
                yValue = 0;
            }
            else if (type == "Right")
            {
                xValue = 0;
                yValue = 0;
            }

            client.ExecuteCommand("NetworkJoystick.SetPOVState", new CallParameter[] 
                {
                    new CallParameter 
                    { 
                        Name = "X",
                        Value = xValue.ToString()
                    },
                    new CallParameter 
                    { 
                        Name = "Y",
                        Value = yValue.ToString()
                    }
                });
        }
    }
}
=======
﻿using Fds.IFAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IFCTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IFConnectorClient client = new IFConnectorClient();

        public MainWindow()
        {
            InitializeComponent();

            airplaneStateGrid.DataContext = null;
            airplaneStateGrid.DataContext = new APIAircraftState();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            client.Connect();

            client.CommandReceived += client_CommandReceived;

            Task.Run(() =>
            {

                while (true)
                {
                    try
                    {
                        client.SendCommand(new APICall { Command = "Airplane.GetState" });

                        Thread.Sleep(2000);

                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        void client_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => 
            {
                var type = Type.GetType(e.Response.Type);

                if (type == typeof(APIAircraftState))
                {
                    var state = Serializer.DeserializeJson<APIAircraftState>(e.CommandString);

                    airplaneStateGrid.DataContext = null;
                    airplaneStateGrid.DataContext = state;
                }
                else if (type == typeof(GetValueResponse))
                {
                    var state = Serializer.DeserializeJson<GetValueResponse>(e.CommandString);

                    Console.WriteLine("{0} -> {1}", state.Parameters[0].Name, state.Parameters[0].Value);
                }
            }));
            
        }

        private void toggleBrakesButton_Click(object sender, RoutedEventArgs e)
        {
            //client.SetValue("Aircraft.Systems.Autopilot.EnableHeading", "True");
//            
        }
        
        private void toggleBrakesButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            client.ExecuteCommand("Commands.Brakes", new CallParameter[] { new CallParameter { Name = "KeyAction", Value = "Down" } } );
        }

        private void toggleBrakesButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            client.ExecuteCommand("Commands.Brakes", new CallParameter[] { new CallParameter { Name = "KeyAction", Value = "Up" } });
        }

        private void parkingBrakeButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.ParkingBrakes");
            client.GetValue("Aircraft.State.IsBraking");
        }

        private void prevCameraButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.PrevCamera");
        }

        private void nextCameraButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.NextCamera");
        }

        private void autopilotHeadingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            client.ExecuteCommand("Commands.Autopilot.SetHeading", new CallParameter[] { new CallParameter { Name = "Heading", Value = autopilotHeadingSlider.Value.ToString() } });            
        }

        private void setGearStateButton_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.LandingGear");
        }

        private void flapsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            client.ExecuteCommand("Commands.Aircraft.SetFlapState", new CallParameter[] { new CallParameter { Name = "Heading", Value = flapsSlider.Value.ToString() } });
        }

        private void addFplItems_Click(object sender, RoutedEventArgs e)
        {
            var items = flightPlanItemsTextBlock.Text.Split(',');
            client.ExecuteCommand("Commands.FlightPlan.AddWaypoints", items.Select(x => new CallParameter { Name = "WPT", Value = x }).ToArray());
        }

        private void clearFplItems_Click(object sender, RoutedEventArgs e)
        {
            client.ExecuteCommand("Commands.FlightPlan.Clear");
        }

        private void activateLegButton_Click(object sender, RoutedEventArgs e)        
        {
            client.ExecuteCommand("Commands.FlightPlan.ActivateLeg", new CallParameter[] { new CallParameter { Name = "Index", Value = "3" } });
        }
    }
}
>>>>>>> rebasing
