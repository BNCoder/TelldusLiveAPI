# TelldusLiveAPI
TelldusLiveAPI web API wrapper

This is a wrapper off the Telldus Live API webserveses.

I chose en file approch so that it should be easy to add to a project just add the file TelldusLiveAPI.cs that is in the repository.

Hear is some example code to get you started.

using System;
using TelldusLiveAPI;

namespace TelldusLiveAPI
{
  class Program
  {
    private const string PublicKey = "";
    private const string PrivateKey = "";
    private const string Token = "";
    private const string TokenSecret = "";

    static void Main(string[] args)
    {
      TelldusLive live = new TelldusLive();
      if ( !live.Login(PublicKey, PrivateKey,Token,TokenSecret).success)          
      {
        Console.WriteLine("Error Loginfailed");
      }
      var clients = live.ClientsList();
      var devices = live.DevicesList();
      var events = live.EventsList();
      var phones = live.UserListPhones();
      var schedulers = live.SchedulerJobList();
      var sensors = live.SensorsList(false,true,true);
     }
  }
}      
